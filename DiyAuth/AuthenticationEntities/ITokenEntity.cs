using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.AuthenticationEntities
{
	public interface ITokenEntity
	{
		string Token { get; set; }
		Guid IdentityId { get; set; }
	}
}
