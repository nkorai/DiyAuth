using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiyAuth.EmailProviders
{
	public interface IEmailProvider
	{
		string EmailTemplate { get; set; }
		Task<bool> SendVerificationEmail(Guid identityId);
		Task<bool> SendForgotPasswordEmail(string emailAddress);
	}
}
