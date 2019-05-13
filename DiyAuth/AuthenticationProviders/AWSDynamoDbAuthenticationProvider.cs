using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using DiyAuth.AuthenticationEntities.AWS;
using DiyAuth.Models;
using DiyAuth.Utility;
using Newtonsoft.Json;

namespace DiyAuth.AuthenticationProviders
{
	public class AWSDynamoDbAuthenticationProvider : IAuthenticationProvider
	{
		public string AwsAccessKeyId { get; set; }
		public string AwsSecretAccessKey { get; set; }

		public RegionEndpoint RegionEndpoint { get; set; }

		public string IdentityTableName { get; set; } = Constants.TableNames.IdentityTable;
		public string TokenTableName { get; set; } = Constants.TableNames.TokenTable;

		public Table IdentityTable { get; set; }
		public Table TokenTable { get; set; }

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

		public async Task Initialize()
		{
			var tablesAdded = false;
			var allTables = new List<string>() { this.IdentityTableName, this.TokenTableName };

			this.DynamoDbClient = new AmazonDynamoDBClient(this.AwsAccessKeyId, this.AwsSecretAccessKey, this.RegionEndpoint);
			var tableResponse = await this.DynamoDbClient.ListTablesAsync();
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
							IndexName = Constants.PartitionNames.IdentityForeignKey,
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

				await this.DynamoDbClient.CreateTableAsync(tableRequest);
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

				await this.DynamoDbClient.CreateTableAsync(tableRequest);
				tablesAdded = true;
			}

			if (tablesAdded)
			{
				while (true)
				{
					var allActive = true;
					foreach (var table in allTables)
					{
						var tableStatus = await GetTableStatus(table);
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

					await Task.Delay(TimeSpan.FromSeconds(5));
				}
			}

			this.IdentityTable = Table.LoadTable(this.DynamoDbClient, IdentityTableName);
			this.TokenTable = Table.LoadTable(this.DynamoDbClient, TokenTableName);
		}

		private async Task<TableStatus> GetTableStatus(string tableName)
		{
			try
			{
				var tableRequest = await this.DynamoDbClient.DescribeTableAsync(new DescribeTableRequest { TableName = tableName });
				var table = tableRequest.Table;
				return table == null ? null : table.TableStatus;
			}
			catch (ResourceNotFoundException)
			{
				return string.Empty;
			}
		}

		public async Task<AuthenticateResult> Authenticate(string token, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		public async Task<AuthorizeResult> Authorize(string emailAddress, string password, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		public async Task<ResetPasswordResult> ChangePassword(Guid identityId, string oldPassword, string newPassword, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		public async Task<bool> CheckIdentityExists(string emailAddress, CancellationToken cancellationToken = default(CancellationToken))
		{
			var partitionKeyDescriptor = ":partitionKey";
			var emailDescriptor = ":partition";
			var queryRequest = new QueryRequest
			{
				TableName = this.IdentityTableName,
				IndexName = Constants.PartitionNames.IdentityForeignKey,
				KeyConditionExpression = $"SecondaryPartitionKey = {partitionKeyDescriptor} and EmailAddress = {emailDescriptor}",
				ExpressionAttributeValues = new Dictionary<string, AttributeValue>
				{
					[partitionKeyDescriptor] = new AttributeValue { S = Constants.PartitionNames.IdentityForeignKey },
					[emailDescriptor] = new AttributeValue { S = emailAddress }
				},
				ScanIndexForward = true
			};

			var result = await this.DynamoDbClient.QueryAsync(queryRequest);

			var identityExists = result.Count > 0;
			return identityExists;
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
				var insertIdentityResponse = await this.IdentityTable.PutItemAsync(identityDocument, cancellationToken);

				// Token generation
				var token = Security.GenerateToken();
				var tokenEntity = new AWSTokenEntity
				{
					IdentityId = identityEntity.IdentityId,
					Token = token
				};


				var tokenDocument = Document.FromJson(JsonConvert.SerializeObject(tokenEntity));
				var insertTokenResponse = await this.TokenTable.PutItemAsync(tokenDocument, cancellationToken);
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
			throw new NotImplementedException();
		}

		public async Task DeleteToken(string token, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		public async Task<AuthorizeResult> GenerateTokenForIdentityId(Guid identityId, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}
	}
}
