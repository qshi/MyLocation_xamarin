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

		public override void DidTapAtCoordinate(MapView mapView, CLLocationCoordinate2D coordinate)
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