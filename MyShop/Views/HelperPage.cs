using System;

using Xamarin.Forms;

namespace MyShop
{
    public class HelperPage : ContentPage
    {
        string action;
        public HelperPage(string action)
        {
            this.action = action;
            //Content = new StackLayout
            //{
            //    Children = {
            //        new Label { Text = "Hello ContentPage" }
            //    }
            //};
        }
        public string Action {
			get
			{
				if (action != null)
				{
					return action;
				}
				return null;
			}
			set
			{
				if (action != null)
					action = value;
			}

        }
		public async void Navigate(StorePage page)
		{
			await Navigation.PushAsync(page);
			//await Navigation.PopAsync();
		}
    }
}

