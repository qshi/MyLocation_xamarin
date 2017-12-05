using System;

using Xamarin.Forms;

namespace MyShopAdmin
{
    public class StreetViewPage : ContentPage
    {
        private double markerLatitude, markerLongitute;

        public StreetViewPage(double latitude, double longitute)
        {
            markerLatitude = latitude;
            markerLongitute = longitute;
        }
        public double Latitude
        {
            get
            {
                return markerLatitude;
                
            }
            set
            {
                markerLatitude = value;
            }

        }

        public double Longitude
        {
            get
            {
                return markerLongitute;
            }
            set
            {
                markerLongitute = value;
            }

        }


    }
}

