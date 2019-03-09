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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleApp
{
	public sealed partial class ChangePasswordDialog : ContentDialog
	{
		public ChangePasswordDialog()
		{
			this.InitializeComponent();
		}

		private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
		}

		private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			var oldPassword = OldPasswordTextBox.Password;
			var newPassword = NewPasswordTextBox.Password;

			var changePasswordResult = await App.Authenticator.ChangePassword(App.IdentityId.Value, oldPassword, newPassword);
			if (changePasswordResult.Success)
			{
				var successDialog = new ContentDialog
				{
					Title = "Success",
					Content = "Successfully changed password.",
					CloseButtonText = "Ok"
				};

				await successDialog.ShowAsync();
			}
			else
			{
				var unssucessDialog = new ContentDialog
				{
					Title = "Error",
					Content = "Unable to change password",
					CloseButtonText = "Ok"
				};

				await unssucessDialog.ShowAsync();
			}
		}
	}
}
