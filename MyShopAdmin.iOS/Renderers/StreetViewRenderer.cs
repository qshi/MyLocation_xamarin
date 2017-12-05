using System;
using MyShop;
using Xamarin.Forms;
using MyShopAdmin;
using MyShopAdmin.iOS;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using Foundation;
using CoreGraphics;

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

[assembly: ExportRenderer(typeof(StreetViewPage), typeof(StreetViewRenderer))]
namespace MyShopAdmin.iOS
{
    public class StreetViewRenderer : PageRenderer
    {
        public StreetViewRenderer()
        {
        }
        public StreetViewPage streetviewpage
        {
            get
            {
                return Element as StreetViewPage;
            }
        }

        public override void LoadView()
        {
            base.LoadView();
            PanoramaView panoView = new PanoramaView();
            View = panoView;
            CLLocationCoordinate2D marker = new CLLocationCoordinate2D(streetviewpage.Latitude, streetviewpage.Longitude);
            panoView.MoveNearCoordinate(marker);
        }

    }
}
