using DiyAuth.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiyAuth.BackendProviders
{
	public interface IBackendProvider
	{
		string ConnectionString { get; set; }
		Task<CreateIdentityResult> CreateIdentity(string username, string password);
		Task<AuthorizeResult> Authorize(string username, string password);
		Task<AuthenticateResult> Authenticate(string token);
	}
}
