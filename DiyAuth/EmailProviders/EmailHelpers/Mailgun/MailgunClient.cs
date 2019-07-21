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
		public static string BaseUrl { get; set; }
		public static string Domain { get; set; }
		public static string ApiKey { get; set; }
		private static HttpClient Client { get; set; }

		public static async Task SendEmail(string fromEmail, string toEmail, string subject, string messageContent, CancellationToken cancellationToken)
		{
			if (Client == null)
			{
				var url = $"{BaseUrl}/{Domain}/";
				Client = new HttpClient
				{
					BaseAddress = new Uri(url)
				};

				var byteArray = Encoding.ASCII.GetBytes($"api:{ApiKey}");
				Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
			}

			using (var content = new MultipartFormDataContent())
			{
				content.Add(new StringContent(fromEmail), "from");
				content.Add(new StringContent(toEmail), "to");
				content.Add(new StringContent(subject), "subject");
				content.Add(new StringContent(messageContent), "html");
				
				var response = await Client.PostAsync("messages", content).ConfigureAwait(false);
				var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			}
		}
	}
}
