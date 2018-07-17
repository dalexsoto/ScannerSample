using System;

using AppKit;
using Foundation;
using ImageCaptureCore;
using ObjCRuntime;
using CoreGraphics;
using MobileCoreServices;

namespace ScannerSample {
	public partial class ViewController : NSViewController, IICDeviceBrowserDelegate, IICScannerDeviceDelegate {

		// Workarounds
		static NSString _ICStatusNotificationKey;
		static NSString ICStatusNotificationKey {
			get {
				if (_ICStatusNotificationKey == null) {
					var libHandle = Dlfcn.dlopen (Constants.ImageCaptureCoreLibrary, 0);
					_ICStatusNotificationKey = Dlfcn.GetStringConstant (libHandle, "ICStatusNotificationKey");
				}
				return _ICStatusNotificationKey;
			}
		}

		static NSString _ICScannerStatusWarmingUp;
		static NSString ICScannerStatusWarmingUp {
			get {
				if (_ICScannerStatusWarmingUp == null) {
					var libHandle = Dlfcn.dlopen (Constants.ImageCaptureCoreLibrary, 0);
					_ICScannerStatusWarmingUp = Dlfcn.GetStringConstant (libHandle, "ICScannerStatusWarmingUp");
				}
				return _ICScannerStatusWarmingUp;
			}
		}

		static NSString _ICScannerStatusWarmUpDone;
		static NSString ICScannerStatusWarmUpDone {
			get {
				if (_ICScannerStatusWarmUpDone == null) {
					var libHandle = Dlfcn.dlopen (Constants.ImageCaptureCoreLibrary, 0);
					_ICScannerStatusWarmUpDone = Dlfcn.GetStringConstant (libHandle, "ICScannerStatusWarmUpDone");
				}
				return _ICScannerStatusWarmUpDone;
			}
		}

		static NSString _ICLocalizedStatusNotificationKey;
		static NSString ICLocalizedStatusNotificationKey {
			get {
				if (_ICLocalizedStatusNotificationKey == null) {
					var libHandle = Dlfcn.dlopen (Constants.ImageCaptureCoreLibrary, 0);
					_ICLocalizedStatusNotificationKey = Dlfcn.GetStringConstant (libHandle, "ICLocalizedStatusNotificationKey");
				}
				return _ICLocalizedStatusNotificationKey;
			}
		}

		// End of workaroounds

		public NSMutableArray Scanners { [Export ("Scanners")] get; [Export ("setScanners:")] set; } = new NSMutableArray ();
		public ICDeviceBrowser DeviceBrowser { get; set; } = new ICDeviceBrowser ();
		public ICScannerDevice SelectedScanner {
			get {
				var selectedObjects = ScannersController.SelectedObjects;
				if (selectedObjects == null || selectedObjects.Length == 0)
					return null;
				return selectedObjects [0] as ICScannerDevice;
			}
		}

		static ViewController ()
		{
			var imageTransformer = new CGImageToNSImageTransformer ();
			NSValueTransformer.SetValueTransformer (imageTransformer, "NSImageFromCGImage");
		}

		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewWillAppear ()
		{
			base.ViewWillAppear ();
			View.Window.Title = "Scanner Sample from Xamarin.Mac";
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			ScannersController.SelectsInsertedObjects = false;

			DeviceBrowser.Delegate = this;
			DeviceBrowser.BrowsedDeviceTypeMask = ICDeviceTypeMask.Scanner | (ICDeviceTypeMask) ICDeviceLocationType.Local | (ICDeviceTypeMask) 0x0000FE00; // 0x0000FE00 Remote
			DeviceBrowser.Start ();

			FunctionalUnitMenu.RemoveAllItems ();
			FunctionalUnitMenu.Enabled = false;
		}

		public override NSObject RepresentedObject {
			get {
				return base.RepresentedObject;
			}
			set {
				base.RepresentedObject = value;
				// Update the view, if already loaded.
			}
		}

		// IICDeviceBrowserDelegate Interface

		public void DidAddDevice (ICDeviceBrowser browser, ICDevice device, bool moreComing)
		{
			Console.WriteLine ($"{nameof (DidAddDevice)}: {device}");

			if (device.Type.HasFlag (ICDeviceType.Scanner)) {
				WillChangeValue ("Scanners");
				Scanners.Add (device);
				DidChangeValue ("Scanners");
				device.Delegate = this;
			}
		}

		public void DidRemoveDevice (ICDeviceBrowser browser, ICDevice device, bool moreGoing)
		{
			Console.WriteLine ($"{nameof (DidRemoveDevice)}: {device}");
			ScannersController.RemoveObject (device);
		}

		[Export ("deviceBrowser:deviceDidChangeName:")]
		public void DeviceDidChangeName (ICDeviceBrowser browser, ICDevice device) => Console.WriteLine ($"{nameof (DeviceDidChangeName)}: {device}");

