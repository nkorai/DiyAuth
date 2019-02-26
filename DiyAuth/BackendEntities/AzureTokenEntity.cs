﻿using DiyAuth.Utility;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.BackendEntities
{
	public class AzureTokenEntity : TableEntity, ITokenEntity
	{
		public string Token
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

		public AzureTokenEntity()
		{
			this.PartitionKey = Defaults.PartitionName;
		}
	}
}
