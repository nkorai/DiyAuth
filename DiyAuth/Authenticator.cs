using DiyAuth.AuthenticationProviders;
using System.Threading.Tasks;

namespace DiyAuth
{
	public static class Authenticator
	{
		public static async Task<AzureAuthenticationProvider> GetAzureAuthenticator(string connectionString)
		{
			var provider = await AzureAuthenticationProvider.Create(connectionString).ConfigureAwait(false);
			return provider;
		}
	}
}
