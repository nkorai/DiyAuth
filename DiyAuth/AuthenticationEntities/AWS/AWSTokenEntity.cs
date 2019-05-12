﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.AuthenticationEntities.AWS
{
	public class AWSTokenEntity : ITokenEntity
	{
		public string Token { get; set; }
		public Guid IdentityId { get; set; }
	}
}