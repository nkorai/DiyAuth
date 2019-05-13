using Amazon.DynamoDBv2.DocumentModel;
using DiyAuth.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.AuthenticationEntities.AWS
{
	public class AWSTokenEntity : ITokenEntity
	{
		public string PartitionKey { get; set; } = Constants.PartitionNames.TokenPrimary;
		public string Token { get; set; }
		public Guid IdentityId { get; set; }
	}
}
