using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiyAuth.EmailProviders
{
	public class SendGridEmailProvider : IEmailProvider
	{
		// Interface properties
		public string VerificationTokenEmailTemplate { get; set; }
		public string ForgotPasswordEmailTemplate { get; set; }
		public string TwoFactorAuthenticationEmailTemplate { get; set; }

		// SendGrid specific properties
		public string ApiKey { get; set; }
		public string FromEmail { get; set; }
		public SendGridClient Client { get; set; }

		public SendGridEmailProvider(string apiKey, string fromEmail)
		{
			this.ApiKey = apiKey;
			this.FromEmail = fromEmail;
			this.Client = new SendGridClient(apiKey);
		}

		public static SendGridEmailProvider Create(string apiKey, string fromEmail)
		{
			var provider = new SendGridEmailProvider(apiKey, fromEmail);
			return provider;
		}

		public async Task SendForgotPasswordEmail(string emailAddress, string subject, CancellationToken cancellationToken = default(CancellationToken))
		{
			var content = this.ForgotPasswordEmailTemplate;
			var from = new EmailAddress(this.FromEmail);
			var to = new EmailAddress(emailAddress);
			var htmlContent = content;
			var message = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
			var response = await this.Client.SendEmailAsync(message, cancellationToken);
		}

		public async Task SendTwoFactorAuthenticationCodeEmail(Guid identityId, string subject, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		public async Task SendVerificationEmail(Guid identityId, string subject, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}
	}
}
