using DiyAuth.Utility;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.AuthenticationEntities.Azure
{
	public class AzureIdentityForeignKeyEntity : TableEntity
	{
		public string EmailAddress
		{
			get
			{
				return this.RowKey?.ToLowerInvariant();
			}
			set
			{
				this.RowKey = value?.ToLowerInvariant();
			}
		}

		public Guid IdentityId { get; set; }

		public AzureIdentityForeignKeyEntity()
		{
			this.PartitionKey = Constants.PartitionNames.EmailAddressToIdentityForeignKey;
		}
	}
}
