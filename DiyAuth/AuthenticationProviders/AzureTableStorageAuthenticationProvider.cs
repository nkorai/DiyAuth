using DiyAuth.AuthenticationEntities;
using DiyAuth.AuthenticationEntities.Azure;
using DiyAuth.EmailProviders;
using DiyAuth.Models;
using DiyAuth.Utility;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiyAuth.AuthenticationProviders
{
	public class AzureTableStorageAuthenticationProvider : IAuthenticationProvider
	{
		// Interface properties
		public IEmailProvider EmailProvider { get; set; }

		// Azure specific properties
		public string ConnectionString { get; set; }
		public string IdentityTableName { get; set; } = Constants.TableNames.IdentityTable;
		public string TokenTableName { get; set; } = Constants.TableNames.TokenTable;
		public string VerificationTokenTableName { get; set; } = Constants.TableNames.VerificationTokenTable;

		public CloudStorageAccount StorageAccount { get; private set; }
		public CloudTable IdentityTable { get; private set; }
		public CloudTable TokenTable { get; private set; }
		public CloudTable VerificationTokenTable { get; private set; }
		public CloudTableClient TableClient { get; private set; }

		public AzureTableStorageAuthenticationProvider(string storageAccountConnectionString)
		{
			this.ConnectionString = storageAccountConnectionString;
		}

		public static async Task<AzureTableStorageAuthenticationProvider> Create(string connectionString)
		{
			var provider = new AzureTableStorageAuthenticationProvider(connectionString);
			await provider.Initialize().ConfigureAwait(false);
			return provider;
		}

		public async Task Initialize(CancellationToken cancellationToken = default(CancellationToken))
		{
			this.StorageAccount = CloudStorageAccount.Parse(this.ConnectionString);
			this.TableClient = this.StorageAccount.CreateCloudTableClient();

			this.IdentityTable = this.TableClient.GetTableReference(this.IdentityTableName);
			this.TokenTable = this.TableClient.GetTableReference(this.TokenTableName);
			this.VerificationTokenTable = this.TableClient.GetTableReference(this.VerificationTokenTableName);

			await this.IdentityTable.CreateIfNotExistsAsync(null, null, cancellationToken).ConfigureAwait(false);
			await this.TokenTable.CreateIfNotExistsAsync(null, null, cancellationToken).ConfigureAwait(false);
			await this.VerificationTokenTable.CreateIfNotExistsAsync(null, null, cancellationToken).ConfigureAwait(false);
		}

		public void SetEmailProvider(IEmailProvider emailProvider)
		{
			this.EmailProvider = emailProvider;
			emailProvider.AuthenticationProvider = this;
		}

		public async Task<AuthenticateResult> Authenticate(string token, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				var retrieveOperation = TableOperation.Retrieve<AzureTokenEntity>(Constants.PartitionNames.TokenPrimary, token);
				var retrievedResult = await this.TokenTable.ExecuteAsync(retrieveOperation, null, null, cancellationToken).ConfigureAwait(false);
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

		public async Task<AuthorizeResult> Authorize(string emailAddress, string password, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				// Check to see if email exists
				var retrieveOperation = TableOperation.Retrieve<AzureIdentityForeignKeyEntity>(Constants.PartitionNames.EmailAddressToIdentityForeignKey, emailAddress);
				var retrievedResult = await this.IdentityTable.ExecuteAsync(retrieveOperation, null, null, cancellationToken).ConfigureAwait(false);
				var retrievedEntity = (AzureIdentityForeignKeyEntity)retrievedResult?.Result;
				if (retrievedEntity == null)
				{
					return new AuthorizeResult
					{
						Success = false
					};
				}

				// Retrieve IdentityEntity
				var retrieveIdentityOperation = TableOperation.Retrieve<AzureIdentityEntity>(Constants.PartitionNames.IdentityPrimary, retrievedEntity.IdentityId.ToString());
				var retrievedIdentityResult = await this.IdentityTable.ExecuteAsync(retrieveIdentityOperation, null, null, cancellationToken).ConfigureAwait(false);
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

				// Generate, store and return token
				var token = Security.GenerateToken();
				var tokenEntity = new AzureTokenEntity
				{
					IdentityId = retrievedEntity.IdentityId,
					Token = token
				};

				var createOperation = TableOperation.Insert(tokenEntity);
				var result = await this.TokenTable.ExecuteAsync(createOperation, null, null, cancellationToken).ConfigureAwait(false);
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

		public async Task<CreateIdentityResult> CreateIdentity(string emailAddress, string password, CancellationToken cancellationToken = default(CancellationToken))
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
				await this.IdentityTable.ExecuteAsync(createForeignKeyOperation, null, null, cancellationToken).ConfigureAwait(false); // This insert first to ensure there isn't a key conflict
				await this.IdentityTable.ExecuteAsync(createIdentityOperation, null, null, cancellationToken).ConfigureAwait(false);

				// Token generation
				var token = Security.GenerateToken();
				var tokenEntity = new AzureTokenEntity
				{
					IdentityId = identityEntity.IdentityId,
					Token = token
				};

				var createTokenOperation = TableOperation.Insert(tokenEntity);
				await this.TokenTable.ExecuteAsync(createTokenOperation, null, null, cancellationToken).ConfigureAwait(false);

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

		public async Task<bool> CheckIdentityExists(string emailAddress, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				var retrieveOperation = TableOperation.Retrieve<AzureIdentityForeignKeyEntity>(Constants.PartitionNames.EmailAddressToIdentityForeignKey, emailAddress);
				var retrievedResult = await this.IdentityTable.ExecuteAsync(retrieveOperation, null, null, cancellationToken).ConfigureAwait(false);
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

		public async Task<ResetPasswordResult> ChangePassword(Guid identityId, string oldPassword, string newPassword, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				var retrieveOperation = TableOperation.Retrieve<AzureIdentityEntity>(Constants.PartitionNames.IdentityPrimary, identityId.ToString());
				var retrievedResult = await this.IdentityTable.ExecuteAsync(retrieveOperation, null, null, cancellationToken).ConfigureAwait(false);
				var retrievedEntity = (AzureIdentityEntity)retrievedResult?.Result;

				var authorizeResult = await Authorize(retrievedEntity.EmailAddress, oldPassword, cancellationToken).ConfigureAwait(false);
				if (!authorizeResult.Success)
				{
					return new ResetPasswordResult
					{
						Success = false
					};
				}

				var perUserSalt = Security.GeneratePerUserSalt();
				var hashedPassword = Security.GeneratePasswordHash(newPassword, perUserSalt);

				retrievedEntity.PerUserSalt = perUserSalt;
				retrievedEntity.HashedPassword = hashedPassword;

				var setOperation = TableOperation.InsertOrReplace(retrievedEntity);
				var result = await this.IdentityTable.ExecuteAsync(setOperation, null, null, cancellationToken).ConfigureAwait(false);

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

		public async Task<AuthorizeResult> GenerateTokenForIdentityId(Guid identityId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var retrieveOperation = TableOperation.Retrieve<AzureIdentityEntity>(Constants.PartitionNames.IdentityPrimary, identityId.ToString());
			var retrievedResult = await this.IdentityTable.ExecuteAsync(retrieveOperation, null, null, cancellationToken).ConfigureAwait(false);
			var retrievedEntity = (AzureIdentityEntity)retrievedResult?.Result;

			var token = Security.GenerateToken();
			var tokenEntity = new AzureTokenEntity
			{
				IdentityId = retrievedEntity.IdentityId,
				Token = token
			};

			var createTokenOperation = TableOperation.Insert(tokenEntity);
			await this.TokenTable.ExecuteAsync(createTokenOperation, null, null, cancellationToken).ConfigureAwait(false);

			return new AuthorizeResult
			{
				Success = true,
				Token = token,
				IdentityId = identityId
			};
		}

		public async Task DeleteToken(string token, CancellationToken cancellationToken = default(CancellationToken))
		{
			var deleteEntity = new AzureTokenEntity
			{
				PartitionKey = Constants.PartitionNames.TokenPrimary,
				RowKey = token,
				ETag = "*"
			};

			var deleteOperation = TableOperation.Delete(deleteEntity);
			var result = await this.TokenTable.ExecuteAsync(deleteOperation, null, null, cancellationToken).ConfigureAwait(false);
		}

		public async Task DeleteIdentity(Guid identityId, CancellationToken cancellationToken = default(CancellationToken))
		{
			// Retrieve identity entity
			var retrieveOperation = TableOperation.Retrieve<AzureIdentityEntity>(Constants.PartitionNames.IdentityPrimary, identityId.ToString());
			var retrievedResult = await this.IdentityTable.ExecuteAsync(retrieveOperation, null, null, cancellationToken).ConfigureAwait(false);
			var retrievedEntity = (AzureIdentityEntity)retrievedResult?.Result;

			// Delete both foreign key as well as identity record
			var deleteIdentityEntity = new AzureIdentityEntity
			{
				PartitionKey = Constants.PartitionNames.IdentityPrimary,
				RowKey = identityId.ToString(),
				ETag = "*"
			};

			var deleteForeignKeyEntity = new AzureIdentityForeignKeyEntity
			{
				PartitionKey = Constants.PartitionNames.EmailAddressToIdentityForeignKey,
				RowKey = retrievedEntity.EmailAddress,
				ETag = "*"
			};

			// Delete all authentication tokens associated with the Identity
			var query = new TableQuery<AzureTokenEntity>();
			query.Where(TableQuery.CombineFilters(
					TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Constants.PartitionNames.TokenPrimary),
					TableOperators.And,
					TableQuery.GenerateFilterConditionForGuid("IdentityId", QueryComparisons.Equal, identityId)
				)
			);

			TableContinuationToken continuationToken = null;
			do
			{
				var segmentResult = await this.TokenTable.ExecuteQuerySegmentedAsync(query, continuationToken, null, null, cancellationToken);
				var batchOption = new TableBatchOperation();
				foreach (var entity in segmentResult.Results)
				{
					batchOption.Delete(entity);
				}

				if (batchOption.Count > 0)
				{
					await this.TokenTable.ExecuteBatchAsync(batchOption, null, null, cancellationToken).ConfigureAwait(false);
				}

				continuationToken = segmentResult.ContinuationToken;
			} while (continuationToken != null);

			// Delete identity and identity foreign key 
			var deleteEntityOperation = TableOperation.Delete(deleteIdentityEntity);
			var deleteForeignKeyOperation = TableOperation.Delete(deleteForeignKeyEntity);

			var entityDeleteResult = await this.IdentityTable.ExecuteAsync(deleteEntityOperation, null, null, cancellationToken).ConfigureAwait(false);
			var foreignKeyDeleteResult = await this.IdentityTable.ExecuteAsync(deleteForeignKeyOperation, null, null, cancellationToken).ConfigureAwait(false);
		}

		public async Task<IIdentityEntity> GetIdentityById(Guid identityId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var retrieveOperation = TableOperation.Retrieve<AzureIdentityEntity>(Constants.PartitionNames.IdentityPrimary, identityId.ToString());
			var retrievedResult = await this.IdentityTable.ExecuteAsync(retrieveOperation, null, null, cancellationToken).ConfigureAwait(false);
			var retrievedEntity = (AzureIdentityEntity)retrievedResult.Result;
			return retrievedEntity;
		}

		public async Task<IIdentityEntity> GetIdentityByEmail(string emailAddress, CancellationToken cancellationToken = default(CancellationToken))
		{
			// Check to see if email exists
			var retrieveOperation = TableOperation.Retrieve<AzureIdentityForeignKeyEntity>(Constants.PartitionNames.EmailAddressToIdentityForeignKey, emailAddress);
			var retrievedResult = await this.IdentityTable.ExecuteAsync(retrieveOperation, null, null, cancellationToken).ConfigureAwait(false);
			var retrievedEntity = (AzureIdentityForeignKeyEntity)retrievedResult?.Result;
			if (retrievedEntity == null)
			{
				throw new KeyNotFoundException($"The EmailAddress '{emailAddress}' was not found");
			}

			// Retrieve IdentityEntity
			var retrieveIdentityOperation = TableOperation.Retrieve<AzureIdentityEntity>(Constants.PartitionNames.IdentityPrimary, retrievedEntity.IdentityId.ToString());
			var retrievedIdentityResult = await this.IdentityTable.ExecuteAsync(retrieveIdentityOperation, null, null, cancellationToken).ConfigureAwait(false);
			var retrievedIdentityEntity = (AzureIdentityEntity)retrievedIdentityResult?.Result;
			return retrievedIdentityEntity;
		}

		public async Task<string> GenerateVerificationToken(Guid identityId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var verificationToken = Security.GenerateToken();
			var verificationTokenEntity = new AzureVerificationTokenEntity
			{
				VerificationToken = verificationToken,
				IdentityId = identityId
			};

			var createOperation = TableOperation.Insert(verificationTokenEntity);
			var result = await this.TokenTable.ExecuteAsync(createOperation, null, null, cancellationToken).ConfigureAwait(false);
			return verificationToken;
		}

		public async Task<IIdentityEntity> VerifyVerificationToken(string verificationToken, bool deleteTokenOnRetrieval = true, CancellationToken cancellationToken = default(CancellationToken))
		{
			// Check to see if VerificationToken exists
			var retrieveOperation = TableOperation.Retrieve<AzureVerificationTokenEntity>(Constants.PartitionNames.VerificationTokenPrimary, verificationToken);
			var retrievedResult = await this.VerificationTokenTable.ExecuteAsync(retrieveOperation, null, null, cancellationToken).ConfigureAwait(false);
			var retrievedEntity = (AzureVerificationTokenEntity)retrievedResult?.Result;
			if (retrievedEntity == null)
			{
				throw new KeyNotFoundException($"The VerificationToken '{verificationToken}' was not found");
			}

			// Retrieve IdentityEntity
			var retrieveIdentityOperation = TableOperation.Retrieve<AzureIdentityEntity>(Constants.PartitionNames.IdentityPrimary, retrievedEntity.IdentityId.ToString());
			var retrievedIdentityResult = await this.IdentityTable.ExecuteAsync(retrieveIdentityOperation, null, null, cancellationToken).ConfigureAwait(false);
			var retrievedIdentityEntity = (AzureIdentityEntity)retrievedIdentityResult?.Result;

			if (!deleteTokenOnRetrieval)
			{
				return retrievedIdentityEntity;
			}


			// Delete the verification token record
			var deleteVerificationEntity = new AzureVerificationTokenEntity
			{
				PartitionKey = Constants.PartitionNames.IdentityPrimary,
				RowKey = retrievedEntity.VerificationToken.ToString(),
				ETag = "*"
			};

			// Delete identity and identity foreign key 
			var deleteEntityOperation = TableOperation.Delete(deleteVerificationEntity);
			var entityDeleteResult = await this.IdentityTable.ExecuteAsync(deleteEntityOperation, null, null, cancellationToken).ConfigureAwait(false);

			return retrievedIdentityEntity;
		}
	}
}
