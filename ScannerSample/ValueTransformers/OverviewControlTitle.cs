using System;
using Foundation;
using ObjCRuntime;

namespace ScannerSample {
	[Register ("OverviewControlTitle")]
	public class OverviewControlTitle : NSValueTransformer {
		[Export ("transformedValueClass")]
		public static new Class TransformedValueClass => new Class (typeof (NSString));

		[Export ("allowsReverseTransformation")]
		public static new bool AllowsReverseTransformation => false;

		public override NSObject TransformedValue (NSObject value)
		{
			if (value != null && value is NSNumber number && number.Int32Value != 0)
				return (NSString) "Cancel";
			return (NSString) "Overview";
		}
	}
}
