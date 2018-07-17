// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace ScannerSample
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSPopUpButton FunctionalUnitMenu { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator ProgressIndicator { get; set; }

		[Outlet]
		AppKit.NSArrayController ScannersController { get; set; }

		[Outlet]
		AppKit.NSTableView ScannersTableView { get; set; }

		[Outlet]
		AppKit.NSTextField StatusText { get; set; }

		[Action ("OpenCloseSession:")]
		partial void OpenCloseSession (AppKit.NSButton sender);

		[Action ("StartOverviewScan:")]
		partial void StartOverviewScan (AppKit.NSButton sender);

		[Action ("StartScan:")]
		partial void StartScan (AppKit.NSButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (StatusText != null) {
				StatusText.Dispose ();
				StatusText = null;
			}

			if (ProgressIndicator != null) {
				ProgressIndicator.Dispose ();
				ProgressIndicator = null;
			}

			if (FunctionalUnitMenu != null) {
				FunctionalUnitMenu.Dispose ();
				FunctionalUnitMenu = null;
			}

			if (ScannersController != null) {
				ScannersController.Dispose ();
				ScannersController = null;
			}

			if (ScannersTableView != null) {
				ScannersTableView.Dispose ();
				ScannersTableView = null;
			}
		}
	}
}
