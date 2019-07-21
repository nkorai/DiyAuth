using DiyAuth.EmailProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleApp
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MailgunConfigurationPage : Page
	{
		public MailgunConfigurationPage()
		{
			this.InitializeComponent();

			var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
			var connectionStringExists = localSettings.Values.ContainsKey(Constants.LocalStorage.MailgunApiKey);
			if (connectionStringExists)
			{
				ApiKeyTextBox.Text = localSettings.Values[Constants.LocalStorage.MailgunApiKey] as string;
			}
		}

		private void Next_Click(object sender, RoutedEventArgs e)
		{
			var apiKeyString = ApiKeyTextBox.Text;
			var defaultUri = new Uri("https://www.diyauth.com/verification/");
			var defaultSubjectLine = "Hello from your friends at DiyAuth";

			var sendGridEmailProvider = new MailgunEmailProvider(apiKeyString, "diyauth", "noreply", "DiyAuth", defaultSubjectLine, defaultSubjectLine, defaultSubjectLine, defaultUri, defaultUri, defaultUri);
			App.Authenticator.SetEmailProvider(sendGridEmailProvider);

			var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
			localSettings.Values[Constants.LocalStorage.MailgunApiKey] = apiKeyString;

			this.Frame.Navigate(typeof(LoginPage));
		}
	}
}