		[Export ("deviceBrowser:deviceDidChangeSharingState:")]
		public void DeviceDidChangeSharingState (ICDeviceBrowser browser, ICDevice device) => Console.WriteLine ($"{nameof (DeviceDidChangeSharingState)}: {device}");

		[Export ("deviceBrowser:requestsSelectDevice:")]
		public void RequestsSelectDevice (ICDeviceBrowser browser, ICDevice device) => Console.WriteLine ($"{nameof (RequestsSelectDevice)}: {device}");

		// IICDeviceBrowser Interface

		public void DidRemoveDevice (ICDevice device)
		{
			Console.WriteLine ($"{nameof (DidRemoveDevice)}: {device}");
			ScannersController.RemoveObject (device);
		}

		[Export ("device:didOpenSessionWithError:")]
		public void DidOpenSession (ICDevice device, NSError error) => Console.WriteLine ($"{nameof (DidOpenSession)}: {device} Error: {error}");

		[Export ("deviceDidBecomeReady:")]
		public void DidBecomeReady (ICDevice device)
		{
			var scanner = device as ICScannerDevice;
			var availabeTypes = scanner.AvailableFunctionalUnitTypes;
			var functionalUnit = scanner.SelectedFunctionalUnit;

			Console.WriteLine ($"{nameof (DidBecomeReady)}: {scanner}");

			FunctionalUnitMenu.RemoveAllItems ();
			FunctionalUnitMenu.Enabled = false;

			if (availabeTypes.Length > 0) {
				var menu = new NSMenu ();
				FunctionalUnitMenu.Enabled = true;

				foreach (var item in availabeTypes) {
					NSMenuItem menuItem;
					switch ((ICScannerFunctionalUnitType) item.Int32Value) {
					case ICScannerFunctionalUnitType.Flatbed:
						menuItem = new NSMenuItem ("Flatbed", "", SelectFunctionalUnit) {
							Tag = (nint) (long) ICScannerFunctionalUnitType.Flatbed
						};
						menu.AddItem (menuItem);
						break;
					case ICScannerFunctionalUnitType.PositiveTransparency:
						menuItem = new NSMenuItem ("PositiveTransparency", "", SelectFunctionalUnit) {
							Tag = (nint) (long) ICScannerFunctionalUnitType.PositiveTransparency
						};
						menu.AddItem (menuItem);
						break;
					case ICScannerFunctionalUnitType.NegativeTransparency:
						menuItem = new NSMenuItem ("NegativeTransparency", "", SelectFunctionalUnit) {
							Tag = (nint) (long) ICScannerFunctionalUnitType.NegativeTransparency
						};
						menu.AddItem (menuItem);
						break;
					case ICScannerFunctionalUnitType.DocumentFeeder:
						menuItem = new NSMenuItem ("DocumentFeeder", "", SelectFunctionalUnit) {
							Tag = (nint) (long) ICScannerFunctionalUnitType.DocumentFeeder
						};
						menu.AddItem (menuItem);
						break;
					}
				}
				FunctionalUnitMenu.Menu = menu;
			}
			Console.WriteLine ($"observeValueForKeyPath - functionalUnit: {functionalUnit}");
			if (functionalUnit != null)
				FunctionalUnitMenu.SelectItemWithTag ((nint) (long) functionalUnit.Type);
		}

		[Export ("device:didCloseSessionWithError:")]
		public void DidCloseSession (ICDevice device, NSError error) => Console.WriteLine ($"{nameof (DidCloseSession)}: {device} Error: {error}");

		[Export ("deviceDidChangeName:")]
		public void DidChangeName (ICDevice device) => Console.WriteLine ($"{nameof (DidChangeName)}: {device}");

		[Export ("deviceDidChangeSharingState:")]
		public void DidChangeSharingState (ICDevice device) => Console.WriteLine ($"{nameof (DidChangeSharingState)}: {device}");

		[Export ("device:didReceiveStatusInformation:")]
		public void DidReceiveStatusInformation (ICDevice device, NSDictionary<NSString, NSObject> status)
		{
			Console.WriteLine ($"{nameof (DidReceiveStatusInformation)}: {device} Status: {status}");

			if ((status[ICStatusNotificationKey] as NSString) == ICScannerStatusWarmingUp) {
				ProgressIndicator.IsDisplayedWhenStopped = true;
				ProgressIndicator.Indeterminate = true;
				ProgressIndicator.StartAnimation (null);
				StatusText.StringValue = status[ICLocalizedStatusNotificationKey] as NSString;
			} else if ((status [ICStatusNotificationKey] as NSString) == ICScannerStatusWarmUpDone) {
				StatusText.StringValue = string.Empty;
				ProgressIndicator.StopAnimation (null);
				ProgressIndicator.Indeterminate = false;
				ProgressIndicator.IsDisplayedWhenStopped = false;
			}
		}

		[Export ("device:didEncounterError:")]
		public void DidEncounterError (ICDevice device, NSError error) => Console.WriteLine ($"{nameof (DidEncounterError)}: {device} Error: {error}");

