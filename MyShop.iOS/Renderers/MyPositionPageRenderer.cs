using System;
using System.Threading.Tasks;
using System.Globalization;
using System.Drawing;
using System.Net;
using System.Collections;
using System.IO;
using System.Linq;
using AVFoundation;
using MoreLinq;

using C5;
using Xamarin.Forms;
using MyShop;
using MyShop.iOS;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using Foundation;
using CoreGraphics;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using BleIosExample.BLE;
using BleIosExample.Models;

#if __UNIFIED__

using CoreLocation;

#else
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreLocation;

// Type Mappings Unified to monotouch.dll
using CGRect = global::System.Drawing.RectangleF;
using CGSize = global::System.Drawing.SizeF;
using CGPoint = global::System.Drawing.PointF;

using nfloat = global::System.Single;
using nint = global::System.Int32;
using nuint = global::System.UInt32;
#endif

using System.Collections.Generic;
using Google.Maps;

[assembly: ExportRenderer(typeof(PositionPage), typeof(MyPositionPageRenderer))]

namespace MyShop.iOS
{
	public delegate void PlaceSelected(object sender, JObject locationData);

	public enum PlaceType
	{
		All, Geocode, Address, Establishment, Regions, Cities
	}

	public class LocationBias
	{
		public readonly double latitude;
		public readonly double longitude;
		public readonly int radius;

		public LocationBias(double latitude, double longitude, int radius)
		{
			this.latitude = latitude;
			this.longitude = longitude;
			this.radius = radius;
		}

		public override string ToString()
		{
			return $"&location={latitude},{longitude}&radius={radius}";
		}
	}

	public class LocationObject
	{
		public double lat { get; set; }
		public double lon { get; set; }
		public string placeName { get; set; }
		public string placeID { get; set; }
	}

	public class Point
	{
		public double x { get; set; }
		public double y { get; set; }

	}

    public class Variables
    {
        public static bool isSelectingDestination;
        public static Marker destinationMarker;
    }
	/*
	class DescendedRssiComparer : IComparer<double>
	{
		public int Compare(double x, double y)
		{
			// use the default comparer to do the original comparison for datetimes
			int ascendingResult = Comparer<double>.Default.Compare(x, y);

			// turn the result around
			return 0 - ascendingResult;
		}
	}
*/
	public class MyPositionPageRenderer : PageRenderer
	{
		MapView mapView;
		//private MapDelegate mapDelegate;
		Dictionary<string, Store> landmarkbeacon;
		Dictionary<string, List<int>> beaconrssilist;
		List<Marker> markers;
		Dictionary<string, double> ReceivedBleAvgRssi;
		//List<GBeacon> Receivedbeacons;
		List<string> landmarktypes;

		UITextView BLEinstructionview;
        UITextView GPSinstructionview;
        UITextView Nearbyinstructionview;
        UIButton searchNearby;
        Store nearestLandmark;
        Queue myRecentLocation;
        //Marker destinationMarker;
        //bool isSelectingDestination;
        Dictionary<Marker, Store> landmarkMarkerInfo;
        int countShouldUpdateCamera;
        int countShouldUpdateNearbyLm;
        //int mindist;
        Dictionary<Store, List<Tuple<Store,string>>> childrenLandmarks;

		/* serach box ini */
		UISearchBar searchBar;
        bool searchFieldWasFirstResponder;
		UIImageView googleAttribution;
		UITableView resultsTable;
		UITableViewSource tableSource;
		public string apiKey { get; set; }
		LocationBias locationBias;
		string CustomPlaceType;
		public event PlaceSelected PlaceSelected;

		CLLocationManager iPhoneLocationManager = null;
        List<Google.Maps.Polyline> Lines;
        string speakText;
        string distanceVoiceOver;
        string bleVoiceOver;
        AVSpeechSynthesizer speechSynthesizer;

		//GPS
		double myheading;
		CLLocation myposition;
		Marker mylocationMarker = new Marker()
		{
			Title = "MyLocation",
			//Icon = UIImage.FromBundle ("pin"),
			//Icon = UIImage.FromBundle("navigation5"),
			Icon = Marker.MarkerImage(UIColor.Purple),
			Position = new CLLocationCoordinate2D(42.391827, -72.527012),
			//Map = mapView
		};

		//Marker cur_marker = null;
		private IBeaconReciever _beaconReciever;

		public PositionPage positionpage
		{
			get
			{
				return Element as PositionPage;
			}
		}

