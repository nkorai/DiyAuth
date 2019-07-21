using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using DiyAuth.AuthenticationEntities;
using DiyAuth.AuthenticationEntities.AWS;
using DiyAuth.EmailProviders;
using DiyAuth.Models;
using DiyAuth.Utility;
using Newtonsoft.Json;

namespace DiyAuth.AuthenticationProviders
{
	public class AWSDynamoDbAuthenticationProvider : IAuthenticationProvider
	{
		// Interface properties
		public IEmailProvider EmailProvider { get; set; }
		public bool AllowUnverifiedIdentities { get; set; }

		// AWS specific properties
		public string AwsAccessKeyId { get; set; }
		public string AwsSecretAccessKey { get; set; }

		public RegionEndpoint RegionEndpoint { get; set; }

		public string IdentityTableName { get; set; } = Constants.TableNames.IdentityTable;
		public string TokenTableName { get; set; } = Constants.TableNames.TokenTable;
		public string VerificationTokenTableName { get; set; } = Constants.TableNames.VerificationTokenTable;

		public Table IdentityTable { get; set; }
		public Table TokenTable { get; set; }
		public Table VerificationTokenTable { get; set; }

		public IAmazonDynamoDB DynamoDbClient { get; private set; }

		public AWSDynamoDbAuthenticationProvider(string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint regionEndpoint)
		{
			this.AwsAccessKeyId = awsAccessKeyId;
			this.AwsSecretAccessKey = awsSecretAccessKey;
			this.RegionEndpoint = regionEndpoint;
		}

		public static async Task<AWSDynamoDbAuthenticationProvider> Create(string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint regionEndpoint)
		{
			var provider = new AWSDynamoDbAuthenticationProvider(awsAccessKeyId, awsSecretAccessKey, regionEndpoint);
			await provider.Initialize().ConfigureAwait(false);
			return provider;
		}

		public async Task Initialize(CancellationToken cancellationToken = default(CancellationToken))
		{
			var tablesAdded = false;
			var allTables = new List<string>() { this.IdentityTableName, this.TokenTableName, this.VerificationTokenTableName };

			this.DynamoDbClient = new AmazonDynamoDBClient(this.AwsAccessKeyId, this.AwsSecretAccessKey, this.RegionEndpoint);
			var tableResponse = await this.DynamoDbClient.ListTablesAsync(cancellationToken).ConfigureAwait(false);
			var currentTables = tableResponse.TableNames;

			if (!currentTables.Contains(this.IdentityTableName))
			{
				var tableRequest = new CreateTableRequest
				{
					TableName = this.IdentityTableName,
					ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },
					KeySchema = new List<KeySchemaElement>
					{
						new KeySchemaElement // Partition
						{
							AttributeName = nameof(AWSIdentityEntity.PartitionKey),
							KeyType = KeyType.HASH
						},
						new KeySchemaElement // Sort key
						{
							AttributeName = nameof(AWSIdentityEntity.IdentityId),
							KeyType = KeyType.RANGE
						}
					},
					AttributeDefinitions = new List<AttributeDefinition>
					{
						new AttributeDefinition
						{
							AttributeName =  nameof(AWSIdentityEntity.PartitionKey),
							AttributeType = ScalarAttributeType.S
						},
						new AttributeDefinition
						{
							AttributeName =  nameof(AWSIdentityEntity.SecondaryPartitionKey),
							AttributeType = ScalarAttributeType.S
						},
						new AttributeDefinition
						{
							AttributeName =  nameof(AWSIdentityEntity.IdentityId),
							AttributeType = ScalarAttributeType.S
						},
						new AttributeDefinition
						{
							AttributeName =  nameof(AWSIdentityEntity.EmailAddress),
							AttributeType = ScalarAttributeType.S
						}
					},
					GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>{
						new GlobalSecondaryIndex
						{
							IndexName = Constants.PartitionNames.EmailAddressToIdentityForeignKey,
							ProvisionedThroughput = new ProvisionedThroughput
							{
								ReadCapacityUnits = 10,
								WriteCapacityUnits = 1
							},
							Projection = new Projection
							{
								ProjectionType = "INCLUDE",
								NonKeyAttributes = new List<string>() { nameof(AWSIdentityEntity.IdentityId) }
							},
							KeySchema = new List<KeySchemaElement>
							{
								new KeySchemaElement
								{
									AttributeName = nameof(AWSIdentityEntity.SecondaryPartitionKey),
									KeyType = KeyType.HASH
								},
								new KeySchemaElement
									{
									AttributeName = nameof(AWSIdentityEntity.EmailAddress),
									KeyType = KeyType.RANGE
								}
							}
						}
					},
				};

				await this.DynamoDbClient.CreateTableAsync(tableRequest, cancellationToken).ConfigureAwait(false);
				tablesAdded = true;
			}

