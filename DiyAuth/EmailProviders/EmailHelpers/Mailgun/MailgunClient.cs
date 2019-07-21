using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace DiyAuth.EmailProviders.EmailHelpers.Mailgun
{
	public class MailgunClient
	{
		private const string BaseUrl = "https://api.eu.mailgun.net/v3";

		public string DomainUrl { get; set; }
		public string ApiKey { get; set; }
		private static HttpClient Client { get; set; }

		public MailgunClient(string domainUrl, string apiKey)
		{
			this.DomainUrl = domainUrl;
			this.ApiKey = apiKey;

			var url = $"{BaseUrl}/{domainUrl}";

			Client = new HttpClient
			{
				BaseAddress = new Uri(url)
			};
		}
	}
}
