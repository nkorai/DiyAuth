﻿using DiyAuth.AuthenticationProviders;
using System.Threading.Tasks;

namespace DiyAuth
{
	public static class Authenticator
	{
		public static async Task<AzureTableStorageAuthenticationProvider> GetAzureAuthenticator(string connectionString)
		{
			var provider = await AzureTableStorageAuthenticationProvider.Create(connectionString).ConfigureAwait(false);
			return provider;
		}

		public static async Task<AWSDynamoDbAuthenticationProvider> GetAWSDynamoDbAuthenticator(string awsAccessKeyId, string awsSecretAccessKey)
		{
			var provider = await AWSDynamoDbAuthenticationProvider.Create(awsAccessKeyId, awsSecretAccessKey).ConfigureAwait(false);
			return provider;
		}
	}
}
