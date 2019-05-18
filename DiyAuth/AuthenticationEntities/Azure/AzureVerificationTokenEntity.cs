using DiyAuth.Utility;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.AuthenticationEntities.Azure
{
	public class AzureVerificationTokenEntity : TableEntity, IVerificationToken
	{
		public string VerificationToken
		{
			get
			{
				return this.RowKey;
			}
			set
			{
				this.RowKey = value;
			}
		}

		public Guid IdentityId { get; set; }

		public AzureVerificationTokenEntity()
		{
			this.PartitionKey = Constants.PartitionNames.VerificationTokenPrimary;
		}
	}
}
