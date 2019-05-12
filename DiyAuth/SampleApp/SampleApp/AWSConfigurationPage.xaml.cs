using DiyAuth;
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
	public sealed partial class AWSConfigurationPage : Page
	{
		public AWSConfigurationPage()
		{
			this.InitializeComponent();
		}

		private async void Next_Click(object sender, RoutedEventArgs e)
		{
			var accessKeyId = AccessKeyIdTextBox.Text;
			var secretAccessKey = SecretAccessTextBox.Text;

			var awsAuthenticator = await Authenticator.GetAWSDynamoDbAuthenticator(accessKeyId, secretAccessKey);
			App.Authenticator = awsAuthenticator;

			var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
			localSettings.Values[Constants.LocalStorage.AwsAccessKeyId] = accessKeyId;
			localSettings.Values[Constants.LocalStorage.AwsSecretAccessKey] = secretAccessKey;

			this.Frame.Navigate(typeof(LoginPage));
		}
	}
}
