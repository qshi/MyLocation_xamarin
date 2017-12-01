using System;

using Xamarin.Forms;

namespace MyShop
{
	public class SelectPositionPage : ContentPage
	{
		private FeedbackViewModel _feedbackVm;

		public string Latitude
		{
			get
			{
				if (_feedbackVm != null)
					return _feedbackVm.Latitude;
				return "";
			}
			set
			{
				if (_feedbackVm != null)
					_feedbackVm.Latitude = value;
			}

		}

		public string Longitude
		{
			get
			{
				if (_feedbackVm != null)
					return _feedbackVm.Longitude;
				return "";
			}
			set
			{
				if (_feedbackVm != null)
					_feedbackVm.Longitude = value;
			}

		}

		public SelectPositionPage(FeedbackViewModel feedbackVm)
		{
			_feedbackVm = feedbackVm;
		}


		public async void NavigateBack()
		{

			await Navigation.PopAsync();
		}
	}
}

