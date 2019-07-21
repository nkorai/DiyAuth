using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiyAuth.EmailProviders.EmailHelpers.Mailgun
{
	public class MailgunClient
	{
		public string BaseUrl { get; set; }
		public string DomainUrl { get; set; }
		public string ApiKey { get; set; }
		private static HttpClient Client { get; set; }

		public MailgunClient(string baseUrl, string domainUrl, string apiKey)
		{
			this.BaseUrl = baseUrl;
			this.DomainUrl = domainUrl;
			this.ApiKey = apiKey;

			// Setting up HttpClient with Basic Authentication
			var url = $"{BaseUrl}/{domainUrl}";
			Client = new HttpClient
			{
				BaseAddress = new Uri(url)
			};

			 var byteArray = Encoding.ASCII.GetBytes($"api:{this.ApiKey}");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
		}

		public async Task SendEmail(string fromEmail, string toEmail, string subject, string messageContent, CancellationToken cancellationToken)
		{

		}
	}
}
