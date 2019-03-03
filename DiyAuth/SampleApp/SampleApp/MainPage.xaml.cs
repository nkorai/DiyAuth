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
		}

		private async void Next_Click(object sender, RoutedEventArgs e)
		{
			var azureAuthenticator = await Authenticator.GetAzureAuthenticator(ConnectionStringTextBlock.Text);
			App.Authenticator = azureAuthenticator;
			this.Frame.Navigate(typeof(LoginPage));
		}
	}
}
