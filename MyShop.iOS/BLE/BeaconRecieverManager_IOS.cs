using System;
using CoreLocation;
using Foundation;

using BleIosExample.Helper;

namespace BleIosExample.BLE
{
	//public delegate void BeaconBroadcast(object source, CLBeacon beacon, BeaconEventType eventType);
	//implementeventtype

	public class BeaconRecieverManager_IOS : IBeaconReciever
	{


		public event EventHandler BeaconBroadcastEvent;

		private const string Uuid = "401EE3FD-6F9F-4500-8F47-A99D25C66412";
        private const string virtualUuid = "11111111-1111-1111-1111-111111111111";

		private string BeaconId = "BleTags";
		private CLLocationManager _locationManager;
		private bool _serviceStarted;
		private readonly CLBeaconRegion _beaconRegion;

        private readonly CLBeaconRegion _virtualBeaconRegion;

		public BeaconRecieverManager_IOS()
		{
			_serviceStarted = false;
			var beaconUuid = new NSUuid(Uuid);

			_beaconRegion = new CLBeaconRegion(beaconUuid, BeaconId)
			{
				NotifyEntryStateOnDisplay = true,
				NotifyOnEntry = true,
				NotifyOnExit = true
			};

            _virtualBeaconRegion = new CLBeaconRegion(new NSUuid(virtualUuid), "virtualBleTags")
            {
                NotifyEntryStateOnDisplay = true,
                NotifyOnEntry = true,
                NotifyOnExit = true
            };

		}

		public bool IsServiceStarted()
		{
			return _serviceStarted;
		}

		public void Start()
		{
			if (_serviceStarted)
			{
				return;
			}

			_locationManager = new CLLocationManager();

			_locationManager.RequestWhenInUseAuthorization();
			_locationManager.DidStartMonitoringForRegion += OnLocationManagerOnDidStartMonitoringForRegion;
			_locationManager.DidDetermineState += OnLocationManagerOnDidDetermineState;
			_locationManager.DidRangeBeacons += OnLocationManagerOnDidRangeBeacons;

			_locationManager.StartMonitoring(_beaconRegion);
			_locationManager.StartRangingBeacons(_beaconRegion);
            _locationManager.StartMonitoring(_virtualBeaconRegion);
            _locationManager.StartRangingBeacons(_virtualBeaconRegion);
			_serviceStarted = true;
		}




		public void Stop()
		{
			if (!_serviceStarted)
			{
				return;
			}

			_locationManager.DidStartMonitoringForRegion -= OnLocationManagerOnDidStartMonitoringForRegion;
			_locationManager.RegionEntered -= OnLocationManagerOnRegionEntered;
			_locationManager.DidDetermineState -= OnLocationManagerOnDidDetermineState;
			_locationManager.DidRangeBeacons -= OnLocationManagerOnDidRangeBeacons;

			_locationManager.StopMonitoring(_beaconRegion);
			_locationManager.StopRangingBeacons(_beaconRegion);
            _locationManager.StopMonitoring(_virtualBeaconRegion);
            _locationManager.StopRangingBeacons(_virtualBeaconRegion);

			_locationManager = null;


			_serviceStarted = false;
		}



		private void OnLocationManagerOnDidStartMonitoringForRegion(object sender, CLRegionEventArgs e)
		{
			_locationManager.RequestState(e.Region);
		}

		private void OnLocationManagerOnRegionEntered(object sender, CLRegionEventArgs e)
		{

			if (e.Region.Identifier == BeaconId)
			{
				Console.WriteLine("beacon region entered");
			}
		}

		private void LocationManagerOnRegionLeft(object sender, CLRegionEventArgs clRegionEventArgs)
		{
			throw new NotImplementedException();
		}

		private void OnLocationManagerOnDidDetermineState(object sender, CLRegionStateDeterminedEventArgs e)
		{
			switch (e.State)
			{
			case CLRegionState.Inside:
				Console.WriteLine("region state inside");
				break;
			case CLRegionState.Outside:
				Console.WriteLine("region state outside");
				break;
			case CLRegionState.Unknown:
			default:
				Console.WriteLine("region state unknown");
				break;
			}
		}

		private void OnLocationManagerOnDidRangeBeacons(object sender, CLRegionBeaconsRangedEventArgs e)
		{
			if (e.Beacons.Length > 0)
			{
				foreach (var beacon in e.Beacons)
				{
					if (beacon.Rssi != 0)
					{

						if (BeaconBroadcastEvent != null)
						{

							var gBeacon = GBeaconHelper.ToGBeacon(beacon);
							BeaconBroadcastEvent(this, new GBeaconEventArgs(gBeacon, BeaconEventType.Broadcast));
						}
					}
				}
			}
		}
	}
}
