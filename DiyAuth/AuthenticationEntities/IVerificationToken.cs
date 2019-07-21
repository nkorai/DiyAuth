using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.AuthenticationEntities
{
	public interface IVerificationToken
	{
		string VerificationToken { get; set; }
		Guid IdentityId { get; set; }
	}
}
