using System;
using MyShop;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MyShopAdmin
{
	public class StoreViewModel : ViewModelBase
	{
		public StoreViewModel(Page page) : base(page)
		{
		}

		double latitude = 0;
		public double Latitude
		{
			get { return latitude; }
			set { SetProperty(ref latitude, value); }
		}

		double longitude = 0;
		public double Longitude
		{
			get { return longitude; }
			set { SetProperty(ref longitude, value); }
		}

		string uuid = string.Empty;
		public string Uuid
		{
			get { return uuid; }
			set { SetProperty(ref uuid, value); }
		}
		public static double weight = 96640;

		long minor = 0;
		public long Minor
		{
			get { return minor; }
			set { SetProperty(ref minor, value); }
		}

		long major = 0;
		public long Major
		{
			get { return major; }
			set { SetProperty(ref major, value); }
		}

		public int Rssi { get; set; }


		public double DisInMeter
		{
			get { return 0.30480000000122 * (Math.Pow(10, (-this.Rssi - 63.5379) / (10 * 2.086)) * 3); }

		}
		public string FullId
		{
			get { return this.Major + "-" + this.Minor; }
		}
		public double DisInRadians
		{
			get { return this.DisInMeter / weight; }
		}
		public GProximity Proximity { get; set; }
		public DateTime Timestamp { get; set; }


		public bool IsBeaconTimestampRecent(int recentTimeThreshold)
		{
			var timeElapsed = DateTime.Now - this.Timestamp;
			if (timeElapsed.Seconds < recentTimeThreshold)
			{
				return true;
			}
			return false;
		}
	}

	public enum GProximity
	{
		Unknown = 1,
		Far = 2,
		Near = 3,
		Immediate = 4
	}


}

