using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiyAuth.BackendProviders
{
	public interface IBackendProvider
	{
		string ConnectionString { get; set; }
		Task CreateIdentity(string username, string password);
		Task Authorize(string username, string password);
		Task Authenticate(string token);
	}
}
