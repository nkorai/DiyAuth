namespace DiyAuth.Utility
{
	public static class Constants
	{
		public static class TableNames
		{
			public const string IdentityTable = "diyidentity";
			public const string TokenTable = "diytoken";
			public const string VerificationTokenTable = "diyverification";
		}

		public static class PartitionNames
		{
			public const string IdentityPrimary = "IdentityPrimary";
			public const string TokenPrimary = "TokenPrimary";
			public const string VerificationTokenPrimary = "VerificationTokenPrimary";
			public const string EmailAddressToIdentityForeignKey = "EmailAddressToIdentityId";
		}
	}
}
