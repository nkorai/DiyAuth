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
		Task<bool> CheckIdentityExists(string emailAddress);
		Task<CreateIdentityResult> CreateIdentity(string emailAddress, string password);
		Task<AuthorizeResult> Authorize(string emailAddress, string password);
		Task<AuthenticateResult> Authenticate(string token);

		Task<ResetPasswordResult> ChangePassword(string emailAddress, string oldPassword, string newPassword);
		Task GenerateTokenForIdentity(Guid identityId);
		Task DeleteExpiredTokens(DateTime expiryDateTime);
		Task DeleteToken(string token);
		Task DeleteIdentity(Guid identityId);

		Task<string> GenerateVerificationToken();
		Task VerifyIdentity(string verificationToken);
	}
}
