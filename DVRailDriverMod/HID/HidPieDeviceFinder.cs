using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DVRailDriverMod.HID
{
    internal static class HidPieDeviceFinder
    {
        public const int VendorId = 1523;

        public static PieDeviceInformation[] FindPieDevices()
        {
            var deviceList = new List<string>();
            Guid hidGuid = Guid.Empty;
            HidApiDeclarations.HidD_GetHidGuid(ref hidGuid);
            var deviceInfoSet = DeviceManagementApiDeclarations.SetupDiGetClassDevs(ref hidGuid, null, IntPtr.Zero,
                DeviceManagementApiDeclarations.DIGCF_PRESENT | DeviceManagementApiDeclarations.DIGCF_DEVICEINTERFACE);
            DeviceManagementApiDeclarations.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData = default;
            DeviceInterfaceData.cbSize = Marshal.SizeOf(DeviceInterfaceData);

            //Enumerate all devices and extract the device path for later use
            for (int i = 0; DeviceManagementApiDeclarations.SetupDiEnumDeviceInterfaces(deviceInfoSet, 0, ref hidGuid, i, ref DeviceInterfaceData) != 0; i++)
            {
                int RequiredSize = 0;
                DeviceManagementApiDeclarations.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref DeviceInterfaceData, IntPtr.Zero, 0, ref RequiredSize, IntPtr.Zero);
                IntPtr intPtr = Marshal.AllocHGlobal(RequiredSize);

                if (IntPtr.Size == 8)
                {
                    Marshal.WriteInt32(intPtr, Marshal.SizeOf(typeof(IntPtr)));
                }
                else
                {
                    Marshal.WriteInt32(intPtr, 4 + Marshal.SystemDefaultCharSize);
                }

                if (DeviceManagementApiDeclarations.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref DeviceInterfaceData, intPtr, RequiredSize, ref RequiredSize, IntPtr.Zero))
                {
                    //First 4 bytes do not contain string data
                    deviceList.Add(Marshal.PtrToStringAuto(intPtr + 4));
                }
            }
            //Cleanup
            DeviceManagementApiDeclarations.SetupDiDestroyDeviceInfoList(deviceInfoSet);

            //Filter devices
            return deviceList
                .Where(m => IsPieDevice(m))
                .Select(m => new PieDeviceInformation(m))
                .ToArray();
        }

        private static bool IsPieDevice(string path)
        {
            var deviceHandle = FileIOApiDeclarations.CreateFile(path,
                    FileIOApiDeclarations.DesiredAccess.GENERIC_WRITE,
                    FileIOApiDeclarations.FileShare.FILE_SHARE_READWRITE,
                    IntPtr.Zero,
                    FileIOApiDeclarations.CreationDisposition.OPEN_EXISTING, 0u, 0);
            if (deviceHandle.IsInvalid)
            {
                //Unable to open device. Skip and try the next one
                return false;
            }
            using (deviceHandle)
            {
                HidApiDeclarations.HIDD_ATTRIBUTES Attributes = default;
                Attributes.Size = Marshal.SizeOf(Attributes);
                if (HidApiDeclarations.HidD_GetAttributes(deviceHandle, ref Attributes) == 0 || Attributes.VendorID != VendorId)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
