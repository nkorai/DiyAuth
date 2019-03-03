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
	public sealed partial class CreateUserPage : Page
	{
		public CreateUserPage()
		{
			this.InitializeComponent();
		}

		private async void NavigateToLoginPage()
		{

		}

		private async void CreateIdentity_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var identityAlreadyExists = await App.Authenticator.CheckIdentityExists(EmailAddressTextBlock.Text);
				if (identityAlreadyExists)
				{
					ICommand navigateToLoginCommand = null;
					var identityExistsDialog = new ContentDialog
					{
						Title = "Error",
						Content = $"The identity {EmailAddressTextBlock.Text} already exists. Navigating you to the Login page",
						CloseButtonText = "Ok",
						CloseButtonCommand = navigateToLoginCommand
					};

					var result = await identityExistsDialog.ShowAsync();
				}
				else
				{
					var result = await App.Authenticator.CreateIdentity(EmailAddressTextBlock.Text, PasswordTextBlock.Text);
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