		public override void LoadView()
		{
			
			base.LoadView();
			nfloat h = 240.0f;
			nfloat w = View.Bounds.Width;
			nfloat maphei= View.Bounds.Height;

			_beaconReciever = new BeaconRecieverManager_IOS();
			markers = new List<Marker>();
			//Receivedbeacons = new List<GBeacon>();
			landmarkbeacon = new Dictionary<string, Store>();
			ReceivedBleAvgRssi = new Dictionary<string, double>();
			beaconrssilist = new Dictionary<string, List<int>>();
            myRecentLocation = new Queue();
            //mindist = int.MaxValue;

            Variables.isSelectingDestination = false;
            speakText = "";
            distanceVoiceOver = "";
            bleVoiceOver = "";
            speechSynthesizer = new AVSpeechSynthesizer();

            //Variables.destinationMarker = new Marker();
            landmarkMarkerInfo = new Dictionary<Marker, Store>();
            countShouldUpdateCamera = 0;
            countShouldUpdateNearbyLm = 0;
			apiKey = "AIzaSyApVhhHzhJF59Qbp3SWmyVaGtKVvx3lhqU";
            childrenLandmarks = new Dictionary<Store, List<Tuple<Store, string>>>();
		     
			CameraPosition camera = CameraPosition.FromCamera(latitude: 42.392262,
														  longitude: -72.526992, zoom: 17);
            mapView = MapView.FromCamera(new RectangleF(0, 0, 375, 603), camera);
			mapView.MyLocationEnabled = true;
			mapView.Settings.MyLocationButton = true;
			mapView.Settings.CompassButton = true;
			mapView.Settings.SetAllGesturesEnabled(true);
			//Init MapDelegate
			//mapDelegate = new MapDelegate(mapView);
			//mapView.Delegate = mapDelegate;
			//View = mapView;

			BLEinstructionview = new UITextView()
			{
				Text = "No BLE signal",
				Editable = false,
				Frame = new CGRect(0, 40, w/2 - 30, h/2 + 15),
                AdjustsFontForContentSizeCategory = true
			};

			GPSinstructionview = new UITextView()
			{
				Text = "No GPS signal",
				Editable = false,
                Font = UIFont.FromName("Helvetica Neue", 17),
                Frame = new CGRect(0, maphei / 2 - 30, w, maphei / 2-15),
                BackgroundColor = UIColor.FromWhiteAlpha(1f, 0.7f),
                AdjustsFontForContentSizeCategory = true
			};

            searchNearby = UIButton.FromType(UIButtonType.RoundedRect);
            //searchNearby.SetImage(UIImage.FromFile("nearestplace.png"), UIControlState.Normal);
            searchNearby.IsAccessibilityElement = true;
           
            searchNearby.SetTitle("Instructions", UIControlState.Normal);
            searchNearby.TitleLabel.Font = UIFont.FromName("Helvetica-Bold", 20f);
            //searchNearby.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            searchNearby.Frame = new CGRect(10, maphei-100, w-10, 15);
            searchNearby.TitleLabel.Hidden = false;
            searchNearby.TouchUpInside += async delegate{
                //displayNearestLandmarkInfo();
                await playInstructions();
                //searchCurrentLocationNearby();
             };

            Nearbyinstructionview = new UITextView()
            {
                Text = "No nearby place",
                IsAccessibilityElement = true,
                AccessibilityTraits = UIAccessibilityTrait.Header | UIAccessibilityTrait.Selected,
                Editable = false,
                Font = UIFont.FromName("Helvetica Neue", 17),
                Frame = new CGRect(5, 0, w-10, maphei / 2 - 30),
                BackgroundColor = UIColor.FromWhiteAlpha(1f, 0.7f),
                AdjustsFontForContentSizeCategory = true
            };


            //View = Nearbyinstructionview;
            View.AddSubview(Nearbyinstructionview);
            if (positionpage.Destination != null) {
                Nearbyinstructionview.Frame = new CGRect(5, 0, w - 10, maphei);
                View.AddSubview(searchNearby);
            } else {
                View.AddSubview(GPSinstructionview);
            }
            View.AddSubview(mapView);
            View.SendSubviewToBack(mapView);
            searchNearby.Hidden = true;

			landmarktypes = new List<string>();
			landmarktypes.Add("Work Zone");
            landmarktypes.Add("Building Entrance");
            landmarktypes.Add("Bus Stop");
			landmarktypes.Add("Round About");
			landmarktypes.Add("Crosswalk");
            landmarktypes.Add("Traffic Signal");
            landmarktypes.Add("Knowles Engineering Building");

            //mapView.TappedMarker = (aMapView, aMarker) =>
            //{
            //    // Animate to the marker
            //    var cam = new CameraPosition(aMarker.Position, 18, 0, 0);
            //    mapView.Animate(cam);
            //    UIAlertView alert = new UIAlertView();
            //    alert.Title = isSelectingDestination ? "Cancel previous destinsation?" : "Select this position as destination?";
            //    alert.AddButton("Cancel");
            //    alert.AddButton("OK");
            //    alert.CancelButtonIndex = 0;
            //    alert.Message = " If yes, please click OK";
            //    //alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
            //    alert.Clicked += (object s, UIButtonEventArgs ev) =>
            //    {
            //        if (ev.ButtonIndex != 0)
            //        {
            //            if (isSelectingDestination) {
            //                isSelectingDestination = false;
            //            } else {
            //                isSelectingDestination = true;
            //                destinationMarker = aMarker;
            //            }
            //        }
            //    };
            //    alert.Show();
            //    return true;
            //};
			/*
			mapView.CoordinateLongPressed += HandleLongPress;

			mapView.DraggingMarkerStarted += (sender, e) =>
			{
				movingmarker = true;
				e.Marker.Title = "start moving";
			};

			mapView.DraggingMarkerEnded += (sender, e) =>
			{
				movingmarker = false;
				UIAlertView alert = new UIAlertView();
				alert.Title = "Choose this position? ";
				alert.Message = string.Format("BLE at: {0}, {1}", e.Marker.Position.Latitude, e.Marker.Position.Longitude);
				alert.AddButton("Cancel");
				alert.AddButton("Yes");
				alert.CancelButtonIndex = 0;
				alert.Show();
				alert.Clicked += (object s, UIButtonEventArgs ev) =>
				{
					if (ev.ButtonIndex != 0)
					{
						e.Marker.Title = string.Format("BLE at: {0}, {1}", e.Marker.Position.Latitude, e.Marker.Position.Longitude);
						cur_marker = e.Marker;
					}
					else e.Marker.Map = null;
				};

			};
			mapView.DraggingMarker += (sender, e) => 
			{ 
				instructionview.Text = string.Format("selecting position at: {0}, {1}", e.Marker.Position.Latitude, e.Marker.Position.Longitude);
			};
			mapView.MarkerInfoWindow = (aMapview, aMarker) =>
			{
				if (aMarker == cur_marker) return new UIImageView(UIImage.FromBundle("beacon_linearnie"));
				return null;
			};
			*/
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
   //         searchFieldWasFirstResponder = false;
   //         /* searchbar ini */
   //         searchBar = new UISearchBar();
			//searchBar.TranslatesAutoresizingMaskIntoConstraints = false;
			//searchBar.ReturnKeyType = UIReturnKeyType.Done;
			//searchBar.SizeToFit();
			//searchBar.AutocorrectionType = UITextAutocorrectionType.No;
			//searchBar.AutocapitalizationType = UITextAutocapitalizationType.None;
			//searchBar.Placeholder = "Search";
			//searchBar.ShowsCancelButton = true;
			//searchBar.CancelButtonClicked += (object sender, EventArgs e) =>
			//{
			//	foreach (var marker in markers) marker.Map = null;
   //             //searchFieldWasFirstResponder = false;
   //             searchBar.ResignFirstResponder();
			//};

			//View.AddSubview(searchBar);
			//AddSearchBarConstraints();
   //         searchBar.ShouldBeginEditing += (UISearchBar sb) =>
   //         {
   //             if (!searchFieldWasFirstResponder)
   //             {
   //                 searchFieldWasFirstResponder = true;
   //                 searchBar.BecomeFirstResponder();
   //             }
   //             return true;
   //         };
   //         searchBar.ShouldEndEditing += (UISearchBar sb) =>
   //         {
   //             if (searchFieldWasFirstResponder)
   //             {
   //                 searchFieldWasFirstResponder = false;
   //                 //searchBar.ResignFirstResponder();
   //             }
   //             return true;
   //         };
   //         //searchBar.BecomeFirstResponder();
                     
			//resultsTable = new UITableView();
			//tableSource = new ResultsTableSource();
			//resultsTable.TranslatesAutoresizingMaskIntoConstraints = false;
			//resultsTable.Source = tableSource;
			//((ResultsTableSource)resultsTable.Source).apiKey = apiKey;
			//((ResultsTableSource)resultsTable.Source).RowItemSelected += OnPlaceSelection;
			//View.AddSubview(resultsTable);
			//AddResultsTableConstraints();

			//searchBar.TextChanged += SearchInputChanged;
			//resultsTable.Hidden = true;
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			mapView.StartRendering();

			//CoordinateBounds bounds = null;
            Lines = new List<Google.Maps.Polyline>();

			foreach (var landmarks in positionpage.Landmarks)
			{
				string fullid = landmarks.LocationHint + "-" + landmarks.LocationCode;

				CLLocationCoordinate2D pos;
				pos.Latitude = landmarks.Latitude;
				pos.Longitude = landmarks.Longitude;

                foreach (var landmarks2 in positionpage.Landmarks)
                {
                    if (landmarks2 != landmarks) {
                        if (landmarks.MondayOpen == landmarks2.StreetAddress) {
                            if (!childrenLandmarks.ContainsKey(landmarks)) childrenLandmarks.Add(landmarks, new List<Tuple<Store, string>>());
                            childrenLandmarks[landmarks].Add(Tuple.Create(landmarks2, landmarks.MondayClose));
                        }
                        if (landmarks.TuesdayOpen == landmarks2.StreetAddress) {
                            if (!childrenLandmarks.ContainsKey(landmarks)) childrenLandmarks.Add(landmarks, new List<Tuple<Store, string>>());
                            childrenLandmarks[landmarks].Add(Tuple.Create(landmarks2, landmarks.TuesdayClose));
                        }
                        if (landmarks.WednesdayOpen == landmarks2.StreetAddress) {
                            if (!childrenLandmarks.ContainsKey(landmarks)) childrenLandmarks.Add(landmarks, new List<Tuple<Store, string>>());
                            childrenLandmarks[landmarks].Add(Tuple.Create(landmarks2, landmarks.WednesdayClose));
                        }
                        if (landmarks.ThursdayOpen == landmarks2.StreetAddress){
                            if (!childrenLandmarks.ContainsKey(landmarks)) childrenLandmarks.Add(landmarks, new List<Tuple<Store, string>>());
                            childrenLandmarks[landmarks].Add(Tuple.Create(landmarks2, landmarks.ThursdayClose));
                        } 
                        if (landmarks.FridayOpen == landmarks2.StreetAddress) {
                            if (!childrenLandmarks.ContainsKey(landmarks)) childrenLandmarks.Add(landmarks, new List<Tuple<Store, string>>());
                            childrenLandmarks[landmarks].Add(Tuple.Create(landmarks2, landmarks.FridayClose));
                        }
                        if (landmarks.SaturdayOpen == landmarks2.StreetAddress) {
                            if (!childrenLandmarks.ContainsKey(landmarks)) childrenLandmarks.Add(landmarks, new List<Tuple<Store, string>>());
                            childrenLandmarks[landmarks].Add(Tuple.Create(landmarks2, landmarks.SaturdayClose));
                        }
                        if (landmarks.SundayOpen == landmarks2.StreetAddress) {
                            if (!childrenLandmarks.ContainsKey(landmarks)) childrenLandmarks.Add(landmarks, new List<Tuple<Store, string>>());
                            childrenLandmarks[landmarks].Add(Tuple.Create(landmarks2, landmarks.MondayClose));
                        }
                    }
                }

				if (!landmarkbeacon.ContainsKey(fullid))
				{
					landmarkbeacon.Add(fullid, landmarks);
				}
				else {
					landmarkbeacon[fullid] = landmarks;
				}

				var testMarker = new Marker()
				{
					Title = string.Format("Landmarks at: {0}, {1}", landmarks.Latitude, landmarks.Longitude),
					//Snippet = string.Format("{0} : {1} ; Submitted by {2}", feedback.ServiceType, feedback.Text, feedback.Name),
					Position = pos,
					AppearAnimation = MarkerAnimation.Pop,
					Tappable = true,
					Map = mapView
				};

                if (positionpage.Destination != null && landmarks == positionpage.Destination)
                {
                    Variables.destinationMarker = testMarker;
                    Variables.isSelectingDestination = true;
                }
                //markers.Add(testMarker);
                landmarkMarkerInfo[testMarker] = landmarks;
			}

			iPhoneLocationManager = new CLLocationManager();
			if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				iPhoneLocationManager.RequestWhenInUseAuthorization();
			}

