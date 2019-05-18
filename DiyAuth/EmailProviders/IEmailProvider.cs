using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiyAuth.EmailProviders
{
	public interface IEmailProvider
	{
		string VerificationTokenEmailTemplate { get; set; }
		string ForgotPasswordEmailTemplate { get; set; }
		string TwoFactorAuthenticationEmailTemplate { get; set; }

		Task SendVerificationEmail(Guid identityId, string subject, CancellationToken cancellationToken = default(CancellationToken));
		Task SendForgotPasswordEmail(string emailAddress, string subject, CancellationToken cancellationToken = default(CancellationToken));
		Task SendTwoFactorAuthenticationCodeEmail(Guid identityId, string subject, CancellationToken cancellationToken = default(CancellationToken));
	}
}
