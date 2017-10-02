using System;
using CoreBluetooth;
using CoreFoundation;
using CoreLocation;
using Foundation;
using UIKit;
using CoreGraphics;

namespace MyShop.iOS
{
	public partial class ConfigurationViewController : UITableViewController
	{
		bool enabled;
		NSUuid uuid;
		NSNumber major;
		NSNumber minor;
		NSNumber power;

		CBPeripheralManager peripheralManager;
		NSNumberFormatter numberFormatter;
		UIButton doneButton;
		UIButton saveButton;
        UIButton stopButton;

        UILabel enbleLabel;
		UISwitch enabledSwitch;
        UILabel majorLabel;
		UITextField majorTextField;
        UILabel measuredPoweLabel;
		UITextField measuredPowerTextField;
        UILabel minorLabel;
		UITextField minorTextField;
        UILabel uuidTextLabel;
		UITextField uuidTextField;

		public ConfigurationViewController()
		{
			var peripheralDelegate = new PeripheralManagerDelegate();
			peripheralManager = new CBPeripheralManager(peripheralDelegate, DispatchQueue.DefaultGlobalQueue);
			numberFormatter = new NSNumberFormatter()
			{
				NumberStyle = NSNumberFormatterStyle.Decimal
			};
			uuid = Defaults.DefaultProximityUuid;
			power = Defaults.DefaultPower;
		}

