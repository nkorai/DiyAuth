using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DiyAuth.Models;
using DiyAuth.Utility;

namespace DiyAuth.AuthenticationProviders
{
	public class AWSDynamoDbAuthenticationProvider : IAuthenticationProvider
	{
		public string ConnectionString { get; set; }
		public string IdentityTableName { get; set; } = Constants.TableNames.IdentityTable;
		public string TokenTableName { get; set; } = Constants.TableNames.TokenTable;

		public AWSDynamoDbAuthenticationProvider(string connectionString)
		{
			this.ConnectionString = storageAccountConnectionString;
		}

		public async Task Initialize()
		{
			//this.StorageAccount = CloudStorageAccount.Parse(this.ConnectionString);
			//this.TableClient = this.StorageAccount.CreateCloudTableClient();
			//
			//this.IdentityTable = this.TableClient.GetTableReference(this.IdentityTableName);
			//this.TokenTable = this.TableClient.GetTableReference(this.TokenTableName);
			//
			//await this.IdentityTable.CreateIfNotExistsAsync().ConfigureAwait(false);
			//await this.TokenTable.CreateIfNotExistsAsync().ConfigureAwait(false);
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
