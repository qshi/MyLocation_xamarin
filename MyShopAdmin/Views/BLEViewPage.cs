using System;
using System.Reflection;
using System.Linq;

using Xamarin.Forms;
using MyShop;

namespace MyShopAdmin
{
	public class BLEViewPage : ContentPage
	{
		private StoreViewModel viewmodel;
		/*
			public string UUID
			{
				get
				{
					if ( Store!= null)
						return Store.Name;
					return "";
				}
				set
				{
					if (Store != null)
						Store.Name = value;
				}

			}*/
		public string UUID
		{
			get
			{
				if (viewmodel != null)
					return viewmodel.Uuid;
				return "";
			}
			set
			{
				if (viewmodel != null)
					viewmodel.Uuid = value;
			}

		}

		public long MINOR
		{
			get
			{
				if (viewmodel != null)
					return viewmodel.Minor;
				return 0;
			}
			set
			{
				if (viewmodel != null)
					viewmodel.Minor = value;
			}

		}

		public long MAJOR
		{
			get
			{
				if (viewmodel != null)
					return viewmodel.Major;
				return 0;
			}
			set
			{
				if (viewmodel != null)
					viewmodel.Major = value;
			}

		}

		public BLEViewPage(StoreViewModel viewModel)
		{
			viewmodel = viewModel;
			//BindingContext = Store;

		}

		public async void NavigateBack()
		{

			await Navigation.PopAsync();

		}

	}
}

