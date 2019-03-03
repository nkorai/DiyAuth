using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.Utility
{
	public class Defaults
	{
		public const string IdentityTableName = "diyidentity";
		public const string TokenTableName = "diytoken";
		public const string DefaultPartitionName = "default";
		public const string IdentityForeignKeyPartitionName = "emailAddressToIdentityId";
	}
}
