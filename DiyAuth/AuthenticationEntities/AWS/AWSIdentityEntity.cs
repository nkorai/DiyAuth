using Amazon.DynamoDBv2.DocumentModel;
using DiyAuth.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.AuthenticationEntities.AWS
{
	public class AWSIdentityEntity : IIdentityEntity
	{
		public string PartitionKey { get; set; } = Constants.PartitionNames.IdentityPrimary;
		public string SecondaryPartitionKey { get; set; } = Constants.PartitionNames.EmailAddressToIdentityForeignKey;
		public Guid IdentityId { get; set; }
		public string EmailAddress { get; set; }
		public string HashedPassword { get; set; }
		public string PerUserSalt { get; set; }
	}
}
