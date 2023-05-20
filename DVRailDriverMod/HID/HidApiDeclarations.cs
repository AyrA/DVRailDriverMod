using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace DVRailDriverMod.HID
{
    internal sealed class HidApiDeclarations
    {
        public struct HIDD_ATTRIBUTES
        {
            public int Size;

            public short VendorID;

            public short ProductID;

            public short VersionNumber;
        }

        public struct HIDP_CAPS
        {
            public short Usage;

            public short UsagePage;

            public short InputReportByteLength;

            public short OutputReportByteLength;

            public short FeatureReportByteLength;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public short[] Reserved;

            public short NumberLinkCollectionNodes;

            public short NumberInputButtonCaps;

            public short NumberInputValueCaps;

            public short NumberInputDataIndices;

            public short NumberOutputButtonCaps;

            public short NumberOutputValueCaps;

            public short NumberOutputDataIndices;

            public short NumberFeatureButtonCaps;

            public short NumberFeatureValueCaps;

            public short NumberFeatureDataIndices;
        }

        [DllImport("hid.dll")]
        public static extern int HidD_GetAttributes(SafeFileHandle HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);
        [DllImport("hid.dll")]
        public static extern void HidD_GetHidGuid(ref Guid HidGuid);

        [DllImport("hid.dll")]
        public static extern bool HidD_GetPreparsedData(SafeFileHandle HidDeviceObject, ref IntPtr PreparsedData);

        [DllImport("hid.dll")]
        public static extern int HidP_GetCaps(IntPtr PreparsedData, ref HIDP_CAPS Capabilities);
    }
}
