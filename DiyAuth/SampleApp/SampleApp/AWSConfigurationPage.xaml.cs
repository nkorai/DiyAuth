using Amazon;
using DiyAuth;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SampleApp
{
	public sealed partial class AWSConfigurationPage : Page
	{
		public readonly List<string> RegionList = new List<string>() { "APNortheast1", "APNortheast2", "APNortheast3", "APSouth1", "APSoutheast1", "APSoutheast2", "CACentral1", "CNNorth1", "CNNorthWest1", "EUCentral1", "EUNorth1", "EUWest1", "EUWest2", "EUWest3", "SAEast1", "USEast1", "USEast2", "USGovCloudEast1", "USGovCloudWest1", "USWest1", "USWest2" };

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