			if (!currentTables.Contains(this.TokenTableName))
			{
				var tableRequest = new CreateTableRequest
				{
					TableName = this.TokenTableName,
					ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },
					KeySchema = new List<KeySchemaElement>
					{
						new KeySchemaElement
						{
							AttributeName = nameof(AWSTokenEntity.PartitionKey),
							KeyType = KeyType.HASH
						},
						new KeySchemaElement
						{
							AttributeName = nameof(AWSTokenEntity.Token),
							KeyType = KeyType.RANGE
						},
					},
					AttributeDefinitions = new List<AttributeDefinition>
					{
						new AttributeDefinition
						{
							AttributeName =  nameof(AWSTokenEntity.PartitionKey),
							AttributeType = ScalarAttributeType.S
						},
						new AttributeDefinition
						{
							AttributeName =  nameof(AWSTokenEntity.Token),
							AttributeType = ScalarAttributeType.S
						}
					}
				};

				await this.DynamoDbClient.CreateTableAsync(tableRequest, cancellationToken).ConfigureAwait(false);
				tablesAdded = true;
			}

			if (!currentTables.Contains(this.VerificationTokenTableName))
			{
				var tableRequest = new CreateTableRequest
				{
					TableName = this.VerificationTokenTableName,
					ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },
					KeySchema = new List<KeySchemaElement>
					{
						new KeySchemaElement
						{
							AttributeName = nameof(AWSVerificationTokenEntity.PartitionKey),
							KeyType = KeyType.HASH
						},
						new KeySchemaElement
						{
							AttributeName = nameof(AWSVerificationTokenEntity.VerificationToken),
							KeyType = KeyType.RANGE
						},
					},
					AttributeDefinitions = new List<AttributeDefinition>
					{
						new AttributeDefinition
						{
							AttributeName =  nameof(AWSVerificationTokenEntity.PartitionKey),
							AttributeType = ScalarAttributeType.S
						},
						new AttributeDefinition
						{
							AttributeName =  nameof(AWSVerificationTokenEntity.VerificationToken),
							AttributeType = ScalarAttributeType.S
						}
					}
				};

