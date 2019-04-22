﻿using DiyAuth.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiyAuth.AuthenticationProviders
{
	public interface IAuthenticationProvider
	{
		Task<bool> CheckIdentityExists(string emailAddress);
		Task<CreateIdentityResult> CreateIdentity(string emailAddress, string password);
		Task<AuthorizeResult> Authorize(string emailAddress, string password);
		Task<AuthenticateResult> Authenticate(string token);

		Task<ResetPasswordResult> ChangePassword(Guid identityId, string oldPassword, string newPassword);
		Task<AuthorizeResult> GenerateTokenForIdentityId(Guid identityId);
		Task DeleteToken(string token);
		Task DeleteIdentity(Guid identityId);
	}
}