			iPhoneLocationManager.LocationsUpdated += (sender, e) =>
			{
				myposition = e.Locations[0];

                double nearestDistance = 500000;
                foreach (var landmarks in positionpage.Landmarks)
                {
                    CLLocation landmarkPosition = new CLLocation(landmarks.Latitude, landmarks.Longitude);
                    //if (landmarks.LandmarksType == 5) continue; // skip indoor beacons

                    var landmarktype = landmarktypes[landmarks.LandmarksType];

                    if (myposition != null && landmarkPosition != null)
                    {
                        double distance = myposition.DistanceFrom(landmarkPosition);
                        if (nearestDistance > distance)
                        {
                            nearestDistance = distance;
                            nearestLandmark = landmarks;
                        }
                    }
                }

                if (countShouldUpdateCamera == 8) {
                    countShouldUpdateCamera = 0;
                }
                if (countShouldUpdateCamera == 0)
                {
                    var cam = new CameraPosition(myposition.Coordinate, 18, 0, 0);
                    mapView.Animate(cam);
                    if (positionpage.Destination != null && landmarkMarkerInfo[Variables.destinationMarker] != null) {
                        //getRoutesToDestination(Variables.destinationMarker);
                    }
                }
                countShouldUpdateCamera++;
                //mapView.Animate(cam);
                if (myRecentLocation.Count > 5) {
                    myRecentLocation.Dequeue();

                }

                myRecentLocation.Enqueue(myposition);

                if (nearestLandmark != null && (ReceivedBleAvgRssi.Count < 2 || myposition.HorizontalAccuracy < 10))
                {
                    updateNearbyinstructionview(nearestLandmark);
                }

                //if (nearestLandmark != null && !Variables.isSelectingDestination && ReceivedBleAvgRssi.Count < 3) {
                //    updateNearbyinstructionview(nearestLandmark);
                //} else if (Variables.isSelectingDestination && Variables.destinationMarker != null) {
                //    if (landmarkMarkerInfo[Variables.destinationMarker] != null && ReceivedBleAvgRssi.Count < 3) {
                //        updateNearbyinstructionview(landmarkMarkerInfo[Variables.destinationMarker]);
                //    }
                //}

				//if (myposition.HorizontalAccuracy > 15)
				//{
				//	GPSinstructionview.Text = "Weak GPS signal" + "\n" + "Your heading(°):" + myheading + "\n";
				//}
				//else 
                //gpslocation(myposition);

                //int headingidx = GPSinstructionview.Text.LastIndexOf("Your are at: ");
                //if (headingidx >= 0)
                //{
                    //string newinstruction = GPSinstructionview.Text.Substring(0, headingidx);
                    //newinstruction += "Direction: " + Direction(myposition, landmark) + "\n";
                    //"Your heading:" + myheading.ToString();
                    //GPSinstructionview.Text = newinstruction;
                //} else {
                    //GPSinstructionview.Text += "Your are at: " + currentLocation(myposition);
                //}

				//if (myposition.HorizontalAccuracy > 9)
				//{
				//	indoorlocalization();
				//}
				//Console.WriteLine(e.Locations[0]);
			};
			iPhoneLocationManager.StartUpdatingLocation();

			iPhoneLocationManager.UpdatedHeading += (object sender, CLHeadingUpdatedEventArgs e) =>
			{
				myheading = e.NewHeading.MagneticHeading;
                //if(ReceivedBleAvgRssi.Count < 2 || myposition.HorizontalAccuracy < 10) gpslocation(myposition);
				//if (GPSinstructionview.Text == "") GPSinstructionview.Text = "Your heading:" + myheading.ToString();
				//else {
				//	int headingidx = GPSinstructionview.Text.LastIndexOf("Your heading");
				//	if (headingidx >= 0)
				//	{
				//		string newinstruction = GPSinstructionview.Text.Substring(0, headingidx);
    //                    //newinstruction += "Direction: " + Direction(myposition, landmark) + "\n";
    //                        //"Your heading:" + myheading.ToString();
				//		GPSinstructionview.Text = newinstruction;
				//	}
				//}
				//if (myposition.HorizontalAccuracy <= 50) gpslocation(myposition);
				//Console.WriteLine("!rotation "+e.NewHeading.MagneticHeading.ToString ());
			};

			if (CLLocationManager.HeadingAvailable) iPhoneLocationManager.StartUpdatingHeading();
		}

		public override void ViewWillDisappear(bool animated)
		{
			mapView.StopRendering();
			base.ViewWillDisappear(animated);
            if (speechSynthesizer.Speaking) speechSynthesizer.StopSpeaking(AVSpeechBoundary.Immediate);
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			StartIBeaconReciever();

		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			StopIBeaconReciever();

			if (CLLocationManager.HeadingAvailable) iPhoneLocationManager.StopUpdatingHeading();
			iPhoneLocationManager.StopUpdatingLocation();
		}

