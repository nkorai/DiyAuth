using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.Models
{
	public class CreateIdentityResult
	{
		public bool Success { get; internal set; }
		public Guid IdentityId { get; internal set; }
	}
}
