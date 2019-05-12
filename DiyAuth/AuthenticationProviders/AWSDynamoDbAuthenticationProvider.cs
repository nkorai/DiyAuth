using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DiyAuth.AuthenticationEntities.AWS;
using DiyAuth.Models;
using DiyAuth.Utility;

namespace DiyAuth.AuthenticationProviders
{
	public class AWSDynamoDbAuthenticationProvider : IAuthenticationProvider
	{
		public string AwsAccessKeyId { get; set; }
		public string AwsSecretAccessKey { get; set; }

		public string IdentityTableName { get; set; } = Constants.TableNames.IdentityTable;
		public string TokenTableName { get; set; } = Constants.TableNames.TokenTable;

		public IAmazonDynamoDB DynamoDbClient { get; private set; }

		public AWSDynamoDbAuthenticationProvider(string awsAccessKeyId, string awsSecretAccessKey)
		{
			this.AwsAccessKeyId = awsAccessKeyId;
			this.AwsSecretAccessKey = awsSecretAccessKey;
		}

		public static async Task<AWSDynamoDbAuthenticationProvider> Create(string awsAccessKeyId, string awsSecretAccessKey)
		{
			var provider = new AWSDynamoDbAuthenticationProvider(awsAccessKeyId, awsSecretAccessKey);
			await provider.Initialize().ConfigureAwait(false);
			return provider;
		}

		public async Task Initialize()
		{
			var tablesAdded = false;
			var allTables = new List<string>() { this.IdentityTableName, this.TokenTableName };

			this.DynamoDbClient = new AmazonDynamoDBClient(this.AwsAccessKeyId, this.AwsSecretAccessKey);
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
						new KeySchemaElement
						{
							AttributeName = nameof(AWSIdentityEntity.IdentityId),
							KeyType = KeyType.HASH
						},
						new KeySchemaElement
						{
							AttributeName = nameof(AWSIdentityEntity.EmailAddress),
							KeyType = KeyType.RANGE
						}
					},
					AttributeDefinitions = new List<AttributeDefinition>
					{
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
					}
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
							AttributeName = nameof(AWSTokenEntity.Token),
							KeyType = KeyType.HASH
						},
						new KeySchemaElement
						{
							AttributeName = nameof(AWSTokenEntity.IdentityId),
							KeyType = KeyType.RANGE
						},
					},
					AttributeDefinitions = new List<AttributeDefinition>
					{
						new AttributeDefinition
						{
							AttributeName =  nameof(AWSTokenEntity.Token),
							AttributeType = ScalarAttributeType.S
						},
						new AttributeDefinition
						{
							AttributeName =  nameof(AWSTokenEntity.IdentityId),
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

		public Task<AuthenticateResult> Authenticate(string token)
		{
			throw new NotImplementedException();
		}

		public Task<AuthorizeResult> Authorize(string emailAddress, string password)
		{
			throw new NotImplementedException();
		}

		public Task<ResetPasswordResult> ChangePassword(Guid identityId, string oldPassword, string newPassword)
		{
			throw new NotImplementedException();
		}

		public Task<bool> CheckIdentityExists(string emailAddress)
		{
			throw new NotImplementedException();
		}

		public Task<CreateIdentityResult> CreateIdentity(string emailAddress, string password)
		{
			throw new NotImplementedException();
		}

		public Task DeleteIdentity(Guid identityId)
		{
			throw new NotImplementedException();
		}

		public Task DeleteToken(string token)
		{
			throw new NotImplementedException();
		}

		public Task<AuthorizeResult> GenerateTokenForIdentityId(Guid identityId)
		{
			throw new NotImplementedException();
		}
	}
}
