using DiyAuth.AuthenticationEntities;
using DiyAuth.EmailProviders;
using DiyAuth.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiyAuth.AuthenticationProviders
{
	public interface IAuthenticationProvider
	{
		IEmailProvider EmailProvider { get; set; }
		void SetEmailProvider(IEmailProvider emailProvider);

		Task<bool> CheckIdentityExists(string emailAddress, CancellationToken cancellationToken = default(CancellationToken));
		Task<IIdentityEntity> GetIdentityById(Guid identityId, CancellationToken cancellationToken = default(CancellationToken));
		Task<IIdentityEntity> GetIdentityByEmail(string emailAddress, CancellationToken cancellationToken = default(CancellationToken));
		Task<CreateIdentityResult> CreateIdentity(string emailAddress, string password, CancellationToken cancellationToken = default(CancellationToken));
		Task<AuthorizeResult> Authorize(string emailAddress, string password, CancellationToken cancellationToken = default(CancellationToken));
		Task<AuthenticateResult> Authenticate(string token, CancellationToken cancellationToken = default(CancellationToken));
		Task<ResetPasswordResult> ChangePassword(Guid identityId, string oldPassword, string newPassword, CancellationToken cancellationToken = default(CancellationToken));
		Task<AuthorizeResult> GenerateTokenForIdentityId(Guid identityId, CancellationToken cancellationToken = default(CancellationToken));
		Task DeleteToken(string token, CancellationToken cancellationToken = default(CancellationToken));
		Task DeleteIdentity(Guid identityId, CancellationToken cancellationToken = default(CancellationToken));
	}
}