		public override void ViewWillAppear(bool animated)
		{
			enabled = enabledSwitch.On = peripheralManager.Advertising;
			base.ViewWillAppear(animated);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			this.Title = "Configure";

			enbleLabel = new UILabel(new CGRect(10, 10, 100, 30));
			enbleLabel.Text = "Enabled";
			enabledSwitch = new UISwitch()
			{
			  Frame = new CGRect(100, 10, 300, 30)
			};
			majorLabel = new UILabel(new CGRect(10, 95, 50, 30));
			majorLabel.Text = "Major";
			majorTextField = new UITextField()
			{
			  Text = "999",
			  Frame = new CGRect(100, 95, 250, 30),
              BorderStyle = UITextBorderStyle.RoundedRect,
              AllowsEditingTextAttributes = false   
			};
    
			measuredPoweLabel = new UILabel(new CGRect(10, 185, 150, 30));
			measuredPoweLabel.Text = "Measured Power";
			measuredPowerTextField = new UITextField()
			{
			  Text = "",
			  Frame = new CGRect(200, 185, 100, 30),
              ReturnKeyType = UIReturnKeyType.Send,
              KeyboardType = UIKeyboardType.DecimalPad,
              BorderStyle = UITextBorderStyle.RoundedRect
			};
			minorLabel = new UILabel(new CGRect(10, 140, 50, 30));
			minorLabel.Text = "Minor";
			minorTextField = new UITextField()
			{
			  Text = "",
			  Frame = new CGRect(100, 140, 250, 30),
              BorderStyle = UITextBorderStyle.RoundedRect
			};
			uuidTextLabel = new UILabel(new CGRect(10, 50, 50, 30));
			uuidTextLabel.Text = "UUID";     
            uuidTextField = new UITextField()
			{
			  Text = "",
			  Frame = new CGRect(60, 50, 300, 30),
              AdjustsFontSizeToFitWidth = true
			};

			TableView.InsertSubview(enbleLabel,1);
			TableView.InsertSubview(majorLabel,3);
            TableView.InsertSubview(measuredPoweLabel,5);
			TableView.InsertSubview(minorLabel,4);
			TableView.InsertSubview(uuidTextLabel,2);

			TableView.InsertSubview(enabledSwitch,1);
			TableView.InsertSubview(majorTextField,3);
			TableView.InsertSubview(measuredPowerTextField,5);
			TableView.InsertSubview(minorTextField,4);
			TableView.InsertSubview(uuidTextField,2);

			enabledSwitch.ValueChanged += (sender, e) => {
				enabled = enabledSwitch.On;
			};

			uuidTextField.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			uuidTextField.InputView = new UuidPickerView(uuidTextField);
			uuidTextField.EditingDidBegin += HandleEditingDidBegin;
			uuidTextField.EditingDidEnd += (sender, e) => {
				uuid = new NSUuid(uuidTextField.Text);
				//NavigationItem.RightBarButtonItem = saveButton;
			};
			uuidTextField.Text = uuid.AsString();

			majorTextField.KeyboardType = UIKeyboardType.NumberPad;
			majorTextField.ReturnKeyType = UIReturnKeyType.Done;
			majorTextField.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			majorTextField.EditingDidBegin += HandleEditingDidBegin;
			majorTextField.EditingDidEnd += (sender, e) => {
				major = numberFormatter.NumberFromString(majorTextField.Text);
				//NavigationItem.RightBarButtonItem = saveButton;
			};

			minorTextField.KeyboardType = UIKeyboardType.NumberPad;
			minorTextField.ReturnKeyType = UIReturnKeyType.Done;
			minorTextField.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			minorTextField.EditingDidBegin += HandleEditingDidBegin;
			minorTextField.EditingDidEnd += (sender, e) => {
				minor = numberFormatter.NumberFromString(minorTextField.Text);
				//NavigationItem.RightBarButtonItem = saveButton;
			};

			measuredPowerTextField.KeyboardType = UIKeyboardType.NumberPad;
			measuredPowerTextField.ReturnKeyType = UIReturnKeyType.Done;
			measuredPowerTextField.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			measuredPowerTextField.EditingDidBegin += HandleEditingDidBegin;
			measuredPowerTextField.EditingDidEnd += (sender, e) => {
				power = numberFormatter.NumberFromString(measuredPowerTextField.Text);
				//NavigationItem.RightBarButtonItem = saveButton;
			};
            measuredPowerTextField.Text = power.ToString();

			stopButton = UIButton.FromType(UIButtonType.RoundedRect);
			stopButton.Frame = new CGRect(160, 320, 140, 40);
			stopButton.SetTitle("Stop", UIControlState.Normal);

			stopButton.TouchUpInside += (sender, ea) =>
			{
                new UIAlertView("Stop to configure your device as a beacon", "", null, "OK", null).Show();
				peripheralManager.StopAdvertising();
			};


			doneButton = UIButton.FromType(UIButtonType.RoundedRect);
            doneButton.Frame = new CGRect(160, 230, 140, 40);
            doneButton.SetTitle("Done", UIControlState.Normal);

            doneButton.TouchUpInside += (sender, ea) =>
            {
                uuidTextField.ResignFirstResponder();
                majorTextField.ResignFirstResponder();
                minorTextField.ResignFirstResponder();
                measuredPowerTextField.ResignFirstResponder();
                TableView.ReloadData();
            };

			saveButton = UIButton.FromType(UIButtonType.RoundedRect);
			saveButton.Frame = new CGRect(160, 275, 140, 40);
			saveButton.SetTitle("Save", UIControlState.Normal);
			saveButton.TouchUpInside += (sender, ea) =>
			{
                new UIAlertView("Start to configure your device as a beacon", "", null, "OK", null).Show();
				if (peripheralManager.State < CBPeripheralManagerState.PoweredOn)
				{
					new UIAlertView("Bluetooth must be enabled", "To configure your device as a beacon", null, "OK", null).Show();
					return;
				}

				if (enabled)
				{
					CLBeaconRegion region = Helpers.CreateRegion(uuid, major, minor);
					if (region != null)
						peripheralManager.StartAdvertising(region.GetPeripheralData(power));
                    View.Add(stopButton);
				}
				else
				{
					peripheralManager.StopAdvertising();
				}

				//NavigationController.PopViewController(true);
			};

            //NavigationItem.RightBarButtonItem = saveButton;
            View.Add(saveButton);
		}

		// identical code share across all UITextField
		void HandleEditingDidBegin(object sender, EventArgs e)
		{
            //NavigationItem.RightBarButtonItem = doneButton;
            View.Add(doneButton);
		}

	}
}

