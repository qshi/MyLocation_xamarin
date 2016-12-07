using UIKit;
using Foundation;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MyShop.iOS
{
	class ResultsTableSource : UITableViewSource
	{
		public LocationPredictions predictions { get; set; }
		const string cellIdentifier = "TableCell";
		public event PlaceSelected RowItemSelected;
		public string apiKey { get; set; }

		public ResultsTableSource()
		{
			predictions = new LocationPredictions();
		}

		public ResultsTableSource(LocationPredictions predictions)
		{
			this.predictions = predictions;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			if (predictions.Predictions != null)
				return predictions.Predictions.Count;

			return 0;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);

			if (cell == null)
				cell = new UITableViewCell(UITableViewCellStyle.Default, cellIdentifier);

			cell.TextLabel.Text = predictions.Predictions[indexPath.Row].Description;

			return cell;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			var selectedPrediction = predictions.Predictions[indexPath.Row].Place_ID;
			ReturnPlaceDetails(selectedPrediction);
		}

		async void ReturnPlaceDetails(string selectionID)
		{
			try
			{
				WebRequest request = WebRequest.Create(CreateDetailsRequestUri(selectionID));
				request.Method = "GET";
				request.ContentType = "application/json";
				WebResponse response = await request.GetResponseAsync();
				string responseStream = string.Empty;
				using (StreamReader sr = new StreamReader(response.GetResponseStream()))
				{
					responseStream = sr.ReadToEnd();
				}
				response.Close();

				JObject jObject = JObject.Parse(responseStream);
				if (jObject != null && RowItemSelected != null)
					RowItemSelected(this, jObject);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		string CreateDetailsRequestUri(string place_id)
		{
			var url = "https://maps.googleapis.com/maps/api/place/details/json";
			return $"{url}?placeid={Uri.EscapeUriString(place_id)}&key={apiKey}";
		}
	}
}