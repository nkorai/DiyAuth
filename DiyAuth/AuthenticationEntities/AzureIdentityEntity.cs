using DiyAuth.Utility;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace DiyAuth.AuthenticationEntities
{
	public class AzureIdentityEntity : TableEntity, IIdentityEntity
	{
		public Guid IdentityId
		{
			get
			{
				return Guid.Parse(this.RowKey);
			}
			set
			{
				this.RowKey = value.ToString();
			}
		}

		public string EmailAddress { get; set; }
		public string HashedPassword { get; set; }
		public string PerUserSalt { get; set; }

		public AzureIdentityEntity()
		{
			this.PartitionKey = Constants.PartitionNames.IdentityPrimary;
		}
	}
}
