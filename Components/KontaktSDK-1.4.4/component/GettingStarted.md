#Getting started with Kontakt.io iOS SDK for Xamarin

This document will briefly describe how you can use our SDK on Xamarin Platform.

## Configuration

As a first step, you should set your API key in `AppDelegate`'s `FinishedLaunching` method:

```csharp
Kontakt.APIKey = "YOUR-API-KEY";
```

## Beacon Manager

`KTKBeaconManager` is the main class responsible for handling iBeacon monitoring and ranging. 

### Permissions

iBeacon support on iOS is a part of Location Services. Kontakt.io iOS SDK, when it comes to monitoring and ranging, is built on top of Core Location. Because of this, before you start monitoring you need to make sure your app has the necessary permission to use Location Services and ask for them if needed.

Due to that, you must specify `NSLocationAlwaysUsageDescription` or `NSLocationWhenInUseUsageDescription` in you `info.plist` file with a description that will be prompted to your users. Then while using BeaconManager, you should call either `RequestLocationAlwaysAuthorization` or `RequestLocationWhenInUseAuthorization` method first.

### Example

The example below presents beacons monitoring & ranging functionality.

```csharp
KTKBeaconManager beaconManager;
KTKBeaconRegion beaconRegion;

public override void ViewDidLoad()
{
	base.ViewDidLoad();

	// Initialize beacon manager
	beaconManager = new KTKBeaconManager(new BeaconManagerDelegate());
	beaconManager.RequestLocationAlwaysAuthorization();

	// Create beacon region - Kontakt.io proximity UUID by default
	NSUuid proximityUUUID = new NSUuid("f7826da6-4fa2-4e98-8024-bc5b71e0893e");
	beaconRegion = new KTKBeaconRegion(proximityUUUID, "region-identifier");
}
		
...

class BeaconManagerDelegate : KTKBeaconManagerDelegate
{
	public override void MonitoringDidFailForRegion(KTKBeaconManager manager, KTKBeaconRegion region, Foundation.NSError error)
	{
		Console.WriteLine("Monitoring beacons failed, error = " + error.Description);
	}

	public override void DidStartMonitoringForRegion(KTKBeaconManager manager, KTKBeaconRegion region)
	{
		Console.WriteLine("Beacon monitoring sucessfully started for region: " + region.Description);
	}

	public override void DidEnterRegion(KTKBeaconManager manager, KTKBeaconRegion region)
	{
		Console.WriteLine("Entered region: " + region.Description);
	}

	public override void DidExitRegion(KTKBeaconManager manager, KTKBeaconRegion region)
	{
		Console.WriteLine("Abandoned region: " + region.Description);
	}

	public override void DidRangeBeacons(KTKBeaconManager manager, CoreLocation.CLBeacon[] beacons, KTKBeaconRegion region)
	{
		Console.WriteLine("Ranged " + beacons.Length + " beacons");
	}
}
```

## Eddystone Manager

Since Core Location is not aware of Eddystone beacons, Kontakt.io iOS SDK has a separate manager for detecting Eddystone beacons. In order to start working with Eddystone beacons you need to initialize a `KTKEddystoneManager` instance with a delegate object that conforms to `KTKEddystoneManagerDelegate` protocol.

```csharp
KTKEddystoneManager eddystoneManager;
KTKEddystoneRegion eddystoneRegion;

public override void ViewDidLoad()
{
	base.ViewDidLoad();

	// Initialize eddystone manager
	eddystoneManager = new KTKEddystoneManager(new EddystoneManagerDelegate());

	// Create eddystone region - Kontakt.io region by default
	eddystoneRegion = new KTKEddystoneRegion("f7826da6bc5b71e0893e", null);
	
	// Start eddystones discovery
	eddystoneManager.StartEddystoneDiscoveryInRegion(eddystoneRegion);
}

...

class EddystoneManagerDelegate : KTKEddystoneManagerDelegate
{
	public override void DidFailToStartDiscovery(KTKEddystoneManager manager, NSError error)
	{
		Console.WriteLine("Eddystone discovery failed with error: " + error.Description);
	}

	public override void DidDiscoverEddystones(KTKEddystoneManager manager, NSSet<KTKEddystone> eddystones, KTKEddystoneRegion region)
	{
		Console.WriteLine("Discovered " + eddystones.Count + " eddystones");
	}
}
```

