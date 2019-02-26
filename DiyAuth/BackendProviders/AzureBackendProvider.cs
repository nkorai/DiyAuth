using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiyAuth.BackendProviders
{
	public class AzureBackendProvider : IBackendProvider
	{
		public string ConnectionString { get; set; }

		public async Task Authenticate(string token)
		{
			throw new NotImplementedException();
		}

		public async Task Authorize(string username, string password)
		{
			throw new NotImplementedException();
		}

		public async Task CreateIdentity(string username, string password)
		{
			throw new NotImplementedException();
		}
	}
}
