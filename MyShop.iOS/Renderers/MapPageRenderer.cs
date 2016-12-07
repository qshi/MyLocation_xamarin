using System;
using Xamarin.Forms;
using MyShop;
using MyShop.iOS;
using Xamarin.Forms.Platform.iOS;
using System.Diagnostics;

#if __UNIFIED__
using UIKit;
using CoreLocation;
using CoreGraphics;

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

[assembly: ExportRenderer(typeof(SelectPositionPage), typeof(MapPageRenderer))]

namespace MyShop.iOS
{
	public class MapPageRenderer : PageRenderer
	{

		private MapView mapView;
		List<Marker> markers;
		//double latti=0, longi=0;
		public SelectPositionPage _selectPagePosition
		{
			get
			{
				return Element as SelectPositionPage;
			}
		}

		public override void LoadView()
		{
			base.LoadView();

			CameraPosition camera = CameraPosition.FromCamera(latitude: 42.392262,
														  longitude: -72.526992,
														  zoom: 6);
			mapView = MapView.FromCamera(CGRect.Empty, camera);
			mapView.MyLocationEnabled = true;

			View = mapView;
			mapView.CoordinateLongPressed += HandleLongPress;

			/*
			var addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add, DidTapAdd);
			var clearButton = new UIBarButtonItem("Clear Markers", UIBarButtonItemStyle.Plain, (o, s) =>
			{
				mapView.Clear();

			});

			NavigationItem.RightBarButtonItems = new[] { addButton, clearButton };
*/
			mapView.TappedMarker = (aMapView, aMarker) =>
			{

				// Animate to the marker
				var cam = new CameraPosition(aMarker.Position, 8, 50, 60);
				mapView.Animate(cam);
				UIAlertView alert = new UIAlertView();
				alert.Title = "Are you sure you want to select this position as the landmarks position?";
				alert.AddButton("Cancel");
				alert.AddButton("OK");
				alert.CancelButtonIndex = 0;
				alert.Message = " If yes, please click OK";
				//alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
				alert.Clicked += (object s, UIButtonEventArgs ev) =>
				{
					// handle click event here
					if (ev.ButtonIndex != 0)
					{
						_selectPagePosition.Longitude = aMarker.Position.Longitude.ToString();
						_selectPagePosition.Latitude = aMarker.Position.Latitude.ToString();

						_selectPagePosition.NavigateBack();

					}

					//var a = Page.Navigation.NavigationStack[0];

					// user input will be in alert.GetTextField(0).Text;
				};

				alert.Show();

				// Melbourne marker has a InfoWindow so return NO to allow markerInfoWindow to
				// fire. Also check that the marker isn't already selected so that the
				// InfoWindow doesn't close.
				/*if (aMarker == melbourneMarker && mapView.SelectedMarker != melbourneMarker)
				{
					return false;
				}*/
				// The Tap has been handled so return YES
				return true;
			};

		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			mapView.StartRendering();
		}

		public override void ViewWillDisappear(bool animated)
		{
			mapView.StopRendering();
			base.ViewWillDisappear(animated);
		}

		void HandleLongPress(object sender, GMSCoordEventArgs e)
		{
			mapView.Clear();
			var marker = new Marker()
			{
				Title = string.Format("Marker at: {0}, {1}", e.Coordinate.Latitude, e.Coordinate.Longitude),
				Position = e.Coordinate,
				AppearAnimation = MarkerAnimation.Pop,
				Map = mapView
			};
			//latti = e.Coordinate.Latitude;
			//longi = e.Coordinate.Longitude;
			// Add the new marker to the list of markers.
			//markers.Add(marker);
		}




	}
}
