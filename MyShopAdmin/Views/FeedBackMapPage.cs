using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using MyShop;

namespace MyShopAdmin
{
	public class FeedBackMapPage : ContentPage
	{
		FeedbackListViewModel viewModel;

		public ObservableCollection<Feedback> Feedbacks
		{
			get
			{
				if (viewModel != null)
				{
					return viewModel.Feedbacks;
				}
				return null;
			}
			set
			{
				if (viewModel != null)
					viewModel.Feedbacks = value;
			}

		}


		protected override void OnAppearing()
		{
			base.OnAppearing();


			if (viewModel.Feedbacks.Count == 0)
				viewModel.GetFeedbackCommand.Execute(null);
		}




		public FeedBackMapPage(FeedbackListViewModel vm)
		{
			viewModel = vm;
		}

        public async void NavigateToStreetView(double latitude, double longitute)
        {
            await Navigation.PushAsync(new StreetViewPage(latitude, longitute));
            //await Navigation.PopAsync();

        }

	}
}

