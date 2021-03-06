﻿using DiyAuth.AuthenticationProviders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiyAuth.EmailProviders
{
	public interface IEmailProvider
	{
		IAuthenticationProvider AuthenticationProvider { get; set; }
		string VerificationEmailSubject { get; set; }
		string ForgotPasswordEmailSubject { get; set; }
		string TwoFactorAuthenticationSubject { get; set; }

		Uri VerificationTokenRedirectUri { get; set; }
		Uri ForgotPasswordRedirectUri { get; set; }
		Uri TwoFactorAuthenticationRedirectUri { get; set; }

		string VerificationTokenEmailTemplate { get; set; }
		string ForgotPasswordEmailTemplate { get; set; }
		string TwoFactorAuthenticationEmailTemplate { get; set; }

		Task SendVerificationEmail(Guid identityId, string subject, CancellationToken cancellationToken = default(CancellationToken));
		Task SendForgotPasswordEmail(string emailAddress, string subject, CancellationToken cancellationToken = default(CancellationToken));
		Task SendTwoFactorAuthenticationCodeEmail(Guid identityId, string subject, CancellationToken cancellationToken = default(CancellationToken));
	}
}
