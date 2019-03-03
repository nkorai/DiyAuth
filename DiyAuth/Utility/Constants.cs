﻿namespace DiyAuth.Utility
{
	public static class Constants
	{
		public static class LocalStorageKeys
		{
			public const string ConnectionStringLocalStoreKey = "ConnectionString";
		}

		public static class TableNames
		{
			public const string IdentityTable = "diyidentity";
			public const string TokenTable = "diytoken";
		}

		public static class PartitionNames
		{
			public const string Default = "default";
			public const string IdentityForeignKey = "emailAddressToIdentityId";
		}
	}
}