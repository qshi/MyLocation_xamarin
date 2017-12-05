using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace MyShopAdmin
{
	public partial class HomePage : ContentPage
	{
		public HomePage()
		{
			InitializeComponent();
			Title = "Admin App";
			ButtonManage.Clicked += async (sender, e) =>
			{
				await Navigation.PushAsync(new StoresPage());
			};

			ButtonFeedback.Clicked += async (sender, e) =>
			{
				await Navigation.PushAsync(new FeedbackListPage());
			};
			ButtonFeedbackMap.Clicked += async (sender, e) =>
			{
				//await Navigation.PushAsync(new FeedBackMapPage());
			};
		}
	}
}