		[Export ("device:didReceiveButtonPress:")]
		public void DidReceiveButtonPress (ICDevice device, string buttonType) => Console.WriteLine ($"{nameof (DidReceiveButtonPress)}: {device} Button: {buttonType}");

		[Export ("scannerDeviceDidBecomeAvailable:")]
		public void DidBecomeAvailable (ICScannerDevice scanner)
		{
			Console.WriteLine ($"{nameof (DidBecomeAvailable)}: {scanner}");
			scanner.RequestOpenSession ();
		}

		[Export ("scannerDevice:didSelectFunctionalUnit:error:")]
		public void DidSelectFunctionalUnit (ICScannerDevice scanner, ICScannerFunctionalUnit functionalUnit, NSError error) => Console.WriteLine ($"{nameof (DidSelectFunctionalUnit)}: {scanner} FunctionalUnit: {functionalUnit} Error: {error}");

		[Export ("scannerDevice:didScanToURL:data:")]
		public void DidScanToUrl (ICScannerDevice scanner, NSUrl url, NSData data) => Console.WriteLine ($"{nameof (DidScanToUrl)}: {scanner} Url: {url} Data: {data}");

		[Export ("scannerDevice:didCompleteOverviewScanWithError:")]
		public void DidCompleteOverviewScan (ICScannerDevice scanner, NSError error)
		{
			Console.WriteLine ($"{nameof (DidCompleteOverviewScan)}: {scanner} Error: {error}");
			ProgressIndicator.Hidden = true;
		}

		[Export ("scannerDevice:didCompleteScanWithError:")]
		public void DidCompleteScan (ICScannerDevice scanner, NSError error)
		{
			Console.WriteLine ($"{nameof (DidCompleteScan)}: {scanner} Error: {error}");
			ProgressIndicator.Hidden = true;
		}

		// Button Outlets

		partial void OpenCloseSession (NSButton sender)
		{
			if (SelectedScanner.HasOpenSession)
				SelectedScanner.RequestCloseSession ();
			else
				SelectedScanner.RequestOpenSession ();
		}

		void SelectFunctionalUnit (object sender, EventArgs e)
		{
			var item = sender as NSMenuItem;
			var title = item.Title;
			var selectedType = (ICScannerFunctionalUnitType) (uint) item.Tag;
			Console.WriteLine ($"{nameof (SelectFunctionalUnit)}: {title} SelectedType: {selectedType}");

			if (selectedType != SelectedScanner?.SelectedFunctionalUnit?.Type)
				SelectedScanner.RequestSelectFunctionalUnit (selectedType);
		}

		partial void StartOverviewScan (NSButton sender)
		{
			var scanner = SelectedScanner;
			var fu = scanner.SelectedFunctionalUnit;
			if (fu.CanPerformOverviewScan && fu.ScanInProgress == false && fu.OverviewScanInProgress == false) {
				fu.OverviewResolution = fu.SupportedResolutions.IndexGreaterThanOrEqual (72);
				scanner.RequestOverviewScan ();
				ProgressIndicator.Hidden = false;
			} else
				scanner.CancelScan ();
		}

		partial void StartScan (NSButton sender)
		{
			var scanner = SelectedScanner;
			var fu = scanner.SelectedFunctionalUnit;

			if (fu.ScanInProgress == false && fu.OverviewScanInProgress == false) {
				if (fu.Type == ICScannerFunctionalUnitType.DocumentFeeder) {
					var dfu = fu as ICScannerFunctionalUnitDocumentFeeder;
					dfu.DocumentType = ICScannerDocumentType.USLetter;
				} else {
					CGSize s;
					fu.MeasurementUnit = ICScannerMeasurementUnit.Inches;
					if (fu.Type == ICScannerFunctionalUnitType.Flatbed)
						s = (fu as ICScannerFunctionalUnitFlatbed).PhysicalSize;
					else if (fu.Type == ICScannerFunctionalUnitType.PositiveTransparency)
						s = (fu as ICScannerFunctionalUnitPositiveTransparency).PhysicalSize;
					else
						s = (fu as ICScannerFunctionalUnitNegativeTransparency).PhysicalSize;
					fu.ScanArea = new CGRect (0, 0, s.Width, s.Height);
				}
				fu.Resolution = fu.SupportedResolutions.IndexGreaterThanOrEqual (100);
				fu.BitDepth = ICScannerBitDepth.Bits8;
				fu.PixelDataType = ICScannerPixelDataType.Rgb;

				scanner.TransferMode = ICScannerTransferMode.FileBased;
				scanner.DownloadsDirectory = NSUrl.FromFilename (new NSString ("~/Downloads").ExpandTildeInPath ());
				scanner.DocumentName = $"Scan-{Guid.NewGuid ()}";
				scanner.DocumentUti = UTType.JPEG;
				scanner.RequestScan ();
				ProgressIndicator.Hidden = false;
			} else
				scanner.CancelScan ();
		}
	}
}