				await this.DynamoDbClient.CreateTableAsync(tableRequest, cancellationToken).ConfigureAwait(false);
				tablesAdded = true;
			}

			if (tablesAdded)
			{
				while (true)
				{
					var allActive = true;
					foreach (var table in allTables)
					{
						var tableStatus = await GetTableStatus(table, cancellationToken).ConfigureAwait(false);
						var isTableActive = string.Equals(tableStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase);
						if (!isTableActive)
						{
							allActive = false;
						}
					}

					if (allActive)
					{
						break;
					}

					await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
				}
			}

			this.IdentityTable = Table.LoadTable(this.DynamoDbClient, IdentityTableName);
			this.TokenTable = Table.LoadTable(this.DynamoDbClient, TokenTableName);
			this.VerificationTokenTable = Table.LoadTable(this.DynamoDbClient, VerificationTokenTableName);
		}

		private async Task<TableStatus> GetTableStatus(string tableName, CancellationToken cancellationToken)
		{
			try
			{
				var tableRequest = await this.DynamoDbClient.DescribeTableAsync(new DescribeTableRequest { TableName = tableName }, cancellationToken).ConfigureAwait(false);
				var table = tableRequest.Table;
				return table == null ? null : table.TableStatus;
			}
			catch (ResourceNotFoundException)
			{
				return string.Empty;
			}
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
				var tokenResult = await this.TokenTable.GetItemAsync(Constants.PartitionNames.TokenPrimary, token, cancellationToken).ConfigureAwait(false);
				var tokenEntity = JsonConvert.DeserializeObject<AWSTokenEntity>(tokenResult.ToJson());
				return new AuthenticateResult
				{
					Success = true,
					IdentityId = tokenEntity.IdentityId
				};
			}
			catch (Exception)
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
				var queryResponse = await QueryEmailSecondaryIndex(emailAddress, cancellationToken).ConfigureAwait(false);
				if (queryResponse.Count != 1)
				{
					return new AuthorizeResult
					{
						Success = false
					};
				}

				var identitySecondaryResult = queryResponse.Items.First();
				var identityId = identitySecondaryResult[nameof(AWSIdentityEntity.IdentityId)].S;

				// Retrieve IdentityEntity
				var identityResult = await this.IdentityTable.GetItemAsync(Constants.PartitionNames.IdentityPrimary, identityId, cancellationToken).ConfigureAwait(false);
				var identityEntity = JsonConvert.DeserializeObject<AWSIdentityEntity>(identityResult.ToJson());

				// Check if provided password is valid
				var passwordMatches = Security.PasswordMatches(identityEntity.PerUserSalt, password, identityEntity.HashedPassword);
				if (!passwordMatches)
				{
					return new AuthorizeResult
					{
						Success = false
					};
				}

				// Email verification check
				if (this.EmailProvider != null && !identityEntity.EmailVerified && !this.AllowUnverifiedIdentities)
				{
					return new AuthorizeResult
					{
						Success = false,
						IdentityId = identityEntity.IdentityId,
						Message = "Identity has not been verified by email yet"
					};
				}

				// Generate, store and return token
				var token = Security.GenerateToken();
				var tokenEntity = new AWSTokenEntity
				{
					IdentityId = identityEntity.IdentityId,
					Token = token
				};

				var tokenDocument = Document.FromJson(JsonConvert.SerializeObject(tokenEntity));
				var putResponse = await this.TokenTable.PutItemAsync(tokenDocument, cancellationToken).ConfigureAwait(false);

				return new AuthorizeResult
				{
					Success = true,
					IdentityId = identityEntity.IdentityId,
					Token = token
				};
			}
			catch (Exception)
			{
				return new AuthorizeResult
				{
					Success = false
				};
			}
		}

		public async Task<ResetPasswordResult> ChangePassword(Guid identityId, string oldPassword, string newPassword, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				var identityResult = await this.IdentityTable.GetItemAsync(Constants.PartitionNames.IdentityPrimary, identityId.ToString(), cancellationToken).ConfigureAwait(false);
				var identityEntity = JsonConvert.DeserializeObject<AWSIdentityEntity>(identityResult.ToJson());

				var authorizeResult = await Authorize(identityEntity.EmailAddress, oldPassword, cancellationToken).ConfigureAwait(false);
				if (!authorizeResult.Success)
				{
					return new ResetPasswordResult
					{
						Success = false
					};
				}

				var perUserSalt = Security.GeneratePerUserSalt();
				var hashedPassword = Security.GeneratePasswordHash(newPassword, perUserSalt);

				identityEntity.PerUserSalt = perUserSalt;
				identityEntity.HashedPassword = hashedPassword;

				var identityDocument = Document.FromJson(JsonConvert.SerializeObject(identityEntity));
				var putResult = await this.IdentityTable.UpdateItemAsync(identityDocument, cancellationToken).ConfigureAwait(false);

				return new ResetPasswordResult
				{
					Success = true
				};
			}
			catch (Exception)
			{
				return new ResetPasswordResult
				{
					Success = false
				};
			}
		}

		public async Task<bool> CheckIdentityExists(string emailAddress, CancellationToken cancellationToken = default(CancellationToken))
		{
			var result = await QueryEmailSecondaryIndex(emailAddress, cancellationToken).ConfigureAwait(false);
			var identityExists = result.Count > 0;
			return identityExists;
		}

		private async Task<QueryResponse> QueryEmailSecondaryIndex(string emailAddress, CancellationToken cancellationToken = default(CancellationToken))
		{
			var partitionKeyDescriptor = ":partitionKey";
			var emailDescriptor = ":partition";
			var queryRequest = new QueryRequest
			{
				TableName = this.IdentityTableName,
				IndexName = Constants.PartitionNames.EmailAddressToIdentityForeignKey,
				KeyConditionExpression = $"SecondaryPartitionKey = {partitionKeyDescriptor} and EmailAddress = {emailDescriptor}",
				ExpressionAttributeValues = new Dictionary<string, AttributeValue>
				{
					[partitionKeyDescriptor] = new AttributeValue { S = Constants.PartitionNames.EmailAddressToIdentityForeignKey },
					[emailDescriptor] = new AttributeValue { S = emailAddress }
				},
				ScanIndexForward = true
			};

			var result = await this.DynamoDbClient.QueryAsync(queryRequest, cancellationToken).ConfigureAwait(false);
			return result;
		}

		public async Task<CreateIdentityResult> CreateIdentity(string emailAddress, string password, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				// Identity generation
				var perUserSalt = Security.GeneratePerUserSalt();
				var hashedPassword = Security.GeneratePasswordHash(password, perUserSalt);
				var identityEntity = new AWSIdentityEntity()
				{
					IdentityId = Guid.NewGuid(),
					EmailAddress = emailAddress,
					PerUserSalt = perUserSalt,
					HashedPassword = hashedPassword
				};

				var identityDocument = Document.FromJson(JsonConvert.SerializeObject(identityEntity));
				var insertIdentityResponse = await this.IdentityTable.PutItemAsync(identityDocument, cancellationToken).ConfigureAwait(false);

				// Token generation
				var token = Security.GenerateToken();
				var tokenEntity = new AWSTokenEntity
				{
					IdentityId = identityEntity.IdentityId,
					Token = token
				};


				var tokenDocument = Document.FromJson(JsonConvert.SerializeObject(tokenEntity));
				var insertTokenResponse = await this.TokenTable.PutItemAsync(tokenDocument, cancellationToken).ConfigureAwait(false);
				return new CreateIdentityResult
				{
					Success = true,
					IdentityId = identityEntity.IdentityId,
					Token = tokenEntity.Token
				};
			}
			catch (Exception)
			{
				return new CreateIdentityResult
				{
					Success = false
				};
			}
		}

		public async Task DeleteIdentity(Guid identityId, CancellationToken cancellationToken = default(CancellationToken))
		{
			// Retrieve identity entity
			var identityResult = await this.IdentityTable.GetItemAsync(Constants.PartitionNames.IdentityPrimary, identityId.ToString(), cancellationToken).ConfigureAwait(false);
			var identityEntity = JsonConvert.DeserializeObject<AWSIdentityEntity>(identityResult.ToJson());

			// Delete all authentication tokens associated with the Identity
			var partitionKeyDescriptor = ":partitionKey";
			var identityIdDescriptor = ":identityId";
			var request = new QueryRequest
			{
				TableName = this.TokenTableName,
				KeyConditionExpression = $"PartitionKey = {partitionKeyDescriptor}",
				FilterExpression = $"IdentityId = {identityIdDescriptor}",
				ExpressionAttributeValues = new Dictionary<string, AttributeValue>
				{
					[identityIdDescriptor] = new AttributeValue { S = identityId.ToString() },
					[partitionKeyDescriptor] = new AttributeValue { S = Constants.PartitionNames.TokenPrimary }
				}
			};

			var tokensOnIdentityResult = await this.DynamoDbClient.QueryAsync(request, cancellationToken).ConfigureAwait(false);
			foreach (var tokenResult in tokensOnIdentityResult.Items)
			{
				var tokenValue = tokenResult[nameof(AWSTokenEntity.Token)].S;
				var tokenDeleteResult = await this.TokenTable.DeleteItemAsync(Constants.PartitionNames.TokenPrimary, tokenValue, cancellationToken).ConfigureAwait(false);
			}

			// Delete Identity entity
			var identityDeleteResult = await this.IdentityTable.DeleteItemAsync(Constants.PartitionNames.IdentityPrimary, identityId.ToString(), cancellationToken).ConfigureAwait(false);
		}

		public async Task DeleteToken(string token, CancellationToken cancellationToken = default(CancellationToken))
		{
			var deleteResult = await this.TokenTable.DeleteItemAsync(Constants.PartitionNames.TokenPrimary, token, cancellationToken).ConfigureAwait(false);
		}

		public async Task<AuthorizeResult> GenerateTokenForIdentityId(Guid identityId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var identityResult = await this.IdentityTable.GetItemAsync(Constants.PartitionNames.IdentityPrimary, identityId.ToString(), cancellationToken).ConfigureAwait(false);
			var identityEntity = JsonConvert.DeserializeObject<AWSIdentityEntity>(identityResult.ToJson());

			var token = Security.GenerateToken();
			var tokenEntity = new AWSTokenEntity
			{
				IdentityId = identityEntity.IdentityId,
				Token = token
			};

			var tokenDocument = Document.FromJson(JsonConvert.SerializeObject(tokenEntity));
			var tokenInsertResult = await this.TokenTable.PutItemAsync(tokenDocument, cancellationToken).ConfigureAwait(false);

			return new AuthorizeResult
			{
				Success = true,
				Token = token,
				IdentityId = identityId
			};
		}

		public async Task<IIdentityEntity> GetIdentityById(Guid identityId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var identityResult = await this.IdentityTable.GetItemAsync(Constants.PartitionNames.IdentityPrimary, identityId.ToString(), cancellationToken).ConfigureAwait(false);
			var identityEntity = JsonConvert.DeserializeObject<AWSIdentityEntity>(identityResult.ToJson());
			return identityEntity;
		}

		public async Task<IIdentityEntity> GetIdentityByEmail(string emailAddress, CancellationToken cancellationToken = default(CancellationToken))
		{
			// Check to see if email exists
			var queryResponse = await QueryEmailSecondaryIndex(emailAddress, cancellationToken).ConfigureAwait(false);
			if (queryResponse.Count != 1)
			{
				throw new KeyNotFoundException($"The EmailAddress '{emailAddress}' was not found");
			}

			var identitySecondaryResult = queryResponse.Items.First();
			var identityId = identitySecondaryResult[nameof(AWSIdentityEntity.IdentityId)].S;

			// Retrieve IdentityEntity
			var identityResult = await this.IdentityTable.GetItemAsync(Constants.PartitionNames.IdentityPrimary, identityId, cancellationToken).ConfigureAwait(false);
			var identityEntity = JsonConvert.DeserializeObject<AWSIdentityEntity>(identityResult.ToJson());

			return identityEntity;
		}

		public async Task<string> GenerateVerificationToken(Guid identityId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var verificationToken = Security.GenerateToken();
			var verificationTokenEntity = new AWSVerificationTokenEntity
			{
				VerificationToken = verificationToken,
				IdentityId = identityId
			};

			var verificationTokenDocument = Document.FromJson(JsonConvert.SerializeObject(verificationTokenEntity));
			var insertTokenResponse = await this.VerificationTokenTable.PutItemAsync(verificationTokenDocument, cancellationToken).ConfigureAwait(false);
			return verificationToken;
		}

		public async Task<IIdentityEntity> VerifyVerificationToken(string verificationToken, bool deleteTokenOnRetrieval = true, CancellationToken cancellationToken = default(CancellationToken))
		{
			// Check to see if VerificationToken exists
			var verificationTokenResult = await this.VerificationTokenTable.GetItemAsync(Constants.PartitionNames.VerificationTokenPrimary, verificationToken, cancellationToken).ConfigureAwait(false);
			var verificationTokenEntity = JsonConvert.DeserializeObject<AWSVerificationTokenEntity>(verificationTokenResult.ToJson());

			// Retrieve IdentityEntity
			var identity = await GetIdentityById(verificationTokenEntity.IdentityId, cancellationToken).ConfigureAwait(false);
			if (!deleteTokenOnRetrieval)
			{
				return identity;
			}

			// Delete the verification token record
			var deleteResult = await this.VerificationTokenTable.DeleteItemAsync(Constants.PartitionNames.VerificationTokenPrimary, verificationToken, cancellationToken).ConfigureAwait(false);
			return identity;
		}
	}
}
