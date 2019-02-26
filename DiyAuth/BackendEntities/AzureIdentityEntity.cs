using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.BackendEntities
{
	public class AzureIdentityEntity : IIdentityEntity
	{
		public Guid IdentityId { get; set; }
		public string Username { get; set; }
		public string HashedPassword { get; set; }
		public string PerUserSalt { get; set; }
	}
}
