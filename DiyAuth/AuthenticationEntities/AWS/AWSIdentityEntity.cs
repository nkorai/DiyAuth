using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.AuthenticationEntities.AWS
{
	public class AWSIdentityEntity : Document, IIdentityEntity
	{
		public Guid IdentityId { get; set; }
		public string EmailAddress { get; set; }
		public string HashedPassword { get; set; }
		public string PerUserSalt { get; set; }
	}
}
