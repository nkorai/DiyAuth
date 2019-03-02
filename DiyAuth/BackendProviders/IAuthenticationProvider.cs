using DiyAuth.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiyAuth.AuthenticationProviders
{
	public interface IAuthenticationProvider
	{
		string ConnectionString { get; set; }
		Task<CreateIdentityResult> CreateIdentity(string emailAddress, string password);
		Task<AuthorizeResult> Authorize(string emailAddress, string password);
		Task<AuthenticateResult> Authenticate(string token);
		Task<ResetPasswordResult> ResetPassword(string emailAddress, string oldPassword, string newPassword);

		Task GenerateTokenForIdentity();
		Task DeleteExpiredTokens();
		Task DeleteToken();
		Task DeleteIdentity();
		Task<string> GenerateVerificationToken();
		Task VerifyIdentity(string verificationToken);
	}
}
