
using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using static System.DateTime;

namespace MyShop
{
	public class FeedbackViewModel : ViewModelBase
	{
		IDataStore dataStore;
		public FeedbackViewModel(Page page) : base(page)
		{
			dataStore = DependencyService.Get<IDataStore>();
			Title = "Leave Report";
		}

		public async Task<IEnumerable<Store>> GetStoreAsync()
		{
			if (IsBusy)
				return new List<Store>();

			IsBusy = true;
			try
			{
				return await dataStore.GetStoresAsync() ?? new List<Store>();

			}
			catch (Exception ex)
			{
				await page.DisplayAlert("Uh Oh :(", "Unable to gather locations.", "OK");
			}
			finally
			{
				IsBusy = false;
			}

			return new List<Store>();
		}

		Command saveFeedbackCommand;
		public Command SaveFeedbackCommand
		{
			get
			{
				return saveFeedbackCommand ??
					(saveFeedbackCommand = new Command(async () => await ExecuteSaveFeedbackCommand(), () => { return !IsBusy; }));
			}
		}

		async Task ExecuteSaveFeedbackCommand()
		{
			if (IsBusy)
				return;

			if (string.IsNullOrWhiteSpace(Text))
			{
				await page.DisplayAlert("Enter Report", "Please enter some report for our team.", "OK");
				return;
			}

			Message = "Submitting report...";
			IsBusy = true;
			saveFeedbackCommand?.ChangeCanExecute();

			try
			{
				await dataStore.AddFeedbackAsync(new Feedback
				{
					Text = this.Text,
					FeedbackDate = UtcNow,
					VisitDate = Date,
					Rating = Rating,
					ServiceType = ServiceType,
					StoreName = StoreName,
					Name = Name,
					PhoneNumber = PhoneNumber,
					Longitude = Longitude,
					RequiresCall = RequiresCall,
				});
			}
			catch (Exception ex)
			{
				await page.DisplayAlert("Uh Oh :(", "Unable to save report, please try again.", "OK");
			}
			finally
			{
				IsBusy = false;
				saveFeedbackCommand?.ChangeCanExecute();
			}

			await page.Navigation.PopAsync();

		}

		bool requiresCall = false;
		public bool RequiresCall
		{
			get { return requiresCall; }
			set { SetProperty(ref requiresCall, value); }
		}


		string phone = string.Empty;
		public string PhoneNumber
		{
			get { return phone; }
			set { SetProperty(ref phone, value); }
		}

		string name = string.Empty;
		public string Name
		{
			get { return name; }
			set { SetProperty(ref name, value); }
		}

		string longitude = string.Empty;
		public string Longitude
		{
			get { return longitude; }
			set { SetProperty(ref longitude, value); }
		}


		string message = "Loading...";
		public string Message
		{
			get { return message; }
			set { SetProperty(ref message, value); }
		}

		string text = string.Empty;
		public string Text
		{
			get { return text; }
			set { SetProperty(ref text, value); }
		}

		int serviceType = 4;
		public int ServiceType
		{
			get { return serviceType; }
			set
			{
				SetProperty(ref serviceType, value);
			}
		}

		string rating = string.Empty;
		public string Rating
		{
			get { return rating; }
			set
			{
				SetProperty(ref rating, value);
			}
		}

		DateTime date = Today;
		public DateTime Date
		{
			get { return date; }
			set
			{
				SetProperty(ref date, value);
			}
		}

		//public string StoreName { get; set; } = string.Empty;
		//public string Lattitude { get; set; } = string.Empty;
		string storename = string.Empty;
		public string StoreName
		{
			get { return storename; }
			set
			{
				SetProperty(ref storename, value);
			}
		}

	}
}

