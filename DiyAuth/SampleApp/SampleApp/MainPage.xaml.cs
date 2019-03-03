using DiyAuth;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SampleApp
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();

			var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
			var connectionStringExists = localSettings.Values.ContainsKey("ConnectionString");
			if (connectionStringExists)
			{
				ConnectionStringTextBox.Text = localSettings.Values["ConnectionString"] as string;
			}
		}

		private async void Next_Click(object sender, RoutedEventArgs e)
		{
			var connectionString = ConnectionStringTextBox.Text;
			var azureAuthenticator = await Authenticator.GetAzureAuthenticator(connectionString);
			App.Authenticator = azureAuthenticator;

			var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
			localSettings.Values["ConnectionString"] = connectionString;

			this.Frame.Navigate(typeof(LoginPage));
		}
	}
}
