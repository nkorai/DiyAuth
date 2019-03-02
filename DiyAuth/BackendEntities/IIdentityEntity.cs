using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.BackendEntities
{
	public interface IIdentityEntity
	{
		Guid IdentityId { get; set; }
		string EmailAddress { get; set; }
		string HashedPassword { get; set; }
		string PerUserSalt { get; set; }
	}
}
