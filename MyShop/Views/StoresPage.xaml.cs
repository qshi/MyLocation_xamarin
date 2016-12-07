using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace MyShop
{
	public partial class StoresPage : ContentPage
	{
        public StoresViewModel viewModel;
        public Action<Store> ItemSelected
        {
            get { return viewModel.ItemSelected; }
            set { viewModel.ItemSelected = value; }
        }
        public StoresPage ()
		{
			InitializeComponent ();
			BindingContext = viewModel = new StoresViewModel (this);
            
			if(Device.OS == TargetPlatform.WinPhone || (Device.OS == TargetPlatform.Windows && Device.Idiom == TargetIdiom.Phone))
			{
				//StoreList.IsGroupingEnabled = false;
				//StoreList.ItemsSource = viewModel.Stores;
			}

			ToolbarItems.Add(new ToolbarItem
			{
				Text = "Map",
				Command = new Command(async (obj) =>
					{
						var selectpos = new PositionPage(viewModel);
						await Navigation.PushAsync(selectpos);

					})

			});
		}

		protected override void OnAppearing ()
		{
			base.OnAppearing ();
			if (viewModel.Stores.Count > 0 || viewModel.IsBusy)
				return;

			viewModel.GetStoresCommand.Execute (null);
		}
	}
}

