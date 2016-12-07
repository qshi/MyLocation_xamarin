using System;

using Xamarin.Forms;

namespace MyShopAdmin
{
	public class SelectBLELLocationPage : ContentPage
	{
		private StoreViewModel viewmodel;

		public double Latitude
		{
			get
			{
				if (viewmodel != null)
					return viewmodel.Latitude;
				return 0;
			}
			set
			{
				if (viewmodel != null)
					viewmodel.Latitude = value;
			}

		}

		public double Longitude
		{
			get
			{
				if (viewmodel != null)
					return viewmodel.Longitude;
				return 0;
			}
			set
			{
				if (viewmodel != null)
					viewmodel.Longitude = value;
			}

		}


		public SelectBLELLocationPage(StoreViewModel ViewModel)
		{
			viewmodel = ViewModel;
		}

		public async void NavigateBack()
		{

			await Navigation.PopAsync();

		}
	}
}

