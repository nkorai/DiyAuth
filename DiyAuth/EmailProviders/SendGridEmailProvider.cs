﻿using DiyAuth.AuthenticationProviders;
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
			var identityExists = await this.AuthenticationProvider.CheckIdentityExists(emailAddress).ConfigureAwait(false);
			if (identityExists)
			{
				return; //TODO: what to do here;
			}

			// TODO: 
			// Generate verification token
			// Replace template token with generated token

			var content = this.ForgotPasswordEmailTemplate;
			var from = new EmailAddress(this.FromEmail);
			var to = new EmailAddress(emailAddress);
			var htmlContent = content;
			var message = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
			var response = await this.Client.SendEmailAsync(message, cancellationToken);
		}

		public async Task SendTwoFactorAuthenticationCodeEmail(Guid identityId, string subject, CancellationToken cancellationToken = default(CancellationToken))
		{
			var identityEntity = await this.AuthenticationProvider.GetIdentityById(identityId).ConfigureAwait(false);


			var content = this.TwoFactorAuthenticationEmailTemplate;
			var from = new EmailAddress(this.FromEmail);
			var to = new EmailAddress(identityEntity.EmailAddress);
			var htmlContent = content;
			var message = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
			var response = await this.Client.SendEmailAsync(message, cancellationToken);
		}

		public async Task SendVerificationEmail(Guid identityId, string subject, CancellationToken cancellationToken = default(CancellationToken))
		{
			var identityEntity = await this.AuthenticationProvider.GetIdentityById(identityId).ConfigureAwait(false);


			var content = this.VerificationTokenEmailTemplate;
			var from = new EmailAddress(this.FromEmail);
			var to = new EmailAddress(identityEntity.EmailAddress);
			var htmlContent = content;
			var message = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
			var response = await this.Client.SendEmailAsync(message, cancellationToken);
		}
	}
}