        public override void MotionBegan(UIEventSubtype motion, UIEvent evt) {
            if (motion == UIEventSubtype.MotionShake) {
                displayNearestLandmarkInfo();
            }
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
            countShouldUpdateNearbyLm++;
            if (beacon.Major.ToString() == "999") return;  // 999 --> virtual BLE

		 	var beaconFullId = beacon.Major + "-" + beacon.Minor;
			var rssi = beacon.Rssi;
			var timestamp = DateTime.Now.ToString();// set zero


			if (rssi > -90 && landmarkbeacon.ContainsKey(beaconFullId))   //Started BLE localization...
			{
				double avgrssi = Double.MinValue;

				if (!beaconrssilist.ContainsKey(beaconFullId))
				{
					List<int> rssilist = new List<int>();
					rssilist.Add(rssi);
					beaconrssilist.Add(beaconFullId, rssilist);
				}
				else {
					if (beaconrssilist[beaconFullId].Count < 6)
					{
						beaconrssilist[beaconFullId].Add(rssi);
					}
					else {
						beaconrssilist[beaconFullId].RemoveAt(0);
						beaconrssilist[beaconFullId].Add(rssi);
					}
				}
				avgrssi = beaconrssilist[beaconFullId].Average();

				if (ReceivedBleAvgRssi.ContainsKey(beaconFullId))
				{
					ReceivedBleAvgRssi[beaconFullId] = avgrssi;
				}
				else
				{
					ReceivedBleAvgRssi.Add(beaconFullId, avgrssi);
				}


                double avg_rssi = ReceivedBleAvgRssi[beaconFullId];
                var DistanceMeter = Convert.ToInt32(0.30480000000122 * (Math.Pow(10, (-avg_rssi - 63.5379) / (10 * 2.086)) * 3));

                var maxRssiFullId = ReceivedBleAvgRssi.MaxBy(kvp => kvp.Value).Key;
                //var cur_landmarktype = landmarktypes[landmarkbeacon[maxRssiFullId].LandmarksType];
                var cur_landmarktype = landmarkbeacon[maxRssiFullId].Landmarks;
                //mindist = DistanceMeter;
                if (ReceivedBleAvgRssi.Count >= 2 && myposition.HorizontalAccuracy >= 10) {
                    nearestLandmark = landmarkbeacon[maxRssiFullId];
                    CLLocation landmark = new CLLocation(landmarkbeacon[maxRssiFullId].Latitude, landmarkbeacon[maxRssiFullId].Longitude);

                    searchNearby.Hidden = false;
                    UIAccessibility.PostNotification(UIAccessibilityPostNotification.LayoutChanged, searchNearby);

                    if (positionpage.Destination != null && searchNearby.Hidden) {
                        Nearbyinstructionview.Text = "Hello! you are near " + nearestLandmark.StreetAddress + "indoors, please find the nearest landmark and click the Instruction button to start navigation";
                        UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, new NSString(Nearbyinstructionview.Text));
                    }

                    if (positionpage.Destination == null) {
                        if (beacon.Proximity == GProximity.Far)
                        {

                            //CLLocation landmark = new CLLocation(landmarkbeacon[beaconFullId].Latitude, landmarkbeacon[beaconFullId].Longitude);
                            Nearbyinstructionview.Text = "BLE detected that you are close to: " + nearestLandmark.StreetAddress + "\n";
                            Nearbyinstructionview.Text += "Distance(meter): " + DistanceMeter.ToString() + "\n";
                            //instructionview.Text += "Direction: " + Direction(mylocation, landmark) + "\n";
                            //BLEinstructionview.Text += "Your heading:" + myheading;
                        }
                        if (beacon.Proximity == GProximity.Near || beacon.Proximity == GProximity.Immediate)
                        {
                            if (positionpage.Landmarks != null)
                            {
                                //CLLocation landmark = new CLLocation(landmarkbeacon[beaconFullId].Latitude, landmarkbeacon[beaconFullId].Longitude);
                                Nearbyinstructionview.Text = "BLE detected that you are at: " + nearestLandmark.StreetAddress + "\n";
                                Nearbyinstructionview.Text += "Distance(meter): " + DistanceMeter.ToString() + "\n";
                                if (nearestLandmark == positionpage.Destination)
                                {
                                    Nearbyinstructionview.Text += "You have arrived your destination " + "\n";
                                }
                                //instructionview.Text += "Direction: " + Direction(mylocation, landmark) + "\n";
                                //BLEinstructionview.Text += "Your heading:" + myheading;
                            }
                            //Console.WriteLine(beaconFullId + ": Near BLE!: " + rssi + "dB  ");
                        }
                        if (beacon.Proximity == GProximity.Unknown)
                        {
                            //CLLocation landmark = new CLLocation(landmarkbeacon[beaconFullId].Latitude, landmarkbeacon[beaconFullId].Longitude);
                            Nearbyinstructionview.Text = "BLE detected that you are far away from: " + nearestLandmark.StreetAddress + "\n";
                            //Console.WriteLine(beaconFullId + ": Unknow BLE!: " + rssi + "dB  ");
                        }
                        if (positionpage.Destination != null)
                        {
                            Nearbyinstructionview.Text += "Your Destination: " + positionpage.Destination.StreetAddress;
                        }

                        var orderedRssiFullId = ReceivedBleAvgRssi.OrderByDescending(kvp => kvp.Value);
                        GPSinstructionview.Text = "";
                        foreach (var fullid in orderedRssiFullId)
                        {
                            var dist = Convert.ToInt32(0.30480000000122 * (Math.Pow(10, (-ReceivedBleAvgRssi[fullid.Key] - 63.5379) / (10 * 2.086)) * 3));
                            var lmtype = landmarktypes[landmarkbeacon[fullid.Key].LandmarksType];
                            GPSinstructionview.Text += "You are close to: " + landmarkbeacon[fullid.Key].StreetAddress + " " + lmtype + "\n";
                            GPSinstructionview.Text += "Distance:(m) " + dist + "\n";
                            //GPSinstructionview.Text += "Direction: " + Direction(mylocation, landmarkPosition) + "\n";
                            GPSinstructionview.Text += "Info: " + landmarkbeacon[fullid.Key].Country + "\n";
                            GPSinstructionview.Text += "-------------------------------------------------------" + "\n";
                        }
                    }


                    //indoorlocalization();
                }

				//}
			}
		}

		//=============================================================================================================
		/*
		void HandleLongPress(object sender, GMSCoordEventArgs e)
		{
			if (!movingmarker)
			{
				var marker = new Marker()
				{
					Title = string.Format("Marker at: {0}, {1}", e.Coordinate.Latitude, e.Coordinate.Longitude),
					Position = e.Coordinate,
					AppearAnimation = MarkerAnimation.Pop,
					Draggable = true,
					Icon = Marker.MarkerImage(UIColor.Blue),
					Map = mapView
				};

				UIAlertView alert = new UIAlertView();
				alert.Title = "Choose this position?" + string.Format(" BLE at: {0}, {1} ", marker.Position.Latitude, marker.Position.Longitude);
				alert.Message = "if you want to change please click Move Marker and long click marker to select position";
				alert.AddButton("Cancel");
				alert.AddButton("Yes");
				alert.AddButton("Move Marker");
				alert.CancelButtonIndex = 0;
				alert.Show();
				alert.Clicked += (object s, UIButtonEventArgs ev) =>
				{
					if (ev.ButtonIndex == 1)
					{
						marker.Title = string.Format("BLE at: {0}, {1}", marker.Position.Latitude, marker.Position.Longitude);
						cur_marker = marker;
					}
					else if (ev.ButtonIndex == 0) marker.Map = null;
					else movingmarker = true;
				};

			}

		}
		*/


		void AddSearchBarConstraints()
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
			{
				var sbLeft = searchBar.LeftAnchor.ConstraintEqualTo(View.LeftAnchor);
				var sbRight = searchBar.RightAnchor.ConstraintEqualTo(View.RightAnchor);
				var sbTop = searchBar.TopAnchor.ConstraintEqualTo(View.TopAnchor);
				var sbHeight = searchBar.HeightAnchor.ConstraintEqualTo(45.0f);
				NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
				{
				sbLeft, sbRight, sbTop, sbHeight
				});
				UpdateViewConstraints();
			}
			else
			{
				searchBar.Frame = new CoreGraphics.CGRect(0, 0, View.Frame.Width, 45.0f);
			}
		}

		void AddResultsTableConstraints()
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
			{
				var rtLeft = resultsTable.LeftAnchor.ConstraintEqualTo(View.LeftAnchor);
				var rtRight = resultsTable.RightAnchor.ConstraintEqualTo(View.RightAnchor);
				var rtTop = resultsTable.TopAnchor.ConstraintEqualTo(searchBar.BottomAnchor);
				var rtBottom = resultsTable.BottomAnchor.ConstraintEqualTo(View.BottomAnchor);
				NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
				{
				rtLeft, rtRight, rtTop, rtBottom
				});
				UpdateViewConstraints();
			}
			else
			{
				resultsTable.Frame = new CoreGraphics.CGRect(0, 45.0f, View.Frame.Width, View.Frame.Height - 45.0f);
			}
		}

		public void SetLocationBias(LocationBias locationBias)
		{
			this.locationBias = locationBias;
		}

		public void SetPlaceType(PlaceType placeType)
		{
			switch (placeType)
			{
				case PlaceType.All:
					CustomPlaceType = "";
					break;
				case PlaceType.Geocode:
					CustomPlaceType = "geocode";
					break;
				case PlaceType.Address:
					CustomPlaceType = "address";
					break;
				case PlaceType.Establishment:
					CustomPlaceType = "establishment";
					break;
				case PlaceType.Regions:
					CustomPlaceType = "(regions)";
					break;
				case PlaceType.Cities:
					CustomPlaceType = "(cities)";
					break;
			}
		}

		async void SearchInputChanged(object sender, UISearchBarTextChangedEventArgs e)
		{
			if (e.SearchText == "")
			{
				resultsTable.Hidden = true;
			}
			else
			{
				resultsTable.Hidden = false;
				var predictions = await GetPlaces(e.SearchText);
				UpdateTableWithPredictions(predictions);
			}
		}

		async Task<string> GetPlaces(string searchText)
		{
			if (searchText == "")
				return "";

			var requestURI = CreatePredictionsUri(searchText);

			try
			{
				WebRequest request = WebRequest.Create(requestURI);
				request.Method = "GET";
				request.ContentType = "application/json";
				WebResponse response = await request.GetResponseAsync();
				string responseStream = string.Empty;
				using (StreamReader sr = new StreamReader(response.GetResponseStream()))
				{
					responseStream = sr.ReadToEnd();
				}
				response.Close();
				return responseStream;
			}
			catch
			{
				Console.WriteLine("Something's going wrong with my HTTP request");
				return "ERROR";
			}
		}

		void UpdateTableWithPredictions(string predictions)
		{
			if (predictions == "")
				return;

			if (predictions == "ERROR")
				return; // TODO - handle this better

			var deserializedPredictions = JsonConvert.DeserializeObject<LocationPredictions>(predictions);

			((ResultsTableSource)resultsTable.Source).predictions = deserializedPredictions;
			resultsTable.ReloadData();
		}

		protected virtual void OnPlaceSelection(object sender, JObject location)
		{
			if (PlaceSelected != null)
				PlaceSelected(this, location);
			resultsTable.Hidden = true;

			Place returnplace = new Place(location);

			searchBar.Text = returnplace.name;

			CLLocationCoordinate2D searchpos;
			searchpos.Latitude = returnplace.latitude;
			searchpos.Longitude = returnplace.longitude;

			var searchMarker = new Marker()
			{
				Title = string.Format("SearchResult at: {0}, {1}", returnplace.latitude, returnplace.longitude),
				Snippet = string.Format(returnplace.name),
				Position = searchpos,
				AppearAnimation = MarkerAnimation.Pop,
				Icon = Marker.MarkerImage(UIColor.Green),
				//Tappable = true,
				Map = mapView
			};
			markers.Add(searchMarker);
			var cam = new CameraPosition(searchpos, 17, 0, 0);
			mapView.Animate(cam);


			//DismissViewController(true, null);
		}


		string CreatePredictionsUri(string searchText)
		{
			var url = "https://maps.googleapis.com/maps/api/place/autocomplete/json";
			var input = Uri.EscapeUriString(searchText);

			var pType = "";
			if (CustomPlaceType != null)
				pType = CustomPlaceType;

			var constructedUrl = $"{url}?input={input}&types={pType}&key={apiKey}";

			if (this.locationBias != null)
				constructedUrl = constructedUrl + locationBias;

			//Console.WriteLine(constructedUrl);
			return constructedUrl;
		}
		//------------------------------------------------------------------------------------------------------------

		public void indoorlocalization()
		{
			if (ReceivedBleAvgRssi.Count < 3) return;
			LocalizationCaculation caculate = new LocalizationCaculation(ReceivedBleAvgRssi, landmarkbeacon);

			mylocationMarker.Position = caculate.PointLocalization();
			mylocationMarker.Map = mapView;
		}

		void BleOutdoorNotif(string fullid,CLLocation mylocation)   //close to landmarks or not
		{
			//instructionview.Text = "Your heading:" + myheading;

			var landmarktype = landmarktypes[landmarkbeacon[fullid].LandmarksType];
			double rssi = ReceivedBleAvgRssi[fullid];

			var DistanceMeter = 0.30480000000122 * (Math.Pow(10, (-rssi - 63.5379) / (10 * 2.086)) * 3);

			if (positionpage.Landmarks != null)
			{
				CLLocation landmark = new CLLocation(landmarkbeacon[fullid].Latitude, landmarkbeacon[fullid].Longitude);
				BLEinstructionview.Text = "BLE detected that you are close to: " + landmarkbeacon[fullid].StreetAddress + " " + landmarktype + "\n";
				BLEinstructionview.Text += "Distance(meter): " + DistanceMeter.ToString() + "\n";
				//instructionview.Text += "Direction: " + Direction(mylocation, landmark) + "\n";
				//BLEinstructionview.Text += "Your heading:" + myheading;
			}
			//Console.WriteLine("DIS " + DistanceMeter + "full " + fullid + "type " + instructionview.Text);
		}

		//------------------------------------------------------------------------------------------------------------
        void updateNearbyinstructionview(Store landmark) {
            if (landmark == null) {
                Nearbyinstructionview.Text = "Waiting for searching landmarks...";
                return;
            }
            searchNearby.Hidden = true;
            Nearbyinstructionview.AccessibilityLabel = Nearbyinstructionview.Text;
            CLLocation landmarkPosition = new CLLocation(landmark.Latitude, landmark.Longitude);
            var myPreLocation = (CLLocation)myRecentLocation.Peek();
            double distance = myposition.DistanceFrom(landmarkPosition);
            string distanceText = "The landmark is " + Convert.ToInt32(distance) + "meters ";
            string directionText = "";

            if (myRecentLocation.Count < 5)
            {
                Nearbyinstructionview.Text = "Hello! you are near " + landmark.StreetAddress + "outdoors, please find the nearest landmark to start navigation";
                UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, new NSString(Nearbyinstructionview.Text));
                return;
            }
            if (distance < 10)
            {
                Nearbyinstructionview.Text = "You are approaching " + landmark.StreetAddress + ",\n";
                string LRdirection = LeftRightDirection(myposition, landmarkPosition);
                directionText = (LRdirection != "") ? distanceText + "to your " + LRdirection + ",\n" : "";

                var navigationTips = navigation(landmark);

                if (Nearbyinstructionview.Text != speakText && searchNearby.Hidden == true) //&& ReceivedBleAvgRssi.Count < 3
                {
                    //speak(Nearbyinstructionview.Text + distanceText + directionText);
                    UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, new NSString(Nearbyinstructionview.Text + directionText + navigationTips.Item2));
                }
                speakText = Nearbyinstructionview.Text;

                if (LRdirection != "") Nearbyinstructionview.Text += distanceText + "to your " + LRdirection + ",\n";
                Nearbyinstructionview.Text += navigationTips.Item2;

            } else if (myPreLocation.DistanceFrom(landmarkPosition) > distance) {
                Nearbyinstructionview.Text = "You are moving towards " + landmark.StreetAddress + ",\n";
                directionText = Direction(myposition, landmarkPosition) + ",\n";
                if (Nearbyinstructionview.Text != speakText && searchNearby.Hidden == true)
                {
                    UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, new NSString(Nearbyinstructionview.Text + distanceText + directionText + landmark.Country));
                    //speak(Nearbyinstructionview.Text + distanceText + directionText);
                }
                speakText = Nearbyinstructionview.Text;

                Nearbyinstructionview.Text += distanceText + Direction(myposition, landmarkPosition) + ",\n";
                //Nearbyinstructionview.Text += distanceText;
                //Nearbyinstructionview.Text += "Info: " + landmark.Country + "\n";
            } else {
                string LRdirection = LeftRightDirection(myposition, landmarkPosition);
                directionText = (LRdirection != "") ? distanceText + "to your " + LRdirection + ",\n" : "";

                var navigationTips = navigation(landmark);
                CLLocation nextLandmarkPosition = new CLLocation(navigationTips.Item1.Latitude, navigationTips.Item1.Longitude);
                double nextDistance = myposition.DistanceFrom(nextLandmarkPosition);
                string nextDistanceText = "The landmark is " + Convert.ToInt32(nextDistance) + "meters ";
                Nearbyinstructionview.Text = navigationTips.Item2 + ",\n" + distanceText + Direction(myposition, nextLandmarkPosition) + ",\n"; ;

                //var nextStop = navigation(landmark).Item1;
                //CLLocation nextlandmarkPosition = new CLLocation(nextStop.Latitude, nextStop.Longitude);
                //Nearbyinstructionview.Text = "Your next stop is " + nextStop.StreetAddress + ",\n";
                //Nearbyinstructionview.Text += "The landmark is " + Convert.ToInt32(distance) + "meter ";
                //Nearbyinstructionview.Text += Direction(myposition, nextlandmarkPosition) + ",\n";

                //Nearbyinstructionview.Text += "Info: " + nextStop.Country + "\n";
                //Nearbyinstructionview.Text = "You are moving away from " + landmark.StreetAddress + ",\n";
                //if (Nearbyinstructionview.Text != speakText)
                //{
                //    UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, new NSString(Nearbyinstructionview.Text));
                //    //speak(Nearbyinstructionview.Text);
                //}
                //speakText = Nearbyinstructionview.Text;
            }

            //if (distanceVoiceOver != distanceText && (ReceivedBleAvgRssi.Count < 2 || myposition.HorizontalAccuracy < 10)) {
            //    UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, new NSString(distanceText));
            //    distanceVoiceOver = distanceText;
            //}

            //if (positionpage.Destination != null)
            //{
            //    Nearbyinstructionview.Text += "Your Destination: " + positionpage.Destination.StreetAddress;
            //}
        }

        Tuple<Store, string> navigation(Store landmark) {
            Tuple<Store,string> ans = null;
            string navigationTips = "";
            if (landmark != null && positionpage.Destination != null)
            {

                CLLocation desPosition = new CLLocation(positionpage.Destination.Latitude, positionpage.Destination.Longitude);

                Tuple<Store, string> nextLandmark = null;
                if (landmark == positionpage.Destination)
                {
                    navigationTips += "You have arrived the destination. " + "\n";
                    ans = Tuple.Create(landmark, navigationTips);
                }
                else
                {
                    if (childrenLandmarks.ContainsKey(landmark) && childrenLandmarks[landmark] != null)
                    {
                        var nextStops = childrenLandmarks[landmark];
                        double mindistance = Double.MaxValue;
                        foreach (var stop in nextStops)
                        {
                            CLLocation nextLandmarkPosition = new CLLocation(stop.Item1.Latitude, stop.Item1.Longitude);
                            var dist = desPosition.DistanceFrom(nextLandmarkPosition);
                            if (dist < mindistance)
                            {
                                mindistance = dist;
                                nextLandmark = stop;
                            }
                        }
                    }
                    if (nextLandmark != null)
                    {
                        CLLocation nextLandmarkPosition = new CLLocation(nextLandmark.Item1.Latitude, nextLandmark.Item1.Longitude);
                        //var directionText = "Direction: " + Direction(myposition, nextLandmarkPosition) + ",\n";
                        navigationTips += "Your next stop: " + nextLandmark.Item1.StreetAddress + ",\n";
                        navigationTips += nextLandmark.Item2;
                        ans = Tuple.Create(nextLandmark.Item1, navigationTips);
                    }
                }
            }
            return ans;
        }

        void displayNearestLandmarkInfo() {

            if (nearestLandmark  != null) {
                CLLocation landmarkPosition = new CLLocation(nearestLandmark.Latitude, nearestLandmark.Longitude);
                UIAlertView alert = new UIAlertView();
                alert.Title = "You're located at: " + nearestLandmark.StreetAddress + " " + nearestLandmark.Landmarks;
                alert.AddButton("Cancel");
                alert.AddButton("Show");
                alert.CancelButtonIndex = 0;
                alert.Message = "Distance(m): "+ Convert.ToInt32(myposition.DistanceFrom(landmarkPosition)) + "\n" + "Info: " + nearestLandmark.Country + "\n" + " If you want to know more about this landmark, please click Show";
                alert.Clicked += (object s, UIButtonEventArgs ev) =>
                {
                    // handle click event here
                    if (ev.ButtonIndex != 0)
                    {
                        var detailpage = new StorePage(nearestLandmark);
                        positionpage.Navigate(detailpage);
                    }
                };
                alert.Show();
            } 
        }

        async Task playInstructions()
        {
            var maxRssiFullId = ReceivedBleAvgRssi.MaxBy(kvp => kvp.Value).Key;
            var cur_landmarktype = landmarkbeacon[maxRssiFullId].Landmarks;
            double avg_rssi = ReceivedBleAvgRssi[maxRssiFullId];
            var DistanceMeter = Convert.ToInt32(0.30480000000122 * (Math.Pow(10, (-avg_rssi - 63.5379) / (10 * 2.086)) * 3));

            searchNearby.Hidden = false;
            nearestLandmark = landmarkbeacon[maxRssiFullId];
            CLLocation landmark = new CLLocation(landmarkbeacon[maxRssiFullId].Latitude, landmarkbeacon[maxRssiFullId].Longitude);
            Nearbyinstructionview.Text = "You're at: " + nearestLandmark.StreetAddress + "\n";
            Nearbyinstructionview.Text += "Distance(meter): " + DistanceMeter.ToString() + "\n";

            if (nearestLandmark != null && positionpage.Destination != null)
            {

                CLLocation landmarkPosition = new CLLocation(positionpage.Destination.Latitude, positionpage.Destination.Longitude);

                Tuple<Store, string> nextLandmark = null;
                if (nearestLandmark == positionpage.Destination) {
                    //UIAlertView alert = new UIAlertView();
                    Nearbyinstructionview.Text += "You have arrived the destination ";
                    //alert.AddButton("OK");
                    //alert.CancelButtonIndex = 0;
                    //alert.Show();
                } else {
                    if (childrenLandmarks.ContainsKey(nearestLandmark) && childrenLandmarks[nearestLandmark] != null)
                    {
                        var nextStops = childrenLandmarks[nearestLandmark];
                        double mindistance = Double.MaxValue;
                        foreach (var stop in nextStops)
                        {
                            CLLocation nextLandmarkPosition = new CLLocation(stop.Item1.Latitude, stop.Item1.Longitude);
                            var distance = landmarkPosition.DistanceFrom(nextLandmarkPosition);
                            if (distance < mindistance)
                            {
                                mindistance = distance;
                                nextLandmark = stop;
                            }
                        }
                    }
                    if (nextLandmark != null)
                    {
                        //UIAlertView alert = new UIAlertView();
                        CLLocation nextLandmarkPosition = new CLLocation(nextLandmark.Item1.Latitude, nextLandmark.Item1.Longitude);
                        //var directionText = "Direction: " + Direction(myposition, nextLandmarkPosition) + ",\n";
                        //if (ReceivedBleAvgRssi.Count >= 2 && myposition.HorizontalAccuracy >= 10) directionTextn= "";

                        var navInstructionText = Nearbyinstructionview.Text;

                        navInstructionText += "Your next stop: " + nextLandmark.Item1.StreetAddress + "\n";
                        //alert.AddButton("OK");
                        //alert.CancelButtonIndex = 0;
                        navInstructionText +=  nextLandmark.Item2 + "\n";
                        navInstructionText += "When you arrive the next landmark, please click the button to get updated instructions to your destintion " + positionpage.Destination.StreetAddress;
                        Nearbyinstructionview.Text = navInstructionText;
                    }
                }

                Console.Out.WriteLine($"Reached Post Notification: {Nearbyinstructionview.Text}");

                await Task.Delay(200);

                UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, new NSString(Nearbyinstructionview.Text));
                //CLLocation landmarkPosition = new CLLocation(nearestLandmark.Latitude, nearestLandmark.Longitude);
                //string instructions = nearestLandmark.Country;
                //speak(instructions);


             }
        }

        //void searchCurrentLocationNearby()
        //{
        //    string cur = "";
        //    double VIEWPORT_DELTA = 0.001;
        //    CLLocationCoordinate2D northEast = new CLLocationCoordinate2D(myposition.Coordinate.Latitude + VIEWPORT_DELTA,
        //                                                                  myposition.Coordinate.Longitude + VIEWPORT_DELTA);
        //    CLLocationCoordinate2D southWest = new CLLocationCoordinate2D(myposition.Coordinate.Latitude - VIEWPORT_DELTA,
        //                                                                  myposition.Coordinate.Longitude - VIEWPORT_DELTA);
        //    CoordinateBounds viewport = new CoordinateBounds(northEast,southWest);
            
        //    var config = new PlacePickerConfig(viewport);
        //    var placePicker = new PlacePicker(config);
        //    Google.Maps.Place newp;
        //    placePicker.PickPlaceWithCallback((result, error) => {

        //        if (error != null)
        //        {
        //            return;
        //        }
        //        if (result != null)
        //        {
        //            newp = result;
        //            cur = result.FormattedAddress;
        //            var selectMarker = new Marker()
        //            {
        //                Title = string.Format("Nearby places at: {0}, {1}", result.Coordinate.Latitude, result.Coordinate.Longitude),
        //                Snippet = string.Format(result.FormattedAddress),
        //                Position = result.Coordinate,
        //                AppearAnimation = MarkerAnimation.Pop,
        //                Icon = Marker.MarkerImage(UIColor.Blue),
        //                //Tappable = true,
        //                Map = mapView
        //            };
        //            var cam = new CameraPosition(result.Coordinate, 17, 0, 0);
        //            mapView.Animate(cam);
        //        }

        //    });
        //}

		void gpslocation(CLLocation mylocation)   //close to landmarks or not
		{
			if (positionpage.Landmarks != null)
			{
                GPSinstructionview.Text = "";
                double nearestDistance = 500000;
				foreach (var landmarks in positionpage.Landmarks)
				{
					CLLocation landmarkPosition = new CLLocation(landmarks.Latitude, landmarks.Longitude);
					//if (landmarks.LandmarksType == 5) continue; // skip indoor beacons

					var landmarktype = landmarktypes[landmarks.LandmarksType];

                    if (mylocation != null && landmarkPosition != null)
					{
                        double distance = mylocation.DistanceFrom(landmarkPosition);
                        if (nearestDistance > distance) {
                            nearestDistance = distance;
                            nearestLandmark = landmarks;
                        }
                        if (distance < 35) {
                            GPSinstructionview.Text += "You are close to: " + landmarks.StreetAddress + " " + landmarktype + "\n";
                            GPSinstructionview.Text += "The landmark is " + Convert.ToInt32(distance) + "meters";
                            GPSinstructionview.Text +=  Direction(mylocation, landmarkPosition) + "\n";
                            GPSinstructionview.Text += "Info: " + landmarks.Country + "\n";
                            GPSinstructionview.Text += "-------------------------------------------------------" + "\n";
                        }
					}

				}
				//GPSinstructionview.Text += "Your heading:" + myheading + "\n";
				//if (!findnear) instructionview.Text = "Your heading:" + myheading+ "\n";
			}
		}


		private double gpsbearing(CLLocation la, CLLocation lb)  // 0  90    0   -90   0  
		{
			double lat_a = la.Coordinate.Latitude;
			double lng_a = la.Coordinate.Longitude;
			double lat_b = lb.Coordinate.Latitude;
			double lng_b = lb.Coordinate.Longitude;
			double d = 0;
			lat_a = lat_a * Math.PI / 180;
			lng_a = lng_a * Math.PI / 180;
			lat_b = lat_b * Math.PI / 180;
			lng_b = lng_b * Math.PI / 180;

			d = Math.Sin(lat_a) * Math.Sin(lat_b) + Math.Cos(lat_a) * Math.Cos(lat_b) * Math.Cos(lng_b - lng_a);
			d = Math.Sqrt(1 - d * d);
			d = Math.Cos(lat_b) * Math.Sin(lng_b - lng_a) / d;
			d = Math.Asin(d) * 180 / Math.PI;

			return d;
		}

		string Direction(CLLocation a, CLLocation b)  // a -> original (b to a)
		{
			double landmarkbearing = gpsbearing(a, b);
			double landmarkheading = 0;

			if (a.Coordinate.Latitude > b.Coordinate.Latitude) // south
			{
				landmarkheading = 180 - landmarkbearing;

				if (a.Coordinate.Longitude < b.Coordinate.Longitude) //east  D2
				{
					/*
					if (myheading > 180 - landmarkbearing && myheading <= 270 - landmarkbearing) dir = "The landmark is on your left front";
					if (myheading <= 180 - landmarkbearing && myheading > 90 - landmarkbearing) dir = "The landmark is on your right front";
					if (myheading > 270 - landmarkbearing && myheading <= 360 - landmarkbearing) dir = "The landmark is on your left behind";
					if (myheading > 360 - landmarkbearing || myheading <= 90 - landmarkbearing) dir = "The landmark is on your right behind";
					*/

                    return clockdirection(landmarkheading);
				}

				if (a.Coordinate.Longitude > b.Coordinate.Longitude)//west  D3
				{
						/*
						if (myheading > 180 - landmarkbearing && myheading <= 270 - landmarkbearing) dir = "The landmark is on your left front";
						if (myheading <= 180 - landmarkbearing || myheading > 90 - landmarkbearing) dir = "The landmark is on your right front";
						if (myheading > 270 - landmarkbearing || myheading <= -landmarkbearing) dir = "The landmark is on your left behind";
						if (myheading > -landmarkbearing && myheading <= 90 - landmarkbearing) dir = "The landmark is on your right behind";
						*/
                    return clockdirection(landmarkheading);
				}

                return clockdirection(landmarkheading);
				

			}
			if (a.Coordinate.Latitude < b.Coordinate.Latitude) //North
			{
				if (landmarkbearing > 0) landmarkheading = landmarkbearing;
				else landmarkheading = 360 + landmarkbearing;

				if (a.Coordinate.Longitude < b.Coordinate.Longitude) //East    D1
				{
					/*
					if (myheading > landmarkbearing && myheading < landmarkbearing + 90) dir = "The landmark is on your left front";
					if ((myheading < landmarkbearing && myheading > 0) || myheading > landmarkbearing + 270) dir = "The landmark is on your right front";
					if (myheading > landmarkbearing + 90 && myheading < landmarkbearing + 180) dir = "The landmark is on your left behind";
					if (myheading > landmarkbearing + 180 && myheading < landmarkbearing + 270) dir = "The landmark is on your right behind";
					*/
                    return clockdirection(landmarkheading);
				}
				if (a.Coordinate.Longitude > b.Coordinate.Longitude)  //West    D4
				{
					/*
					if (myheading > landmarkbearing + 270 && myheading < landmarkbearing + 360) dir = "The landmark is on your right front";
					if ((myheading < landmarkbearing + 90 && myheading > 0) || myheading > landmarkbearing + 360) dir = "The landmark is on your left front";
					if (myheading > landmarkbearing + 90 && myheading < landmarkbearing + 180) dir = "The landmark is on your left behind";
					if (myheading > landmarkbearing + 180 && myheading < landmarkbearing + 270) dir = "The landmark is on your right behind";
					*/
                    return clockdirection(landmarkheading);
				}

                return clockdirection(landmarkheading);

			}
			else    //  latti same -> can not use gps bearing method
			{
				
				if (a.Coordinate.Longitude <= b.Coordinate.Longitude) //East
				{
					landmarkheading = 90;
					/*
					if (myheading >= 0 && myheading <= 90) dir = "The landmark is on your right front";
					if (myheading <= 180 && myheading > 90) dir = "The landmark is on your left front";
					if (myheading > 180 && myheading <= 270) dir = "The landmark is on your left behind";
					if (myheading > 270 && myheading <= 360) dir = "The landmark is on your right behind";
					*/
                    return clockdirection(landmarkheading);
				}
				else //West
				{
					landmarkheading = 270;
					/*
					if (myheading >= 0 && myheading <= 90) dir = "The landmark is on your left behind ";
					if (myheading <= 180 && myheading > 90) dir = "The landmark is on your right behind ";
					if (myheading > 180 && myheading <= 270) dir = "The landmark is on your right front";
					if (myheading > 270 && myheading <= 360) dir = "The landmark is on your left front";
					*/
                    return  clockdirection(landmarkheading);

				}

			}
		}

		string clockdirection(double landmarkheading)
		{
			string clock = "to your ";

			double diff = landmarkheading - myheading;
			if (diff < 0) diff += 360;

			if (diff < 15 && diff >= 0 || diff <= 359 && diff >= 345)
				clock += "12 o'clock";
			if (diff < 45 && diff >= 15 )
				clock += "1 o'clock";
			if (diff < 75 && diff >= 45)
				clock += "2 o'clock";
			if (diff < 105 && diff >= 75)
				clock += "3 o'clock";
			if (diff < 135 && diff >= 105 )
				clock += "4 o'clock";
			if (diff < 165 && diff >= 135)
				clock += "5 o'clock";
			if (diff < 195 && diff >= 165)
				clock += "6 o'clock";
			if (diff < 225 && diff >= 195)
				clock += "7 o'clock";
			if (diff < 255 && diff >= 225 )
				clock += "8 o'clock";
			if (diff < 285 && diff >= 255)
				clock += "9 o'clock";
			if (diff < 315 && diff >= 285)
				clock += "10 o'clock";
			if (diff < 345 && diff >= 315)
				clock += "11 o'clock";

			return clock;
		}


        string LeftRightDirection(CLLocation a, CLLocation b)  // a -> original (b to a)
        {
            double landmarkbearing = gpsbearing(a, b);
            double landmarkheading = 0;

            if (a.Coordinate.Latitude > b.Coordinate.Latitude) // south
            {
                landmarkheading = 180 - landmarkbearing;
            }
            if (a.Coordinate.Latitude < b.Coordinate.Latitude) //North
            {
                if (landmarkbearing > 0) landmarkheading = landmarkbearing;
                else landmarkheading = 360 + landmarkbearing;
            }
            else    //  latti same -> can not use gps bearing method
            {
                if (a.Coordinate.Longitude <= b.Coordinate.Longitude) //East
                {
                    landmarkheading = 90;
                }
                else //West
                {
                    landmarkheading = 270;
                }
            }
            return LeftOrRight(landmarkheading);
        }

        string LeftOrRight(double landmarkheading)
        {
            double diff = landmarkheading - myheading;
            if (diff < 0) diff += 360;
            //if (diff > 0 && diff < 80) return "right front";
            if(diff >= 10 && diff <= 170) return "right";
            //if (diff > 100 && diff < 180) return "right behind";
            //if (diff > 180 && diff < 260) return "left behind";
            if (diff >= 190 && diff <= 350) return "left";
            //if (diff > 280 && diff < 360) return "left front";
            return "";
        }

        //public readonly List<CLLocationCoordinate2D> Locations;
        //public readonly List<Google.Maps.Polyline> Lines;

        void getRoutesToDestination(Marker destination){
            SetDirectionsQuery(destination.Position);
        }

        private async void SetDirectionsQuery(CLLocationCoordinate2D destination)
        {
            //Clear Old Polylines
            string KMdDirectionsUrl = @"http://maps.googleapis.com/maps/api/directions/json?origin=";
            if (Lines.Count > 0)
            {
                foreach (var line in Lines)
                {
                    line.Map = null;
                }
                Lines.Clear();
            }

            //Start building Directions URL
            var sb = new System.Text.StringBuilder();
            sb.Append(KMdDirectionsUrl);
            sb.Append(myposition.Coordinate.Latitude.ToString(CultureInfo.InvariantCulture));
            sb.Append(",");
            sb.Append(myposition.Coordinate.Longitude.ToString(CultureInfo.InvariantCulture));
            sb.Append("&");
            sb.Append("destination=");
            sb.Append(destination.Latitude.ToString(CultureInfo.InvariantCulture));
            sb.Append(",");
            sb.Append(destination.Longitude.ToString(CultureInfo.InvariantCulture));
            sb.Append("&mode=walking");
            sb.Append("&sensor=true");

            //If we have more than 2 locations we'll append waypoints
            //if (Locations.Count > 2)
            //{
            //    sb.Append("&waypoints=");
            //    for (var i = 2; i < Locations.Count; i++)
            //    {
            //        if (i > 2)
            //            sb.Append("|");
            //        sb.Append(Locations[i].Latitude.ToString(CultureInfo.InvariantCulture));
            //        sb.Append(",");
            //        sb.Append(Locations[i].Longitude.ToString(CultureInfo.InvariantCulture));
            //    }
            //}

            //Get directions through Google Web Service
            var directionsTask = GetDirections(sb.ToString());

            var jSonData = await directionsTask;

            //Deserialize string to object
            var routes = JsonConvert.DeserializeObject<RootObject>(jSonData);

            foreach (var route in routes.routes)
            {
                //Encode path from polyline passed back
                var path = Google.Maps.Path.FromEncodedPath(route.overview_polyline.points);

                //Create line from Path
                var line = Google.Maps.Polyline.FromPath(path);
                line.StrokeWidth = 10f;
                line.StrokeColor = UIColor.Red;
                line.Geodesic = true;

                //Place line on map
                line.Map = mapView;
                Lines.Add(line);

            }

        }

        private async Task<String> GetDirections(string url)
        {
            var client = new WebClient();
            var directionsTask = client.DownloadStringTaskAsync(url);
            var directions = await directionsTask;

            return directions;

        }

        //--------------------------------TextToSpeech---------------------------------------------
        void speak(string text)
        {
            //var speechSynthesizer = new AVSpeechSynthesizer();
            var speechUtterance = new AVSpeechUtterance(text)
            {
                Rate = AVSpeechUtterance.MaximumSpeechRate/2,
                Voice = AVSpeechSynthesisVoice.FromLanguage("en-US"),
                Volume = 10f,
                PitchMultiplier = 1.0f
            };
            if (speechSynthesizer.Speaking) speechSynthesizer.StopSpeaking(AVSpeechBoundary.Immediate);
            speechSynthesizer.SpeakUtterance(speechUtterance);
        }


	}
}


