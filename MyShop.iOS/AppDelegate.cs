using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using ImageCircle.Forms.Plugin.iOS;


#if __UNIFIED__

#else
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

using Google.Maps;

namespace MyShop.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override UIWindow Window
		{
			get;
			set;
		}
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			MapServices.ProvideAPIKey("AIzaSyApVhhHzhJF59Qbp3SWmyVaGtKVvx3lhqU");



			UINavigationBar.Appearance.BarTintColor = UIColor.FromRGB(43, 132, 211); //bar background
			UINavigationBar.Appearance.TintColor = UIColor.White; //Tint color of button items
			UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes()
			{
				Font = UIFont.FromName("HelveticaNeue-Light", (nfloat)20f),
				TextColor = UIColor.White
			});
			global::Xamarin.Forms.Forms.Init();

			Xamarin.FormsMaps.Init();


			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();
			SQLitePCL.CurrentPlatform.Init();
			ImageCircleRenderer.Init();


			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}
	}
}

