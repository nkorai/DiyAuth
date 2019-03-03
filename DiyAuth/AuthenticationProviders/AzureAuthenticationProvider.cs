using DiyAuth.AuthenticationEntities;
using DiyAuth.Models;
using DiyAuth.Utility;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiyAuth.AuthenticationProviders
{
	public class AzureAuthenticationProvider : IAuthenticationProvider
	{
		public string ConnectionString { get; set; }
		public string IdentityTableName { get; set; } = Constants.TableNames.IdentityTableName;
		public string TokenTableName { get; set; } = Constants.TableNames.TokenTableName;

		public CloudStorageAccount StorageAccount { get; private set; }
		public CloudTable IdentityTable { get; private set; }
		public CloudTable TokenTable { get; private set; }
		public CloudTableClient TableClient { get; private set; }

		public AzureAuthenticationProvider(string storageAccountConnectionString)
		{
			this.ConnectionString = storageAccountConnectionString;
		}

		public static async Task<AzureAuthenticationProvider> Create(string connectionString)
		{
			var provider = new AzureAuthenticationProvider(connectionString);
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
				var retrieveOperation = TableOperation.Retrieve<AzureTokenEntity>(Constants.PartitionNames.DefaultPartitionName, token);
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
				// Check to see if email exists
				var retrieveOperation = TableOperation.Retrieve<AzureIdentityForeignKeyEntity>(Constants.PartitionNames.IdentityForeignKeyPartitionName, emailAddress);
				var retrievedResult = await this.IdentityTable.ExecuteAsync(retrieveOperation).ConfigureAwait(false);
				var retrievedEntity = (AzureIdentityForeignKeyEntity)retrievedResult?.Result;
				if (retrievedEntity == null)
				{
					return new AuthorizeResult
					{
						Success = false
					};
				}

				// Retrieve IdentityEntity
				var retrieveIdentityOperation = TableOperation.Retrieve<AzureIdentityEntity>(Constants.PartitionNames.DefaultPartitionName, retrievedEntity.IdentityId.ToString());
				var retrievedIdentityResult = await this.IdentityTable.ExecuteAsync(retrieveIdentityOperation).ConfigureAwait(false);
				var retrievedIdentityEntity = (AzureIdentityEntity)retrievedIdentityResult?.Result;

				// Check if provided password is valid
				var passwordMatches = Security.PasswordMatches(retrievedIdentityEntity.PerUserSalt, password, retrievedIdentityEntity.HashedPassword);
				if (!passwordMatches)
				{
					return new AuthorizeResult
					{
						Success = false
					};
				}

				// Genearte, store and return token
				var token = Security.GenerateToken();
				var tokenEntity = new AzureTokenEntity
				{
					IdentityId = retrievedEntity.IdentityId,
					Token = token
				};

				var createOperation = TableOperation.Insert(tokenEntity);
				var result = await this.TokenTable.ExecuteAsync(createOperation).ConfigureAwait(false);
				return new AuthorizeResult
				{
					Success = true,
					IdentityId = retrievedEntity.IdentityId,
					Token = token
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
				// Identity generation
				var perUserSalt = Security.GeneratePerUserSalt();
				var hashedPassword = Security.GeneratePasswordHash(password, perUserSalt);
				var identityEntity = new AzureIdentityEntity()
				{
					IdentityId = Guid.NewGuid(),
					EmailAddress = emailAddress,
					PerUserSalt = perUserSalt,
					HashedPassword = hashedPassword
				};

				var createIdentityOperation = TableOperation.Insert(identityEntity);

				// Identity foreign key generation
				var foreignKey = new AzureIdentityForeignKeyEntity
				{
					IdentityId = identityEntity.IdentityId,
					EmailAddress = emailAddress,
				};

				var createForeignKeyOperation = TableOperation.Insert(foreignKey);

				// Insert identity information
				await this.IdentityTable.ExecuteAsync(createForeignKeyOperation).ConfigureAwait(false); // This insert first to ensure there isn't a key conflict
				await this.IdentityTable.ExecuteAsync(createIdentityOperation).ConfigureAwait(false);

				// Token generation
				var token = Security.GenerateToken();
				var tokenEntity = new AzureTokenEntity
				{
					IdentityId = identityEntity.IdentityId,
					Token = token
				};

				var createTokenOperation = TableOperation.Insert(tokenEntity);
				await this.TokenTable.ExecuteAsync(createTokenOperation).ConfigureAwait(false);

				return new CreateIdentityResult
				{
					Success = true,
					IdentityId = identityEntity.IdentityId,
					Token = tokenEntity.Token
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

		public async Task<bool> CheckIdentityExists(string emailAddress)
		{
			try
			{
				var retrieveOperation = TableOperation.Retrieve<AzureIdentityForeignKeyEntity>(Constants.PartitionNames.IdentityForeignKeyPartitionName, emailAddress);
				var retrievedResult = await this.IdentityTable.ExecuteAsync(retrieveOperation).ConfigureAwait(false);
				var retrievedEntity = (AzureIdentityForeignKeyEntity)retrievedResult?.Result;
				if (retrievedEntity == null)
				{
					return false;
				}

				return true;
			}
			catch (StorageException)
			{
				return false;
			}
		}

		public async Task<ResetPasswordResult> ChangePassword(string emailAddress, string oldPassword, string newPassword)
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

				var retrieveOperation = TableOperation.Retrieve<AzureIdentityEntity>(Constants.PartitionNames.DefaultPartitionName, emailAddress);
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

		public async Task<AuthorizeResult> GenerateTokenForIdentityId(Guid identityId)
		{
			var retrieveOperation = TableOperation.Retrieve<AzureIdentityEntity>(this.IdentityTableName, identityId.ToString());
			var retrievedResult = await this.IdentityTable.ExecuteAsync(retrieveOperation).ConfigureAwait(false);
			var retrievedEntity = (AzureIdentityEntity)retrievedResult?.Result;

			var token = Security.GenerateToken();
			var tokenEntity = new AzureTokenEntity
			{
				IdentityId = retrievedEntity.IdentityId,
				Token = token
			};

			var createTokenOperation = TableOperation.Insert(tokenEntity);
			await this.TokenTable.ExecuteAsync(createTokenOperation).ConfigureAwait(false);

			return new AuthorizeResult
			{
				Success = true, 
				Token = token,
				IdentityId = identityId
			};
		}

		public async Task DeleteToken(string token)
		{
			var deleteEntity = new AzureTokenEntity
			{
				PartitionKey = Constants.PartitionNames.DefaultPartitionName,
				RowKey = token
			};

			var deleteOperation = TableOperation.Delete(deleteEntity);
			var result = await this.TokenTable.ExecuteAsync(deleteOperation).ConfigureAwait(false);
		}

		public async Task DeleteIdentity(Guid identityId)
		{
			// Retrieve identity entity
			var retrieveOperation = TableOperation.Retrieve<AzureIdentityEntity>(Constants.PartitionNames.DefaultPartitionName, identityId.ToString());
			var retrievedResult = await this.IdentityTable.ExecuteAsync(retrieveOperation).ConfigureAwait(false);
			var retrievedEntity = (AzureIdentityEntity)retrievedResult?.Result;

			// Delete both foreign key as well as identity record
			var deleteIdentityEntity = new AzureIdentityEntity
			{
				PartitionKey = Constants.PartitionNames.DefaultPartitionName,
				RowKey = identityId.ToString()
			};

			var deleteForeignKeyEntity = new AzureIdentityForeignKeyEntity
			{
				PartitionKey = Constants.PartitionNames.IdentityForeignKeyPartitionName,
				RowKey = retrievedEntity.EmailAddress
			};

			var deleteEntityOperation = TableOperation.Delete(deleteIdentityEntity);
			var deleteForeignKeyOperation = TableOperation.Delete(deleteForeignKeyEntity);

			var entityDeleteResult = await this.TokenTable.ExecuteAsync(deleteEntityOperation).ConfigureAwait(false);
			var foreignKeyDeleteResult = await this.TokenTable.ExecuteAsync(deleteForeignKeyOperation).ConfigureAwait(false);
		}
	}
}
