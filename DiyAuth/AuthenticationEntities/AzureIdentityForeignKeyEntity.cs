using DiyAuth.Utility;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.AuthenticationEntities
{
	public class AzureIdentityForeignKeyEntity : TableEntity
	{
		public string EmailAddress
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

		public AzureIdentityForeignKeyEntity()
		{
			this.PartitionKey = Constants.PartitionNames.IdentityForeignKeyPartitionName;
		}
	}
}
