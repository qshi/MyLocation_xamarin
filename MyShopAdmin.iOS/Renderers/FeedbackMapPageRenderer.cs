using System;
using MyShop;
using Xamarin.Forms;
using MyShopAdmin;
using MyShopAdmin.iOS;
using Xamarin.Forms.Platform.iOS;
using System.Collections.ObjectModel;
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

[assembly: ExportRenderer(typeof(FeedBackMapPage), typeof(FeedbackMapPageRenderer))]
namespace MyShopAdmin.iOS
{
	public class FeedbackMapPageRenderer : PageRenderer
	{
		public FeedbackMapPageRenderer()
		{
			
		}
		MapView mapView;
		//List<Marker> markers;

		public FeedBackMapPage _feedbackMapPage
		{
			get
			{
				return Element as FeedBackMapPage;
			}
		}



		public override void LoadView()
		{
			base.LoadView();

			CameraPosition camera = CameraPosition.FromCamera(latitude: 42.392262,
														  longitude: -72.526992,
														  zoom: 17);
			mapView = MapView.FromCamera(CGRect.Empty, camera);
			mapView.MyLocationEnabled = true;

			View = mapView;

			//mapView.CoordinateLongPressed += HandleLongPress;


		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			mapView.StartRendering();

			foreach (var feedback in _feedbackMapPage.Feedbacks)
			{
				CLLocationCoordinate2D pos;
				pos.Latitude = Convert.ToDouble(feedback.Latitude);
				pos.Longitude = Convert.ToDouble(feedback.Longitude);

				var marker = new Marker()
				{
					Title = string.Format("Landmarks at: {0}, {1}", pos.Latitude, pos.Longitude),
					Snippet = string.Format("{0} : {1} ; Submitted by {2}", feedback.ServiceType, feedback.Text, feedback.Name),
					Position = pos,
					AppearAnimation = MarkerAnimation.Pop,

					Tappable = true,

					Map = mapView


				};
			}

			

		}



			


		

		public override void ViewWillDisappear(bool animated)
		{
			mapView.StopRendering();
			base.ViewWillDisappear(animated);
		}
			/*
		void HandleLongPress(object sender, GMSCoordEventArgs e)
		{
			var marker = new Marker()
			{
				Title = string.Format("Marker at: {0}, {1}", e.Coordinate.Latitude, e.Coordinate.Longitude),
				Position = e.Coordinate,
				AppearAnimation = MarkerAnimation.Pop,
				Map = mapView
			};

			// Add the new marker to the list of markers.
			//markers.Add(marker);
		}*/
	}
}

