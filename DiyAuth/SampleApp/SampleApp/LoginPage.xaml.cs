using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
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
	public sealed partial class LoginPage : Page
	{
		public LoginPage()
		{
			this.InitializeComponent();
		}

		private async void CreateIdentity_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var identityAlreadyExists = await App.Authenticator.CheckIdentityExists(CreateEmailAddressTextBlock.Text);
				if (identityAlreadyExists)
				{
					var identityExistsDialog = new ContentDialog
					{
						Title = "Error",
						Content = $"The identity {CreateEmailAddressTextBlock.Text} already exists. Please login.",
						CloseButtonText = "Ok"
					};

					var result = await identityExistsDialog.ShowAsync();
				}
				else
				{
					var result = await App.Authenticator.CreateIdentity(CreateEmailAddressTextBlock.Text, CreatePasswordTextBlock.Text);
					//TODO: navigate to landing page
				}
			}
			catch (Exception ex)
			{
				var unhandledErrorDialog = new ContentDialog
				{
					Title = "Error",
					Content = "An unhandled error occured. Exception: " + ex.ToString(),
					CloseButtonText = "Ok"
				};

				var result = await unhandledErrorDialog.ShowAsync();
			}
		}
	}
}
