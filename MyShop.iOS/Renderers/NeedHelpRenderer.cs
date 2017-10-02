using System;
using System.Collections.Generic;
using CoreBluetooth;

//using System.Threading.Tasks;
//using System.Globalization;
//using System.Drawing;
//using System.Net;
//using System.CodeDom.Compiler;
//using System.IO;
//using System.Linq;
using Xamarin.Forms;
using MyShop;
using MyShop.iOS;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using BleIosExample.BLE;

[assembly: ExportRenderer(typeof(HelperPage), typeof(NeedHelpRenderer))]

namespace MyShop.iOS
{
    public class PeripheralManagerDelegate : CBPeripheralManagerDelegate
    {
        public override void StateUpdated(CBPeripheralManager peripheral)
        {
        }
    }
    public class NeedHelpRenderer : PageRenderer
    {
        ConfigurationViewController tableViewController;
        RangingViewController rangeViewController;

		public HelperPage helperpage
		{
			get
			{
				return Element as HelperPage;
			}
		}

        public NeedHelpRenderer()
        {
        }
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
         
            if (helperpage.Action == "I need help") {
				tableViewController = new ConfigurationViewController(); // Use FromString() to play video directly from web.
				this.View.AddSubview(tableViewController.View); // add the view after video starts playing to display it
                //View = tableViewController.View
			}
			if (helperpage.Action == "Search for people in need nearby")
			{
				rangeViewController = new RangingViewController(UITableViewStyle.Plain); // Use FromString() to play video directly from web.
				this.View.AddSubview(rangeViewController.View); // add the view after video starts playing to display it
			}
		}

		public override void ViewDidLayoutSubviews()
		{
			if (helperpage.Action == "I need help")  tableViewController.View.Frame = new CoreGraphics.CGRect(0, 0, this.View.Bounds.Size.Width, this.View.Bounds.Size.Height); // size of the video frame
            if (helperpage.Action == "Search for people in need nearby")  rangeViewController.View.Frame = new CoreGraphics.CGRect(0, 0, this.View.Bounds.Size.Width, this.View.Bounds.Size.Height);
		}

    }


}

