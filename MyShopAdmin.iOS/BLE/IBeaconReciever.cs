using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BleIosExample.Models;



namespace BleIosExample.BLE
{
	public delegate void BeaconBroadcast(object source, GBeacon beacon, BeaconEventType eventType);


	public interface IBeaconReciever
	{
		void Start();
		void Stop();
		bool IsServiceStarted();
		event EventHandler BeaconBroadcastEvent;
	}

	public enum BeaconEventType : int
	{
		Broadcast = 1,
		RegionEntered = 2,
		RegionLeft = 3
	}

	public class GBeaconEventArgs : EventArgs
	{
		public GBeaconEventArgs(GBeacon beacon, BeaconEventType eventType)
		{
			Beacon = beacon;
			EventType = eventType;
		}

		public GBeacon Beacon { get; set; }
		public BeaconEventType EventType { get; set; }
	}
}
