using DiyAuth.BackendEntities;
using DiyAuth.Models;
using DiyAuth.Utility;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace DiyAuth.BackendProviders
{
	public class AzureBackendProvider : IBackendProvider
	{
		public string ConnectionString { get; set; }
		public string IdentityTableName { get; set; } = Defaults.IdentityTableName;
		public string TokenTableName { get; set; } = Defaults.TokenTableName;

		public CloudStorageAccount StorageAccount { get; private set; }
		public CloudTable IdentityTable { get; private set; }
		public CloudTable TokenTable { get; private set; }
		public CloudTableClient TableClient { get; private set; }

		public AzureBackendProvider(string connectionString)
		{
			this.ConnectionString = connectionString;
		}

		public static async Task<AzureBackendProvider> Create(string connectionString)
		{
			var provider = new AzureBackendProvider(connectionString);
			await provider.Initialize().ConfigureAwait(false);
			return provider;
		}

		public async Task Initialize()
		{
			this.StorageAccount = CloudStorageAccount.Parse(this.ConnectionString);
			this.TableClient = this.StorageAccount.CreateCloudTableClient();

			this.IdentityTable = this.TableClient.GetTableReference(this.IdentityTableName);
			this.TokenTable = this.TableClient.GetTableReference(this.TokenTableName);

			await this.IdentityTable.CreateIfNotExistsAsync().ConfigureAwait(false);
			await this.TokenTable.CreateIfNotExistsAsync().ConfigureAwait(false);
		}

		public async Task<AuthenticateResult> Authenticate(string token)
		{
			try
			{
				var retrieveOperation = TableOperation.Retrieve(Defaults.PartitionName, token);
				var retrievedResult = await this.TokenTable.ExecuteAsync(retrieveOperation).ConfigureAwait(false);
				var retrievedEntity = (AzureTokenEntity)retrievedResult?.Result;
				return new AuthenticateResult
				{
					Success = true,
					IdentityId = retrievedEntity.IdentityId
				};
			}
			catch (StorageException)
			{
				return new AuthenticateResult
				{
					Success = false
				};
			}
		}

		public async Task<AuthorizeResult> Authorize(string emailAddress, string password)
		{
			try
			{
				var retrieveOperation = TableOperation.Retrieve(Defaults.PartitionName, emailAddress);
				var retrievedResult = await this.TokenTable.ExecuteAsync(retrieveOperation).ConfigureAwait(false);
				var retrievedEntity = (AzureIdentityEntity)retrievedResult?.Result;
				return new AuthorizeResult
				{
					Success = true,
					Token = Security.GenerateToken()
				};
			}
			catch (StorageException)
			{
				return new AuthorizeResult
				{
					Success = false
				};
			}
		}

		public async Task<CreateIdentityResult> CreateIdentity(string emailAddress, string password)
		{
			try
			{
				var perUserSalt = Security.GeneratePerUserSalt();
				var hashedPassword = Security.GeneratePasswordHash(password, perUserSalt);
				var identityEntity = new AzureIdentityEntity()
				{
					IdentityId = Guid.NewGuid(),
					EmailAddress = emailAddress,
					PerUserSalt = perUserSalt,
					HashedPassword = hashedPassword
				};

				var createOperation = TableOperation.Insert(identityEntity);
				var result = await this.TokenTable.ExecuteAsync(createOperation).ConfigureAwait(false);

				return new CreateIdentityResult
				{
					Success = true,
					IdentityId = identityEntity.IdentityId
				};
			}
			catch (StorageException)
			{
				return new CreateIdentityResult
				{
					Success = false
				};
			}
		}

		public async Task<ResetPasswordResult> ResetPassword(string emailAddress, string oldPassword, string newPassword)
		{
			try
			{
				var authorizeResult = await Authorize(emailAddress, oldPassword).ConfigureAwait(false);
				if (!authorizeResult.Success)
				{
					return new ResetPasswordResult
					{
						Success = false
					};
				}

				var perUserSalt = Security.GeneratePerUserSalt();
				var hashedPassword = Security.GeneratePasswordHash(newPassword, perUserSalt);

				var retrieveOperation = TableOperation.Retrieve(Defaults.PartitionName, emailAddress);
				var retrievedResult = await this.TokenTable.ExecuteAsync(retrieveOperation).ConfigureAwait(false);
				var retrievedEntity = (AzureIdentityEntity)retrievedResult?.Result;

				retrievedEntity.PerUserSalt = perUserSalt;
				retrievedEntity.HashedPassword = hashedPassword;

				var setOperation = TableOperation.InsertOrReplace(retrievedEntity);
				var result = await this.TokenTable.ExecuteAsync(setOperation).ConfigureAwait(false);

				return new ResetPasswordResult
				{
					Success = true
				};
			}
			catch (StorageException)
			{
				return new ResetPasswordResult
				{
					Success = false
				};
			}
		}

		public Task GenerateTokenForIdentity()
		{
			throw new NotImplementedException();
		}

		public Task DeleteExpiredTokens()
		{
			throw new NotImplementedException();
		}

		public Task DeleteToken()
		{
			throw new NotImplementedException();
		}

		public Task DeleteIdentity()
		{
			throw new NotImplementedException();
		}

		public Task<string> GenerateVerificationToken()
		{
			throw new NotImplementedException();
		}

		public Task VerifyIdentity(string verificationToken)
		{
			throw new NotImplementedException();
		}
	}
}
