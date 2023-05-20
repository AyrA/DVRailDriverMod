using System;
using System.Runtime.InteropServices;

namespace DVRailDriverMod.HID
{
    internal sealed class DeviceManagementApiDeclarations
    {
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;

            public Guid InterfaceClassGuid;

            public int Flags;

            public IntPtr Reserved;
        }

        public const int DBT_DEVICEARRIVAL = 32768;

        public const int DBT_DEVICEREMOVECOMPLETE = 32772;

        public const int DBT_DEVTYP_DEVICEINTERFACE = 5;

        public const int DBT_DEVTYP_HANDLE = 6;

        public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;

        public const int DEVICE_NOTIFY_SERVICE_HANDLE = 1;

        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;

        public const int WM_DEVICECHANGE = 537;

        public const short DIGCF_PRESENT = 2;

        public const short DIGCF_DEVICEINTERFACE = 16;

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern int SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern int SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, int DeviceInfoData, ref Guid InterfaceClassGuid, int MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, string Enumerator, IntPtr hwndParent, int Flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, ref int RequiredSize, IntPtr DeviceInfoData);
    }
}
