using System;
using System.Collections.Generic;

using CoreLocation;
using Foundation;
using UIKit;
using CoreGraphics;
using BleIosExample.BLE;
using BleIosExample.Models;

namespace MyShop.iOS
{

	public partial class RangingViewController : UITableViewController
	{
		List<GBeacon>[] beacons;
		//CLLocationManager locationManager;
		//List<CLBeaconRegion> rangedRegions;
		private IBeaconReciever _beaconReciever;
        //UILabel customLabel;
        //Dictionary<string, string> beaconSignalStrength;

		public RangingViewController(UITableViewStyle style) : base(style)
		{
            Unknowns = new List<GBeacon>();
			Immediates = new List<GBeacon>();
			Nears = new List<GBeacon>();
			Fars = new List<GBeacon>();
			beacons = new List<GBeacon>[4] { Unknowns, Immediates, Nears, Fars };
		}

		List<GBeacon> Unknowns { get; set; }

		List<GBeacon> Immediates { get; set; }

		List<GBeacon> Nears { get; set; }

		List<GBeacon> Fars { get; set; }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
            _beaconReciever = new BeaconRecieverManager_IOS();
            //customLabel = new UILabel(new CGRect(10, 10, View.Bounds.Width - 20, View.Bounds.Height - 20));
            StartIBeaconReciever();
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			//StartIBeaconReciever();

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

		/*
        public class MapDelegate : MapViewDelegate
        */

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
            if (beacon.Uuid.ToString() != "11111111-1111-1111-1111-111111111111") return;
            //var beaconFullId = beacon.Major + "-" + beacon.Minor;
            //var rssi = beacon.Rssi;
            //var timestamp = DateTime.Now.ToString();// set zero
            HandleDidRangeBeacons(beacon);
        }

		public override nint NumberOfSections(UITableView tableView)
		{
			// skip empty groups
			int sections = 0;
			foreach (var group in beacons)
			{
				if (group.Count > 0)
					sections++;
			}
			return sections;
		}

		// empty section are not shown in TableView so we must exclude them
		int GetNonEmptySection(int section)
		{
			int current = 0;
			foreach (var group in beacons)
			{
				if (group.Count > 0)
				{
					if (section-- == 0)
						return current;
				}
				current++;
			}
			return -1;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return beacons[GetNonEmptySection((int)section)].Count;
		}

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            GBeacon beacon = beacons[GetNonEmptySection(indexPath.Section)][indexPath.Row];
            string phonenum = beacon.Minor.ToString();
            while (phonenum.Length < 5) {
                phonenum = "0" + phonenum;
            }
            phonenum = beacon.Major.ToString() + phonenum;
            while (phonenum.Length < 10)
            {
                phonenum = "0" + phonenum;
            }

            //long phone = beacon.Major * 100000 + beacon.Minor;
            string proximity = "";
            switch (beacon.Proximity)
            {
                case GProximity.Immediate:
                    proximity = "Immediate";
                    break;
                case GProximity.Near:
                    proximity = "Near";
                    break;
                case GProximity.Far:
                    proximity = "Far";
                    break;
                case GProximity.Unknown:
                    proximity = "Unknown";
                    break;
            }
            UIAlertView alert = new UIAlertView();
            alert.Title = "Do you want to call this phone number? : " + phonenum;
            var DistanceMeter = 0.30480000000122 * (Math.Pow(10, (beacon.Rssi - 63.5379) / (10 * 2.086)) * 3);
            alert.Message = String.Format("There are BVI users in need nearby. Accuracy: {0:0.00}m Estimated distance: {1}m",
                                          beacon.Accuracy, DistanceMeter);
            alert.AddButton("Cancel");
            alert.AddButton("Yes");
            alert.CancelButtonIndex = 0;
            alert.Show();
            alert.Clicked += (object s, UIButtonEventArgs ev) =>
            {
                if (ev.ButtonIndex != 0)
                {
                    var url = new NSUrl("tel:" + phonenum);

                    // ...otherwise show an alert dialog
                    if (!UIApplication.SharedApplication.OpenUrl(url))
                    {
                        var alert2 = UIAlertController.Create("Not supported", "Scheme 'tel:' is not supported on this device", UIAlertControllerStyle.Alert);
                        alert2.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                        PresentViewController(alert2, true, null);
                    }
                }
            };

            tableView.DeselectRow(indexPath, true);
        }

		public override string TitleForHeader(UITableView tableView, nint section)
		{
			if (NumberOfSections(tableView) == 0)
				return null;

			return ((CLProximity)GetNonEmptySection((int)section)).ToString();
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell("Cell");
			if (cell == null)
			{
				cell = new UITableViewCell(UITableViewCellStyle.Subtitle, "Cell");
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			}

			// Display the UUID, major, minor and accuracy for each beacon.
            GBeacon beacon = beacons[GetNonEmptySection(indexPath.Section)][indexPath.Row];
            long phone = beacon.Major * 100000 + beacon.Minor;
            var DistanceMeter = 0.30480000000122 * (Math.Pow(10, (beacon.Rssi - 63.5379) / (10 * 2.086)) * 3);
            cell.TextLabel.Text = beacon.Rssi.ToString() + "dB";
            cell.DetailTextLabel.Text = String.Format("Phone: {0}  Accuracy: {1:0.00}m Estimated distance: {2}m",
                                                      phone, beacon.Accuracy, DistanceMeter);
			return cell;
		}

        void HandleDidRangeBeacons(GBeacon beacon)
        {
            //Unknowns.Clear();
            //Immediates.Clear();
            //Nears.Clear();
            //Fars.Clear();
            long phone = beacon.Major * 100000 + beacon.Minor;
            int index = Immediates.FindIndex(x => x.FullId == beacon.FullId);
            int indexnear = Nears.FindIndex(x => x.FullId == beacon.FullId);
            int indexfar = Fars.FindIndex(x => x.FullId == beacon.FullId);
            int indexunknown = Unknowns.FindIndex(x => x.FullId == beacon.FullId);

            switch (beacon.Proximity)
            {
                case GProximity.Immediate:
                    if (index != -1) {
                        Immediates[index] = beacon;
                    } else Immediates.Add(beacon);
                    if (indexnear != -1) Nears.RemoveAt(indexnear);
                    if (indexfar != -1) Fars.RemoveAt(indexfar);
                    if (indexunknown != -1) Unknowns.RemoveAt(indexunknown);
                    break;
                case GProximity.Near:
					if (indexnear != -1)
					{
						Nears[indexnear] = beacon;
					}
					else Nears.Add(beacon);
					if (index != -1) Immediates.RemoveAt(index);
					if (indexfar != -1) Fars.RemoveAt(indexfar);
					if (indexunknown != -1) Unknowns.RemoveAt(indexunknown);
                    break;
                case GProximity.Far:
					if (indexfar != -1)
					{
						Fars[indexfar] = beacon;
					}
					else Fars.Add(beacon);
					if (indexnear != -1) Nears.RemoveAt(indexnear);
					if (index != -1) Immediates.RemoveAt(index);
					if (indexunknown != -1) Unknowns.RemoveAt(indexunknown);
                    break;
                case GProximity.Unknown:
					if (indexunknown != -1)
					{
						Unknowns[indexunknown] = beacon;
					}
					else Unknowns.Add(beacon);
					if (indexnear != -1) Nears.RemoveAt(indexnear);
					if (indexfar != -1) Fars.RemoveAt(indexfar);
					if (index != -1) Immediates.RemoveAt(index);
                    break;
            }
            TableView.ReloadData ();
        }
	}
}
