using DiyAuth.AuthenticationProviders;
using DiyAuth.Properties;
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
		public IAuthenticationProvider AuthenticationProvider { get; set; }

		public Uri VerificationTokenRedirectUri { get; set; }
		public Uri ForgotPasswordRedirectUri { get; set; }
		public Uri TwoFactorAuthenticationRedirectUri { get; set; }

		public string VerificationTokenEmailTemplate { get; set; }
		public string ForgotPasswordEmailTemplate { get; set; }
		public string TwoFactorAuthenticationEmailTemplate { get; set; }

		// SendGrid specific properties
		public string ApiKey { get; set; }
		public string FromEmail { get; set; }
		public string CompanyName { get; set; }
		public SendGridClient Client { get; set; }

		public SendGridEmailProvider(string apiKey, string fromEmail, string companyName, string verificationTokenRedirectUri, string forgotPasswordRedirectUri, string twoFactorAuthRedirectUri)
		{
			this.ApiKey = apiKey;
			this.CompanyName = companyName;
			this.FromEmail = fromEmail;
			this.Client = new SendGridClient(apiKey);
			this.ForgotPasswordEmailTemplate = Resources.ForgotPasswordTemplate;
			this.VerificationTokenEmailTemplate = Resources.VerificationTokenTemplate;
			this.TwoFactorAuthenticationEmailTemplate = Resources.TwoFactorTemplate;
		}

		public static SendGridEmailProvider Create(string apiKey, string fromEmail, string companyName, string verificationTokenRedirectUri, string forgotPasswordRedirectUri, string twoFactorAuthRedirectUri)
		{
			var provider = new SendGridEmailProvider(apiKey, fromEmail, companyName, verificationTokenRedirectUri, forgotPasswordRedirectUri, twoFactorAuthRedirectUri);
			return provider;
		}

		public async Task SendForgotPasswordEmail(string emailAddress, string subject, CancellationToken cancellationToken = default(CancellationToken))
		{
			var identityEntity = await this.AuthenticationProvider.GetIdentityByEmail(emailAddress).ConfigureAwait(false);

			var verificationToken = await this.AuthenticationProvider.GenerateVerificationToken(identityEntity.IdentityId);
			var verificationTokenLink = this.ForgotPasswordRedirectUri.AbsoluteUri.Trim('/') +  "/" + verificationToken;

			var content = this.ForgotPasswordEmailTemplate;
			content = content.Replace("##__Subject__##", subject);
			content = content.Replace("##__CompanyName__##", this.CompanyName);
			content = content.Replace("##__VerificationTokenLink__##", verificationTokenLink);

			var from = new EmailAddress(this.FromEmail);
			var to = new EmailAddress(emailAddress);
			var htmlContent = content;
			var message = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
			var response = await this.Client.SendEmailAsync(message, cancellationToken);
		}

		public async Task SendVerificationEmail(Guid identityId, string subject, CancellationToken cancellationToken = default(CancellationToken))
		{
			var identityEntity = await this.AuthenticationProvider.GetIdentityById(identityId).ConfigureAwait(false);

			var verificationToken = await this.AuthenticationProvider.GenerateVerificationToken(identityEntity.IdentityId);
			var verificationTokenLink = this.VerificationTokenRedirectUri.AbsoluteUri.Trim('/') + "/" + verificationToken;

			var content = this.VerificationTokenEmailTemplate;
			content = content.Replace("##__Subject__##", subject);
			content = content.Replace("##__CompanyName__##", this.CompanyName);
			content = content.Replace("##__VerificationTokenLink__##", verificationTokenLink);

			var from = new EmailAddress(this.FromEmail);
			var to = new EmailAddress(identityEntity.EmailAddress);
			var htmlContent = content;
			var message = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
			var response = await this.Client.SendEmailAsync(message, cancellationToken);
		}

		public async Task SendTwoFactorAuthenticationCodeEmail(Guid identityId, string subject, CancellationToken cancellationToken = default(CancellationToken))
		{
			var identityEntity = await this.AuthenticationProvider.GetIdentityById(identityId).ConfigureAwait(false);

			var verificationToken = await this.AuthenticationProvider.GenerateVerificationToken(identityEntity.IdentityId);
			var verificationTokenLink = this.TwoFactorAuthenticationRedirectUri.AbsoluteUri.Trim('/') + "/" + verificationToken;

			var content = this.TwoFactorAuthenticationEmailTemplate;
			content = content.Replace("##__Subject__##", subject);
			content = content.Replace("##__CompanyName__##", this.CompanyName);
			content = content.Replace("##__VerificationTokenLink__##", verificationTokenLink);

			var from = new EmailAddress(this.FromEmail);
			var to = new EmailAddress(identityEntity.EmailAddress);
			var htmlContent = content;
			var message = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
			var response = await this.Client.SendEmailAsync(message, cancellationToken);
		}
	}
}
