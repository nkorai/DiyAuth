using DiyAuth.Utility;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.BackendEntities
{
	public class AzureIdentityEntity : TableEntity, IIdentityEntity
	{
		public Guid IdentityId { get; set; }

		public string Username
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

		public string HashedPassword { get; set; }
		public string PerUserSalt { get; set; }

		public AzureIdentityEntity()
		{
			this.PartitionKey = Defaults.PartitionName;
		}
	}
}