/*
foreach (var landmarks in positionpage.Landmarks)
{

	CLLocation landmark = new CLLocation(landmarks.Latitude, landmarks.Longitude);

	if (mapView.MyLocation.DistanceFrom(landmark) < 100)
	{
		instructionField.Text = "GPS detected that you are close to a landmark "+"\n";


	}

}*/

/*
if (rssi > -90)  // Enter BLE area
{
	foreach (var landmarks in positionpage.Landmarks)
	{
		if (landmarks.LocationHint == beacon.Major.ToString() && landmarks.LocationCode == beacon.Minor.ToString())
		{
			instructionField.Text = "Close to landmarks: " + landmarks.Landmarks;

		}

	}
}
*/
/*
if (mapView.MyLocation.HorizontalAccuracy > 20 && !alertshow && !alertshowble)
{

	alertshow = true;
	alertshowble = true;
	//Console.WriteLine(mapView.MyLocation.HorizontalAccuracy + " BLE localization");
	UIAlertView alert = new UIAlertView();
	alert.Title = "Weak GPS signal ";
	alert.AddButton("Cancel");
	alert.AddButton("Start");
	alert.CancelButtonIndex = 0;
	alert.Message = "Accuracy: " + mapView.MyLocation.HorizontalAccuracy +" m"+ "\n" + " If you want to start ble localization, please click Start";
	alert.Clicked += (object s, UIButtonEventArgs ev) =>
	{
		// handle click event here
		if (ev.ButtonIndex != 0)
		{
			UIAlertView alert2 = new UIAlertView();
			alert2.Title = "Started ";
			alert2.AddButton("OK");
			//alert2.CancelButtonIndex = 0;

			alert.Clicked += (object s2, UIButtonEventArgs ev2) =>
			{

			};
			alert2.Show();
			alertshow = false;
		}
		else alertshow = false;

	};

	alert.Show();
}

if (rssi > -90 && !alertshow)
{
	foreach (var landmarks in positionpage.Landmarks)
	{
		if (landmarks.LocationHint == beacon.Major.ToString() && landmarks.LocationCode == beacon.Minor.ToString())
		{
			if (!Receivedlandmarks.Exists(x => x == landmarks))
			{
				Receivedlandmarks.Add(landmarks);
				alertshow = true;

				UIAlertView alert = new UIAlertView();
				alert.Title = "Close to landmarks: " + landmarks.Landmarks;
				alert.AddButton("Cancel");
				alert.AddButton("Show");
				alert.CancelButtonIndex = 0;
				alert.Message = "Address: " + landmarks.StreetAddress + "\n" + " If you want to know more about this landmark, please click Show";
				alert.Clicked += (object s, UIButtonEventArgs ev) =>
			{
				// handle click event here
				if (ev.ButtonIndex != 0)
				{
					var detailpage = new StorePage(landmarks);
					positionpage.Navigate(detailpage);
				}
				alertshow = false;

			};

				alert.Show();
			}

			//Console.WriteLine("Close to landmark");
		}
	}
}

foreach (var marker in markers)
{
	CLLocation landmark = new CLLocation(marker.Position.Latitude, marker.Position.Longitude);
	//Console.WriteLine(mapView.MyLocation.DistanceFrom(landmark));


	if (mapView.MyLocation.DistanceFrom(landmark) < 100 && !alertshow &&!alertshowgps)
	{
		alertshow = true;
		alertshowgps = true;
		//Console.WriteLine(mapView.MyLocation.DistanceFrom(landmark) + " Close");
		UIAlertView alert = new UIAlertView();
		alert.Title = "GPS detected that you are close to a landmark ";
		alert.AddButton("Cancel");
		alert.AddButton("OK");
		alert.CancelButtonIndex = 0;
		alert.Message = "Distance: " + mapView.MyLocation.DistanceFrom(landmark) + "\n" + " If you don't want this notification again, please click cancel";
		alert.Clicked += (object s, UIButtonEventArgs ev) =>
		{
			// handle click event here
			if (ev.ButtonIndex != 0)
			{
				alertshowgps = false;
			}
			alertshow = false;

		};

		alert.Show();
	}

}
*/
