using DiyAuth.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.AuthenticationEntities.AWS
{
	public class AWSVerificationTokenEntity : IVerificationToken
	{
		public string PartitionKey { get; set; } = Constants.PartitionNames.VerificationTokenPrimary;
		public string VerificationToken { get; set; }
		public Guid IdentityId { get; set; }
	}
}
