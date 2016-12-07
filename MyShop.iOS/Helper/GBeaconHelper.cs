using System;
using CoreLocation;
using BleIosExample.Models;


namespace BleIosExample.Helper
{
	public class GBeaconHelper
	{
		public static GBeacon ToGBeacon(CLBeacon beacon)
		{
			var gBeacon = new GBeacon
			{
				Major = (long) beacon.Major,
				Minor = (long) beacon.Minor,
				Rssi = (int) beacon.Rssi,
				Timestamp = DateTime.Now,
				Uuid = beacon.ProximityUuid.AsString()

			};
			switch (beacon.Proximity)
			{
			case CLProximity.Far:
				gBeacon.Proximity = GProximity.Far;
				break;
			case CLProximity.Immediate:
				gBeacon.Proximity = GProximity.Immediate;
				break;
			case CLProximity.Near:
				gBeacon.Proximity = GProximity.Near;
				break;
			case CLProximity.Unknown:
				gBeacon.Proximity = GProximity.Unknown;
				break;
			}
			return gBeacon;
		}
	}
}
