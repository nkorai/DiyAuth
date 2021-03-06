﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.AuthenticationEntities
{
	public interface IIdentityEntity
	{
		Guid IdentityId { get; set; }
		string EmailAddress { get; set; }
		string HashedPassword { get; set; }
		string PerUserSalt { get; set; }
		bool EmailVerified { get; set; }
	}
}
