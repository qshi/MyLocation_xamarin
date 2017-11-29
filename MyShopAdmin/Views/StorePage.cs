using System;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using Xamarin.Forms;
using MyShop;
using System.CodeDom.Compiler;

namespace MyShopAdmin
{
	public class StorePage : ContentPage
	{
		Store Store { get; set; }
		StoreViewModel ViewModel;
		bool isNew;
		EntryCell locationCode, mondayOpen, mondayClose, tuesdayOpen, tuesdayClose, wednesdayOpen, wednesdayClose,
		thursdayOpen, thursdayClose, fridayOpen, fridayClose, saturdayOpen, saturdayClose, sundayOpen, sundayClose,
		phoneNumber, streetAddress, city, state, zipCode, country, name, locationHint, imageUrl;
		TextCell latitude, longitude, detectLatLong, refreshImage, scaniBeacon, selectlocation;
		Picker version;
		PickerCell versioncell;
		Image image;
		readonly IDataStore dataStore;

		public StorePage(Store store)
		{

			dataStore = DependencyService.Get<IDataStore>();
			Store = store;
			if (Store == null)
			{
				Store = new Store();
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
				Store.LandmarksType = 1;
				isNew = true;
            } else if (Store.ISNew) {
                isNew = true;
            }

			BindingContext = ViewModel = new StoreViewModel(this);

			Title = isNew ? "New Landmark" : "Edit Landmark";

			var uuid = new EntryCell { Label = "UUID" };
			uuid.SetBinding(EntryCell.TextProperty, "Uuid");

			var major = new EntryCell { Label = "MAJOR" };
			major.SetBinding(EntryCell.TextProperty, "Major");

			var minor = new EntryCell { Label = "MINOR" };
			minor.SetBinding(EntryCell.TextProperty, "Minor");

			var newlatitude = new TextCell { };
			newlatitude.SetBinding(TextCell.TextProperty, "Latitude");

			var newlongitude = new TextCell { Text = "Longitude" };
			newlongitude.SetBinding(TextCell.TextProperty, "Longitude");

			version = new Picker()
			{
				Title = "type",
				//VerticalOptions = LayoutOptions.CenterAndExpand

			};
			version.Items.Add("Work Zone");
			version.Items.Add("Bus Stop");
			version.Items.Add("Building Entrance");
			version.Items.Add("Round About");
			version.Items.Add("Traffic Signal");
			version.Items.Add("Others");
			version.SelectedIndex = Store.LandmarksType;


			ToolbarItems.Add(new ToolbarItem
			{
				Text = "Save",
				Command = new Command(async (obj) =>
					{
						Store.Name = name.Text.Trim();
						Store.LocationHint = locationHint.Text.Trim();
						Store.City = city.Text.Trim();
						Store.PhoneNumber = phoneNumber.Text.Trim();
						Store.Image = imageUrl.Text.Trim();
						Store.StreetAddress = streetAddress.Text.Trim();
						Store.State = state.Text.Trim();
						Store.ZipCode = zipCode.Text.Trim();
						Store.LocationCode = locationCode.Text.Trim();
						Store.Country = country.Text.Trim();
						double lat;
						double lng;

						var parse1 = double.TryParse(latitude.Text.Trim(), out lat);
						var parse2 = double.TryParse(longitude.Text.Trim(), out lng);
						Store.Longitude = lng;
						Store.Latitude = lat;
						Store.MondayOpen = mondayOpen.Text.Trim();
						Store.MondayClose = mondayClose.Text.Trim();
						Store.TuesdayOpen = tuesdayOpen.Text.Trim();
						Store.TuesdayClose = tuesdayClose.Text.Trim();
						Store.WednesdayOpen = wednesdayOpen.Text.Trim();
						Store.WednesdayClose = wednesdayClose.Text.Trim();
						Store.ThursdayOpen = thursdayOpen.Text.Trim();
						Store.ThursdayClose = thursdayClose.Text.Trim();
						Store.FridayOpen = fridayOpen.Text.Trim();
						Store.FridayClose = fridayClose.Text.Trim();
						Store.SaturdayOpen = saturdayOpen.Text.Trim();
						Store.SaturdayClose = saturdayClose.Text.Trim();
						Store.SundayOpen = sundayOpen.Text.Trim();
						Store.SundayClose = sundayClose.Text.Trim();

						Store.LandmarksType = version.SelectedIndex;
						Store.Landmarks = version.Items[version.SelectedIndex];
						Debug.WriteLine(Store.Landmarks);

						bool isAnyPropEmpty = Store.GetType().GetTypeInfo().DeclaredProperties
							.Where(p => p.GetValue(Store) is string && p.CanRead && p.CanWrite && p.Name != "State") // selecting only string props
							.Any(p => string.IsNullOrWhiteSpace((p.GetValue(Store) as string)));

						if (!parse1 || !parse2)
						{
							await DisplayAlert("Not Valid", "Some fields are not valid, please check", "OK");
							return;
						}
						Title = "SAVING...";
						if (isNew)
						{
							await dataStore.AddStoreAsync(Store);
						}
						else
						{
							await dataStore.UpdateStoreAsync(Store);
						}

						await DisplayAlert("Saved", "Please refresh iBeacons list", "OK");
						await Navigation.PopAsync();
					})
			});


			Content = new TableView
			{
				HasUnevenRows = true,
				Intent = TableIntent.Form,
				Root = new TableRoot {
					
					new TableSection ("Address") {
						(streetAddress = new EntryCell {Label = "Name", Text = Store.StreetAddress }),
                        (versioncell = new PickerCell()
                        {
                            //Label = "Type",
                            Picker = version

                        }),
						(city = new EntryCell {Label = "City", Text = Store.City }),
						(state = new EntryCell {Label = "State", Text = Store.State }),
						(zipCode = new EntryCell {Label = "Zipcode", Text = Store.ZipCode }),
						(country = new EntryCell{Label="Description", Text = Store.Country}),

						(latitude = (isNew)? newlatitude :new TextCell {Text = Store.Latitude.ToString() }),
						(longitude = (isNew)? newlongitude : new TextCell {Text = Store.Longitude.ToString() }),
						(selectlocation = new TextCell()
							{
								Text="Select Location"
							}),
						(detectLatLong = new TextCell()
							{
								Text="Detect Lat/Long"
							})
					},

                    new TableSection ("Image") {
                        (imageUrl = new EntryCell { Label="Image URL", Text = Store.Image, Placeholder = ".png or .jpg image link" }),
                        (refreshImage = new TextCell()
                            {
                                Text="Refresh Image"
                            }),
                        new ViewCell { View = (image = new Image
                            {
                                HeightRequest = 400,
                                VerticalOptions = LayoutOptions.FillAndExpand
                            })
                        }
                    },

                    new TableSection ("Information") {
                        (name = (isNew)? uuid : new EntryCell {Label = "UUID", Text = Store.Name}),

                        (locationHint = (isNew)? major : new EntryCell {Label = "Major ID", Text = Store.LocationHint}),
                        (locationCode = (isNew)? minor : new EntryCell {Label = "Minor ID", Text = Store.LocationCode}),
                        (scaniBeacon = new TextCell()
                            {
                                Text="Scan iBeacon"
                            }),
                        (phoneNumber = new EntryCell {Label = "Phone Number", Text = Store.PhoneNumber, Placeholder ="555-555-5555"})

                    },
					//new TableSection ("Hours") {
					//	(mondayOpen = new EntryCell {Label = "Monday Open", Text = Store.MondayOpen}),
					//	(mondayClose = new EntryCell {Label = "Monday Close", Text = Store.MondayClose}),
					//	(tuesdayOpen = new EntryCell {Label = "Tuesday Open", Text = Store.TuesdayOpen}),
					//	(tuesdayClose = new EntryCell {Label = "Tuesday Close", Text = Store.TuesdayClose}),
					//	(wednesdayOpen = new EntryCell {Label = "Wedneday Open", Text = Store.WednesdayOpen}),
					//	(wednesdayClose = new EntryCell {Label = "Wedneday Close", Text = Store.WednesdayClose}),
					//	(thursdayOpen = new EntryCell {Label = "Thursday Open", Text = Store.ThursdayOpen}),
					//	(thursdayClose = new EntryCell {Label = "Thursday Close", Text = Store.ThursdayClose}),
					//	(fridayOpen = new EntryCell {Label = "Friday Open", Text = Store.FridayOpen}),
					//	(fridayClose = new EntryCell {Label = "Friday Close", Text = Store.FridayClose}),
					//	(saturdayOpen = new EntryCell {Label = "Saturday Open", Text = Store.SaturdayOpen}),
					//	(saturdayClose =new EntryCell {Label = "Saturday Close", Text = Store.SaturdayClose}),
					//	(sundayOpen = new EntryCell {Label = "Sunday Open", Text = Store.SundayOpen}),
					//	(sundayClose = new EntryCell {Label = "Sunday Close", Text = Store.SundayClose}),
					//},
				},
			};

			refreshImage.Tapped += (sender, e) =>
			{
				image.Source = ImageSource.FromUri(new Uri(imageUrl.Text));
			};

			detectLatLong.Tapped += async (sender, e) =>
			{
				var coder = new Xamarin.Forms.Maps.Geocoder();
				var oldTitle = Title;
				Title = "Please wait...";
				var locations = await coder.GetPositionsForAddressAsync(streetAddress.Text + " " + city.Text + ", " + state.Text + " " + zipCode.Text + " " + country.Text);
				Title = oldTitle;
				foreach (var location in locations)
				{
					latitude.Text = location.Latitude.ToString();
					longitude.Text = location.Longitude.ToString();
					break;
				}
			};
			scaniBeacon.Tapped += async (sender, e) =>
			{
				var myNextPage = new BLEViewPage(ViewModel);
				//myNextPage.BindingContext = Store;
				await Navigation.PushAsync(myNextPage);
			};

			selectlocation.Tapped += async (sender, e) =>
			{
				var myNextPage = new SelectBLELLocationPage(ViewModel);
				//myNextPage.BindingContext = Store;
				await Navigation.PushAsync(myNextPage);
			};


			SetBinding(Page.IsBusyProperty, new Binding("IsBusy"));

		}

	

	}
}


