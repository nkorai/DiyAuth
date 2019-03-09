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
	public sealed partial class LandingPage : Page
	{
		public LandingPage()
		{
			this.InitializeComponent();
			IdentityIdTextBlock.Text = App.IdentityId.ToString();
		}

		private async void GenerateTokenForIdentity_Click(object sender, RoutedEventArgs e)
		{
			var tokenResult = await App.Authenticator.GenerateTokenForIdentityId(App.IdentityId.Value);
			var tokenDialog = new ContentDialog()
			{
				Title = "Token",
				Content = tokenResult.Token,
				CloseButtonText = "Close"
			};

			await tokenDialog.ShowAsync();
		}

		private async void ChangePassword_Click(object sender, RoutedEventArgs e)
		{
		}

		private async void SignOut_Click(object sender, RoutedEventArgs e)
		{
			await App.Authenticator.DeleteToken(App.Token);
			SignOut();
		}

		private async void DeleteIdentityAndSignOut_Click(object sender, RoutedEventArgs e)
		{
			await App.Authenticator.DeleteIdentity(App.IdentityId.Value);
			SignOut();
		}

		private void SignOut()
		{
			App.Token = null;
			App.IdentityId = null;
			this.Frame.Navigate(typeof(LoginPage));
		}
	}
}
