using DiyAuth.Models;
using DiyAuth.Utility;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiyAuth.BackendProviders
{
	public class AzureBackendProvider : IBackendProvider
	{
		public string ConnectionString { get; set; }
		public string IdentityTableName { get; set; } = Constants.IdentityTableName;
		public string TokenTableName { get; set; } = Constants.TokenTableName;

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
			await this.IdentityTable.CreateIfNotExistsAsync().ConfigureAwait(false);
		}

		public async Task<AuthenticateResult> Authenticate(string token)
		{
			throw new NotImplementedException();
		}

		public async Task<AuthorizeResult> Authorize(string username, string password)
		{
			throw new NotImplementedException();
		}

		public async Task<CreateIdentityResult> CreateIdentity(string username, string password)
		{
			throw new NotImplementedException();
		}
	}
}
