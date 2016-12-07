using System;
using MyShop;
using Xamarin.Forms;
using MyShopAdmin;
using MyShopAdmin.iOS;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using Foundation;
using CoreGraphics;

using System.CodeDom.Compiler;
using BleIosExample.BLE;
using BleIosExample.Models;

[assembly: ExportRenderer(typeof(BLEViewPage), typeof(BLEViewPageRenderer))]
namespace MyShopAdmin.iOS
{
	public class BLEViewPageRenderer : PageRenderer
	{
		UILabel customLabel = new UILabel(new CGRect(10,10,300,100));
		//UIButton but = UIButton.FromType(UIButtonType.System);
		bool alertshow = false;

		private IBeaconReciever _beaconReciever;

		public BLEViewPageRenderer()
		{
		}

		public BLEViewPage bleviewpage
		{
			get
			{
				return Element as BLEViewPage;
			}
		}

		protected override void OnElementChanged(VisualElementChangedEventArgs e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null || Element == null)
			{
				return;
			}

			try
			{
				StartIBeaconReciever();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(@"          ERROR: ", ex.Message);
			}
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			_beaconReciever = new BeaconRecieverManager_IOS();
			alertshow = false;

			customLabel.Text = "Please Scan the iBeacons";
			customLabel.TextColor = UIColor.Black;

			customLabel.LineBreakMode = UILineBreakMode.WordWrap;
			customLabel.Lines = 0;
			/*
			but.TouchUpInside += DidTap;

			var custombutton = new UIBarButtonItem("Fit Bounds", UIBarButtonItemStyle.Plain, DidTap);
			NavigationItem.RightBarButtonItem = custombutton;

			but.SetTitle("Save", UIControlState.Normal);
			but.SetTitleColor(UIColor.Yellow, UIControlState.Highlighted);

			but.Frame = new CGRect(10, 150, 50, 50);

			View.AddSubview(but);
			*/
			View.AddSubview(customLabel);

		}
		/*
		void DidTap(object sender, EventArgs e)
		{
			bleviewpage.UUID = customLabel.Text;
			bleviewpage.NavigateBack();
		}
		*/

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			StartIBeaconReciever();

		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			StopIBeaconReciever();
		}

		public void StartIBeaconReciever()
		{
			_beaconReciever.Start();
			_beaconReciever.BeaconBroadcastEvent += BeaconRecieverOnBeaconBroadcastEvent;

		}

		public void StopIBeaconReciever()
		{


			_beaconReciever.BeaconBroadcastEvent -= BeaconRecieverOnBeaconBroadcastEvent;
			_beaconReciever.Stop();
		}

		private void BeaconRecieverOnBeaconBroadcastEvent(object sender, EventArgs eventArgs)
		{
			//Console.WriteLine("start calulate signal!");
			var eArgs = (GBeaconEventArgs)eventArgs;

			if (eArgs != null)
			{
				if (eArgs.EventType == BeaconEventType.Broadcast)
				{
					var beacon = eArgs.Beacon;
					AddOrUpdateBeacon(beacon);
				}
			}
		}

		private void AddOrUpdateBeacon(GBeacon beacon)
		{

			var beaconFullId = beacon.Major + "-" + beacon.Minor;
			var rssi = beacon.Rssi;
			var timestamp = DateTime.Now.ToString();


			customLabel.Text = "Please Scan the iBeacons" + "\n" + beaconFullId + ": " + rssi + "dB : " + timestamp;
			Console.WriteLine(beaconFullId+ ": " + rssi + "dB  " );


			if (rssi > -60 && !alertshow)
			{
				alertshow = true;
				UIAlertView alert = new UIAlertView();
				alert.Title = "Are you sure you want to select this ibeacons information?";
				alert.AddButton("Cancel");
				alert.AddButton("OK");
				alert.CancelButtonIndex = 0;
				alert.Message = "iBeacon MajorID-MinorID: " + beaconFullId + "\n"+ " If yes, please click OK";
				alert.Clicked += (object s, UIButtonEventArgs ev) =>
			{
					// handle click event here
					if (ev.ButtonIndex != 0)
					{
						bleviewpage.UUID = beacon.Uuid;
						bleviewpage.MAJOR = beacon.Major;
						bleviewpage.MINOR = beacon.Minor;
						bleviewpage.NavigateBack();

					}else alertshow = false;

			};

				alert.Show();

			}

		}

	}
}

