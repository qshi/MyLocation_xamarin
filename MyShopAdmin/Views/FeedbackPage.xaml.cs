using System;
using System.Collections.Generic;

using Xamarin.Forms;
using MyShop;
using Plugin.Messaging;
using MyShop.Helpers;

namespace MyShopAdmin.Views
{
	public partial class FeedbackPage : ContentPage
	{
		public FeedbackPage (Feedback feedback)
		{
			InitializeComponent ();

			this.BindingContext = feedback;

			ButtonCall.Clicked += (sender, e) => {
                
			var phoneCallTask = MessagingPlugin.PhoneDialer;
			if (phoneCallTask.CanMakePhoneCall)
                phoneCallTask.MakePhoneCall(feedback.PhoneNumber.CleanPhone());
			};

            ToolbarItems.Add(new ToolbarItem
            {
                Text = "Add to Landmarks",
                Command = new Command(async (obj) =>
                {
                    Store Store = new Store();
                    Store.MondayOpen = "9am";
                    Store.TuesdayOpen = "9am";
                    Store.WednesdayOpen = "9am";
                    Store.ThursdayOpen = "9am";
                    Store.FridayOpen = "9am";
                    Store.SaturdayOpen = "9am";
                    Store.SundayOpen = "12pm";
                    Store.MondayClose = "8pm";
                    Store.TuesdayClose = "8pm";
                    Store.WednesdayClose = "8pm";
                    Store.ThursdayClose = "8pm";
                    Store.FridayClose = "8pm";
                    Store.SaturdayClose = "8pm";
                    Store.SundayClose = "6pm";
                    //Store.Name = "";
                    Store.StreetAddress = feedback.StoreName;
                    Store.LandmarksType = feedback.ServiceType;
                    Store.Country = feedback.Text;
                    Store.Longitude = Convert.ToDouble(feedback.Longitude);
                    Store.Latitude = Convert.ToDouble(feedback.Latitude);    //!!?
                    Store.ISNew = true;
                    await Navigation.PushAsync(new StorePage(Store));

                })

            });
		}
	}
}