## Nearby Devices Manager

An object in your app responsible for detecting and connecting to nearby Kontakt.io devices needs to be an instance of a `KTKDevicesManager` class. Under the hood `KTKDevicesManager` depends on Core Bluetooth to find Bluetooth devices and establish connection with your beacons when needed.

In order to detect nearby devices, you can use sample code below:

```csharp
KTKDevicesManager devicesManager;

protected ViewController(IntPtr handle) : base(handle) { }

public override void ViewDidLoad()
{
	base.ViewDidLoad();

	// Initialize devices manager
	devicesManager = new KTKDevicesManager(new DevicesManagerDelegate());
	
	// Start nearby devices discovery
	devicesManager.StartDevicesDiscoveryWithInterval(2.0);
}

...

class DevicesManagerDelegate : KTKDevicesManagerDelegate
{
	public override void DidFailToStartDiscovery(KTKDevicesManager manager, Foundation.NSError error)
	{
		Console.WriteLine("Devices discovery failed with error: " + error.Description);
	}

	public override void DidDiscoverDevices(KTKDevicesManager manager, KTKNearbyDevice[] devices)
	{
		Console.WriteLine("Discovered " + devices.Length + " nearby devices");
	}
}
```

## Applying a new configuration

If you want to change settings of your beacons, you need to use a connection to a beacon to write a new configuration, which is an instance of `KTKDeviceConfiguration` class. A new configuration can be made on the go, directly in your own app. It's just a matter of setting up a `KTKDeviceConfiguration` and then providing new values to that object's properties that correspond with beacon settings that you want to adjust. Another approach is to create a configuration through [Web Panel](https://support.kontakt.io/hc/en-gb/articles/201607891) or API. Please check examples from *API Client section* to learn how to get pending configurations from Kontakt.io Cloud. 

Below you can find simple example how to apply new configuration to a device:

```csharp
class DevicesManagerDelegate : KTKDevicesManagerDelegate
{
	public override void DidDiscoverDevices(KTKDevicesManager manager, KTKNearbyDevice[] devices)
	{
		KTKNearbyDevice device = LookForDevice(devices, "abcd");
		if (device != null)
		{
			// Stop discovery and invalidate timer if found device
			vc.devicesManager.StopDevicesDiscovery();

			// Create sample configuration
			KTKDeviceConfiguration configuration = new KTKDeviceConfiguration("abcd");
			configuration.Major = 111;
			configuration.Minor = 222;

			// Connect to device if found
			KTKDeviceConnection connection = new KTKDeviceConnection(device);
			connection.WriteConfiguration(configuration, (synchronized, appliedConfig, error) => 
			{
				// handle response
				if (error == null) 
				{
					Console.WriteLine("Configuration applied");
				}
			});
		}
	}

	KTKNearbyDevice LookForDevice(KTKNearbyDevice[] devices, string uniqueID)
	{
		foreach (KTKNearbyDevice device in devices) 
		{
			if (device.UniqueID == uniqueID)
			{
				return device;
			}
		}
		return null;
	}
}
```

## API Client

The Kontakt.io Cloud API provides a series of resources to query/update our Cloud Services. Every information that you can either access or modify in Kontakt.io Web Panel is also available through API for our backend.

Class responsible for communication with that API is the `KTKCloudClient`. Most data that you can get from or modify through an API call has an equivalent in one of SDK classes. Here is an example:

```csharp
KTKCloudClient cloudClient = new KTKCloudClient();

// Fetch devices from API
cloudClient.GET("device", null, (response, error) => 
{
	// handle response/error
});

// Fetch pending configs from API, parameters required
var parameters = new NSDictionary("deviceType", "beacon");
cloudClient.GET("config", parameters, (response, error) => 
{
	// handle response/error
});

// Fetch managers from API
cloudClient.GET("manager", null, (response, error) => 
{
	// handle response/error
});
```