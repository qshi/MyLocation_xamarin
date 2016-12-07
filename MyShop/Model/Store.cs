using System;
using Newtonsoft.Json;

namespace MyShop
{
	public class Store
	{

		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }


		[Microsoft.WindowsAzure.MobileServices.Version]
		public string Version { get; set; } = string.Empty; // specific des  bus stop sign   north entrance(building) workzone nothcorner
		public int LandmarksType { get; set; } = 0;
		public string Landmarks { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;    // UUID

		public string LocationHint { get; set; } = string.Empty;  //Major
		public string LocationCode { get; set; } = string.Empty; //Minor
		public int Rssi { get; set; } = 0;

		public string StreetAddress { get; set; } = string.Empty; //street info & direction
		public string City { get; set; } = string.Empty;
		public string State { get; set; } = string.Empty;
		public string Country { get; set; } = string.Empty;
		public string ZipCode { get; set; } = string.Empty;
		public string Image { get; set; } = string.Empty;
		//public string Description { get; set; } = string.Empty;

		[JsonIgnore]
		public Uri ImageUri
		{
			get { return new System.Uri(Image); }
		}

		public double Latitude { get; set; } = 0;
		public double Longitude { get; set; } = 0;

		public string MondayOpen { get; set; } = string.Empty;
		public string MondayClose { get; set; } = string.Empty;
		public string TuesdayOpen { get; set; } = string.Empty;
		public string TuesdayClose { get; set; } = string.Empty;
		public string WednesdayOpen { get; set; } = string.Empty;
		public string WednesdayClose { get; set; } = string.Empty;
		public string ThursdayOpen { get; set; } = string.Empty;
		public string ThursdayClose { get; set; } = string.Empty;
		public string FridayOpen { get; set; } = string.Empty;
		public string FridayClose { get; set; } = string.Empty;
		public string SaturdayOpen { get; set; } = string.Empty;
		public string SaturdayClose { get; set; } = string.Empty;
		public string SundayOpen { get; set; } = string.Empty;
		public string SundayClose { get; set; } = string.Empty;

		public string PhoneNumber { get; set; } = string.Empty;   // service phone



	}
}

