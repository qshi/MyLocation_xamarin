﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BleIosExample.Models
{
	public class GBeacon
	{
		public long Minor { get; set; }
		public long Major { get; set; }
		public int Rssi { get; set; }
		public string Uuid { get; set; }

		public string FullId
		{
			get { return this.Major + "-" + this.Minor; }
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
