using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.BackendEntities
{
	public class AzureTokenEntity : ITokenEntity
	{
		public string Token { get; set; }
		public Guid IdentityId { get; set; }
	}
}
