using System;
using System.Threading.Tasks;
using System.Globalization;
using System.Drawing;
using System.Net;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;

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
		private MapDelegate mapDelegate;
		Dictionary<string, Store> landmarkbeacon;
		Dictionary<string, List<int>> beaconrssilist;
		List<Marker> markers;
		Dictionary<string, double> ReceivedBleAvgRssi;
		//List<GBeacon> Receivedbeacons;
		List<string> landmarktypes;

		//bool alertshow = false;
		//bool movingmarker = false;
		//bool alertshowgps = false;
		//bool alertshowble = false;

		UITextView instructionview;
		//UITextView testview;
		/* serach box ini */
		UISearchBar searchBar;
		UIImageView googleAttribution;
		UITableView resultsTable;
		UITableViewSource tableSource;
		public string apiKey { get; set; }
		LocationBias locationBias;
		string CustomPlaceType;
		public event PlaceSelected PlaceSelected;

		CLLocationManager iPhoneLocationManager = null;

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
			nfloat h = 90.0f;
			nfloat w = View.Bounds.Width;
			nfloat maphei= View.Bounds.Height;

			_beaconReciever = new BeaconRecieverManager_IOS();
			markers = new List<Marker>();
			//Receivedbeacons = new List<GBeacon>();
			landmarkbeacon = new Dictionary<string, Store>();
			ReceivedBleAvgRssi = new Dictionary<string, double>();
			beaconrssilist = new Dictionary<string, List<int>>();

			apiKey = "AIzaSyApVhhHzhJF59Qbp3SWmyVaGtKVvx3lhqU";
		     
			CameraPosition camera = CameraPosition.FromCamera(latitude: 42.392262,
														  longitude: -72.526992, zoom: 17);
			mapView = MapView.FromCamera(CGRect.Empty, camera);
			mapView.MyLocationEnabled = true;
			mapView.Settings.MyLocationButton = true;
			mapView.Settings.CompassButton = true;
			mapView.Settings.SetAllGesturesEnabled(true);
			//Init MapDelegate
			mapDelegate = new MapDelegate(mapView);
			mapView.Delegate = mapDelegate;
			View = mapView;

			instructionview = new UITextView()
			{
				Text = "",
				Editable = false,
				Frame = new CGRect(10, 40, w - 20, h)
			};

			/*testview = new UITextView()
			{
				Text = "",
				Editable = false,
				Frame = new CGRect(10, 140, w - 20, h)
			};
			View.AddSubview(testview);*/

			View.AddSubview(instructionview);

			landmarktypes = new List<string>();
			landmarktypes.Add("Work Zone");
			landmarktypes.Add("Bus Stop");
			landmarktypes.Add("Building Entrance");
			landmarktypes.Add("Round About");
			landmarktypes.Add("Traffic Signal");
			landmarktypes.Add("Others");

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

			/* searchbar ini */
			searchBar = new UISearchBar();
			searchBar.TranslatesAutoresizingMaskIntoConstraints = false;
			searchBar.ReturnKeyType = UIReturnKeyType.Done;
			searchBar.Placeholder = "Search";
			searchBar.ShowsCancelButton = true;
			searchBar.CancelButtonClicked += (object sender, EventArgs e) =>
			{
				foreach (var marker in markers) marker.Map = null;
			};
			View.AddSubview(searchBar);
			AddSearchBarConstraints();
			searchBar.BecomeFirstResponder();

			resultsTable = new UITableView();
			tableSource = new ResultsTableSource();
			resultsTable.TranslatesAutoresizingMaskIntoConstraints = false;
			resultsTable.Source = tableSource;
			((ResultsTableSource)resultsTable.Source).apiKey = apiKey;
			((ResultsTableSource)resultsTable.Source).RowItemSelected += OnPlaceSelection;
			View.AddSubview(resultsTable);
			AddResultsTableConstraints();

			searchBar.TextChanged += SearchInputChanged;
			resultsTable.Hidden = true;
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			mapView.StartRendering();

			//CoordinateBounds bounds = null;


			foreach (var landmarks in positionpage.Landmarks)
			{
				string fullid = landmarks.LocationHint + "-" + landmarks.LocationCode;

				CLLocationCoordinate2D pos;
				pos.Latitude = landmarks.Latitude;
				pos.Longitude = landmarks.Longitude;

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
				//markers.Add(testMarker);

			}


			iPhoneLocationManager = new CLLocationManager();
			if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				iPhoneLocationManager.RequestWhenInUseAuthorization();
			}

			iPhoneLocationManager.LocationsUpdated += (sender, e) =>
			{
				myposition = e.Locations[0];
				if (myposition.HorizontalAccuracy >= 10)
				{
					//instructionview.Text = "Weak GPS signal" + "\n" + "Your heading(°):" + myheading + "\n";
				}
				else gpslocation(myposition);
				//Console.WriteLine(e.Locations[0]);
			};
			iPhoneLocationManager.StartUpdatingLocation();

			iPhoneLocationManager.UpdatedHeading += (object sender, CLHeadingUpdatedEventArgs e) =>
			{
				myheading = e.NewHeading.MagneticHeading;
				if (instructionview.Text == "") instructionview.Text = "Your heading:" + myheading.ToString();
				else {
					int headingidx = instructionview.Text.LastIndexOf("Your heading");
					if (headingidx >= 0)
					{
						string newinstruction = instructionview.Text.Substring(0, headingidx);
						newinstruction += "Your heading:" + myheading.ToString();
						instructionview.Text = newinstruction;
					}
				}
				//if (myposition.HorizontalAccuracy <= 50) gpslocation(myposition);
				//Console.WriteLine("!rotation "+e.NewHeading.MagneticHeading.ToString ());
			};

			if (CLLocationManager.HeadingAvailable) iPhoneLocationManager.StartUpdatingHeading();
		}

		public override void ViewWillDisappear(bool animated)
		{
			mapView.StopRendering();
			base.ViewWillDisappear(animated);
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
            if (beacon.Major.ToString() == "999") return;  // 999 --> virtual BLE

		 	var beaconFullId = beacon.Major + "-" + beacon.Minor;
			var rssi = beacon.Rssi;
			var timestamp = DateTime.Now.ToString();// set zero
     
			//string Text = "Please Scan the iBeacons" + "\n" + beaconFullId + ": " + rssi + "dB : " + timestamp;
			//Console.WriteLine(beaconFullId + ": " + rssi + "dB  ");

			if (rssi > -90 && landmarkbeacon.ContainsKey(beaconFullId))   //Started BLE localization...
			{
				//var DistanceMeter = 0.30480000000122 * (Math.Pow(10, (-rssi - 63.5379) / (10 * 2.086)) * 3);
				//testview.Text = beaconFullId + ": " + rssi + "dB;  Distance:" + DistanceMeter;

				double avgrssi = Double.MinValue;

				if (!beaconrssilist.ContainsKey(beaconFullId))
				{
					List<int> rssilist = new List<int>();
					rssilist.Add(rssi);
					beaconrssilist.Add(beaconFullId, rssilist);
				}
				else {
					if (beaconrssilist[beaconFullId].Count < 10)
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

				var cur_landmarktype = landmarktypes[landmarkbeacon[beaconFullId].LandmarksType];
				double avg_rssi = ReceivedBleAvgRssi[beaconFullId];
				var DistanceMeter = 0.30480000000122 * (Math.Pow(10, (-avg_rssi - 63.5379) / (10 * 2.086)) * 3);

				if (beacon.Proximity == GProximity.Far)
				{
					Console.WriteLine(beaconFullId + ": Far BLE!: " + rssi + "dB  ");
				}
				if (beacon.Proximity == GProximity.Immediate)
				{
					instructionview.Text = "BLE detected that you are at: " + landmarkbeacon[beaconFullId].StreetAddress + " " + cur_landmarktype + "\n";
					instructionview.Text += "Your heading:" + myheading;
					//Console.WriteLine(beaconFullId + ": Immediate BLE!: " + rssi + "dB  ");
				}
				if (beacon.Proximity == GProximity.Near)
				{
					if (positionpage.Landmarks != null)
					{
						CLLocation landmark = new CLLocation(landmarkbeacon[beaconFullId].Latitude, landmarkbeacon[beaconFullId].Longitude);
						instructionview.Text = "BLE detected that you are close to: " + landmarkbeacon[beaconFullId].StreetAddress + " " + cur_landmarktype + "\n";
						instructionview.Text += "Distance(meter): " + DistanceMeter.ToString() + "\n";
						//instructionview.Text += "Direction: " + Direction(mylocation, landmark) + "\n";
						instructionview.Text += "Your heading:" + myheading;
					}
					//Console.WriteLine(beaconFullId + ": Near BLE!: " + rssi + "dB  ");
				}
				if (beacon.Proximity == GProximity.Unknown)
				{
					Console.WriteLine(beaconFullId + ": Unknow BLE!: " + rssi + "dB  ");
				}

				if (myposition.HorizontalAccuracy > 9)
				{
					//if (rssi > -75)
					//{
     //                   //
					//	var landmarktype = landmarktypes[landmarkbeacon[beaconFullId].LandmarksType];
					//	instructionview.Text = "BLE detected that you are at: " + landmarkbeacon[beaconFullId].StreetAddress + " " + landmarktype + "\n";
					//	instructionview.Text += "Your heading:" + myheading;
					//}
					//else if (ReceivedBleAvgRssi.ContainsKey(beaconFullId))
						//BleOutdoorNotif(beaconFullId, myposition);
					
					indoorlocalization();
				}
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
				instructionview.Text = "BLE detected that you are close to: " + landmarkbeacon[fullid].StreetAddress + " " + landmarktype + "\n";
				instructionview.Text += "Distance(meter): " + DistanceMeter.ToString() + "\n";
				//instructionview.Text += "Direction: " + Direction(mylocation, landmark) + "\n";
				instructionview.Text += "Your heading:" + myheading;
			}
			//Console.WriteLine("DIS " + DistanceMeter + "full " + fullid + "type " + instructionview.Text);
		}

		//------------------------------------------------------------------------------------------------------------
		void gpslocation(CLLocation mylocation)   //close to landmarks or not
		{
			//instructionview.Text = "Your heading:" + myheading;
			if (positionpage.Landmarks != null)
			{
				//bool findnear = false;
				instructionview.Text = "strong GPS signal " + mylocation.HorizontalAccuracy + "\n";

				foreach (var landmarks in positionpage.Landmarks)
				{
					CLLocation landmark = new CLLocation(landmarks.Latitude, landmarks.Longitude);
					if (landmarks.LandmarksType == 5) continue; // skip indoor beacons
					var landmarktype = landmarktypes[landmarks.LandmarksType];

					if (mylocation.DistanceFrom(landmark) < 40)
					{
						//findnear = true;
						instructionview.Text += "GPS detected that you are close to: " + landmarks.StreetAddress + " " + landmarktype + "\n";
						instructionview.Text += "Direction: " + Direction(mylocation, landmark) + "\n";
						instructionview.Text += "Distance: " + mylocation.DistanceFrom(landmark) + "\n";
					}

				}
				instructionview.Text += "Your heading:" + myheading + "\n";
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

					return "Southeast" + "\n" + clockdirection(landmarkheading);
				}

				if (a.Coordinate.Longitude > b.Coordinate.Longitude)//west  D3
				{
						/*
						if (myheading > 180 - landmarkbearing && myheading <= 270 - landmarkbearing) dir = "The landmark is on your left front";
						if (myheading <= 180 - landmarkbearing || myheading > 90 - landmarkbearing) dir = "The landmark is on your right front";
						if (myheading > 270 - landmarkbearing || myheading <= -landmarkbearing) dir = "The landmark is on your left behind";
						if (myheading > -landmarkbearing && myheading <= 90 - landmarkbearing) dir = "The landmark is on your right behind";
						*/
					return "Southwest" + "\n" + clockdirection(landmarkheading);
				}

				return "South" + "\n" + clockdirection(landmarkheading);
				

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
					return "Northeast" + "\n" + clockdirection(landmarkheading);
				}
				if (a.Coordinate.Longitude > b.Coordinate.Longitude)  //West    D4
				{
					/*
					if (myheading > landmarkbearing + 270 && myheading < landmarkbearing + 360) dir = "The landmark is on your right front";
					if ((myheading < landmarkbearing + 90 && myheading > 0) || myheading > landmarkbearing + 360) dir = "The landmark is on your left front";
					if (myheading > landmarkbearing + 90 && myheading < landmarkbearing + 180) dir = "The landmark is on your left behind";
					if (myheading > landmarkbearing + 180 && myheading < landmarkbearing + 270) dir = "The landmark is on your right behind";
					*/
					return "Northwest" + "\n" + clockdirection(landmarkheading);
				}

				return "North" + "\n" + clockdirection(landmarkheading);

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
					return "East" + "\n" + clockdirection(landmarkheading);
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
					return "West" +"\n"  + clockdirection(landmarkheading);

				}

			}
		}

		string clockdirection(double landmarkheading)
		{
			string clock = "The landmark is at your ";

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
