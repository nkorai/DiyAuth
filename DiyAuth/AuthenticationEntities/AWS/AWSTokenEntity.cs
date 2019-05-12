using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.AuthenticationEntities.AWS
{
	public class AWSTokenEntity : Document, ITokenEntity
	{
		public string Token { get; set; }
		public Guid IdentityId { get; set; }
	}
}
