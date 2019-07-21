using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp
{
	public static class Constants
	{
		public enum BackendOptions
		{
			AzureTableStorage,
			AwsDynamoDb
		}

		public enum EmailOptions
		{
			None,
			SendGrid
		}

		public static class LocalStorage
		{
			public const string AzureStorageConnectionString = "AzureStorageConnectionString";
			public const string AwsSecretAccessKey = "AwsSecretAccessKey";
			public const string AwsAccessKeyId = "AwsAccessKeyId";
			public const string AwsRegion = "AwsRegion";
			public const string SendGridApiKey = "SendGridApiKey";
		}
	}
}
