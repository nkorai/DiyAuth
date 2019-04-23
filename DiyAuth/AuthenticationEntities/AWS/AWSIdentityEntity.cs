using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.AuthenticationEntities.AWS
{
	public class AWSIdentityEntity : IIdentityEntity
	{
		public Guid IdentityId { get; set; }
		public string EmailAddress { get; set; }
		public string HashedPassword { get; set; }
		public string PerUserSalt { get; set; }
	}
}
