namespace DiyAuth.Utility
{
	public static class Constants
	{
		public static class LocalStorageKeys
		{
			public const string ConnectionStringLocalStoreKey = "ConnectionString";
		}

		public static class TableNames
		{
			public const string IdentityTableName = "diyidentity";
			public const string TokenTableName = "diytoken";
		}

		public static class PartitionNames
		{
			public const string DefaultPartitionName = "default";
			public const string IdentityForeignKeyPartitionName = "emailAddressToIdentityId";
		}
	}
}
