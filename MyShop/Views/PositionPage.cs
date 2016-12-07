using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using MyShop;
using MvvmHelpers;

namespace MyShop
{
	public class PositionPage : ContentPage
	{


		StoresViewModel ViewModel;

		public PositionPage(StoresViewModel viewmodel)
		{
			ViewModel = viewmodel;
		}

		public PositionPage()
		{
			
			BindingContext = ViewModel = new StoresViewModel(this);
			ViewModel.ForceSync = true;

			if (ViewModel.Stores.Count == 0 || !ViewModel.IsBusy) 
				ViewModel.GetStoresCommand.Execute(null);
			ToolbarItems.Add(new ToolbarItem("Filter", "filter.png", async () =>
			{
				var page = new ContentPage();
				var result = await page.DisplayAlert("Title", "Message", "Accept", "Cancel");

			}));

		}

		/*
		protected override void OnAppearing()
		{
			base.OnAppearing();
			if (ViewModel.Stores.Count == 0 || !ViewModel.IsBusy) ViewModel.GetStoresCommand.Execute(null);
		}
*/
		public ObservableRangeCollection<Store> Landmarks
		{
			get
			{
				if (ViewModel != null)
				{
					return ViewModel.Stores;
				}
				return null;
			}
			set
			{
				if (ViewModel != null)
					ViewModel.Stores = value;
			}

		}
		public async void Navigate(StorePage page)
		{
			await Navigation.PushAsync(page);
			//await Navigation.PopAsync();
		}



	}
}

