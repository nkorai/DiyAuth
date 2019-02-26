using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.BackendEntities
{
	public interface ITokenEntity
	{
		string Token { get; set; }
		Guid IdentityId { get; set; }
	}
}
