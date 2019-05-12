using Amazon;
using DiyAuth;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		public readonly ObservableCollection<string> RegionList = new ObservableCollection<string>() { "USEast1", "CACentral1", "CNNorthWest1", "CNNorth1", "USGovCloudWest1", "USGovCloudEast1", "SAEast1", "APSoutheast1", "APSouth1", "APNortheast3", "APSoutheast2", "APNortheast1", "USEast2", "APNortheast2", "USWest2", "EUNorth1", "USWest1", "EUWest2", "EUWest3", "EUCentral1", "EUWest1" };

		public AWSConfigurationPage()
		{
			this.InitializeComponent();
			RegionsCombo.SelectedIndex = 0;
		}

		private async void Next_Click(object sender, RoutedEventArgs e)
		{
			var accessKeyId = AccessKeyIdTextBox.Text;
			var secretAccessKey = SecretAccessTextBox.Text;
			var regionString = RegionsCombo.SelectedValue as string;
			var region = GetRegion(regionString);

			var awsAuthenticator = await Authenticator.GetAWSDynamoDbAuthenticator(accessKeyId, secretAccessKey, region);
			App.Authenticator = awsAuthenticator;

			var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
			localSettings.Values[Constants.LocalStorage.AwsAccessKeyId] = accessKeyId;
			localSettings.Values[Constants.LocalStorage.AwsSecretAccessKey] = secretAccessKey;
			localSettings.Values[Constants.LocalStorage.AwsRegion] = regionString;

			this.Frame.Navigate(typeof(LoginPage));
		}

		private RegionEndpoint GetRegion(string regionString)
		{
			switch (regionString)
			{
				case ("USEast1"):
					return RegionEndpoint.USEast1;
				case ("CACentral1"):
					return RegionEndpoint.CACentral1;
				case ("CNNorthWest1"):
					return RegionEndpoint.CNNorthWest1;
				case ("CNNorth1"):
					return RegionEndpoint.CNNorth1;
				case ("USGovCloudWest1"):
					return RegionEndpoint.USGovCloudWest1;
				case ("USGovCloudEast1"):
					return RegionEndpoint.USGovCloudEast1;
				case ("SAEast1"):
					return RegionEndpoint.SAEast1;
				case ("APSoutheast1"):
					return RegionEndpoint.APSoutheast1;
				case ("APSouth1"):
					return RegionEndpoint.APSouth1;
				case ("APNortheast3"):
					return RegionEndpoint.APNortheast3;
				case ("APSoutheast2"):
					return RegionEndpoint.APSoutheast2;
				case ("APNortheast1"):
					return RegionEndpoint.APNortheast1;
				case ("USEast2"):
					return RegionEndpoint.USEast2;
				case ("APNortheast2"):
					return RegionEndpoint.APNortheast2;
				case ("USWest2"):
					return RegionEndpoint.USWest2;
				case ("EUNorth1"):
					return RegionEndpoint.EUNorth1;
				case ("USWest1"):
					return RegionEndpoint.USWest1;
				case ("EUWest2"):
					return RegionEndpoint.EUWest2;
				case ("EUWest3"):
					return RegionEndpoint.EUWest3;
				case ("EUCentral1"):
					return RegionEndpoint.EUCentral1;
				case ("EUWest1"):
					return RegionEndpoint.EUWest1;
				default:
					throw new NotImplementedException();
			}
		}
	}
}
