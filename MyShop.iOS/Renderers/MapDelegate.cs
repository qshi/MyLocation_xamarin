using Google.Maps;
using System;
using System.Threading.Tasks;
using System.Globalization;
using System.Net;
using System.Collections.Generic;
using UIKit;
using CoreLocation;
using Newtonsoft.Json;

namespace MyShop.iOS
{
	public class MapDelegate : MapViewDelegate
	{
		//Base URL for Directions Service
		const string KMdDirectionsUrl = @"http://maps.googleapis.com/maps/api/directions/json?origin=";

		public readonly List<CLLocationCoordinate2D> Locations;
		private readonly MapView _map;
		public readonly List<Google.Maps.Polyline> Lines;

		public MapDelegate(MapView map)
		{
			Locations = new List<CLLocationCoordinate2D>();
			Lines = new List<Google.Maps.Polyline>();
			_map = map;
		}

        public override bool TappedMarker(MapView mapView, Marker marker) {
            var cam = new CameraPosition(marker.Position, 18, 0, 0);
            mapView.Animate(cam);
            UIAlertView alert = new UIAlertView();
            alert.Title = Variables.isSelectingDestination ? "Cancel previous destinsation?" : (Locations.Count > 0) ? "Select this position as way point?": "Select this position as destination?";
            alert.AddButton("Cancel");
            alert.AddButton("OK");
            alert.CancelButtonIndex = 0;
            alert.Message = " If yes, please click OK";
            //alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
            alert.Clicked += (object s, UIButtonEventArgs ev) =>
            {
                if (ev.ButtonIndex != 0)
                {
                    if (Variables.isSelectingDestination)
                    {
                        Variables.isSelectingDestination = false;
                        Locations.Clear();
                        if (Lines.Count > 0)
                        {
                            foreach (var line in Lines)
                            {
                                line.Map = null;
                            }
                            Lines.Clear();
                        }
                    }
                    else
                    {
                        Variables.isSelectingDestination = true;
                        Variables.destinationMarker = marker;
                        Locations.Add(mapView.MyLocation.Coordinate);
                        Locations.Add(marker.Position);
                        if (Locations.Count > 1)
                        {
                            SetDirectionsQuery();
                        }
                    }
                }
            };
            alert.Show();
            return true;
        }

        public override void DidLongPressAtCoordinate(MapView mapView, CLLocationCoordinate2D coordinate)
		{

			//Create/Add Marker 
			var marker = new Marker { Position = coordinate, Icon = Marker.MarkerImage(UIColor.Orange),Map = mapView };
			Locations.Add(coordinate);

			if (Locations.Count > 1)
			{
				SetDirectionsQuery();
			}
		}

		private async void SetDirectionsQuery()
		{
			//Clear Old Polylines
			if (Lines.Count > 0)
			{
				foreach (var line in Lines)
				{
					line.Map = null;
				}
				Lines.Clear();
			}

			//Start building Directions URL
			var sb = new System.Text.StringBuilder();
			sb.Append(KMdDirectionsUrl);
			sb.Append(Locations[0].Latitude.ToString(CultureInfo.InvariantCulture));
			sb.Append(",");
			sb.Append(Locations[0].Longitude.ToString(CultureInfo.InvariantCulture));
			sb.Append("&");
			sb.Append("destination=");
			sb.Append(Locations[1].Latitude.ToString(CultureInfo.InvariantCulture));
			sb.Append(",");
			sb.Append(Locations[1].Longitude.ToString(CultureInfo.InvariantCulture));
            sb.Append("&mode=walking");
			sb.Append("&sensor=true");

			//If we have more than 2 locations we'll append waypoints
			if (Locations.Count > 2)
			{
				sb.Append("&waypoints=");
				for (var i = 2; i < Locations.Count; i++)
				{
					if (i > 2)
						sb.Append("|");
					sb.Append(Locations[i].Latitude.ToString(CultureInfo.InvariantCulture));
					sb.Append(",");
					sb.Append(Locations[i].Longitude.ToString(CultureInfo.InvariantCulture));
				}
			}

			//Get directions through Google Web Service
			var directionsTask = GetDirections(sb.ToString());

			var jSonData = await directionsTask;

			//Deserialize string to object
			var routes = JsonConvert.DeserializeObject<RootObject>(jSonData);

			foreach (var route in routes.routes)
			{
				//Encode path from polyline passed back
				var path = Path.FromEncodedPath(route.overview_polyline.points);

				//Create line from Path
				var line = Google.Maps.Polyline.FromPath(path);
				line.StrokeWidth = 10f;
				line.StrokeColor = UIColor.Red;
				line.Geodesic = true;

				//Place line on map
				line.Map = _map;
				Lines.Add(line);

			}

		}

		private async Task<String> GetDirections(string url)
		{
			var client = new WebClient();
			var directionsTask = client.DownloadStringTaskAsync(url);
			var directions = await directionsTask;

			return directions;

		}

	}
}