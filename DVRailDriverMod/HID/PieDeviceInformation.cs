using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace DVRailDriverMod.HID
{
    internal sealed class PieDeviceInformation
    {
        public string Path { get; private set; }
        public int WriteSize { get; private set; }
        public int ReadSize { get; private set; }

        public PieDeviceInformation(string path)
        {
            Path = path;
            var deviceHandle = FileIOApiDeclarations.CreateFile(path,
                    FileIOApiDeclarations.DesiredAccess.GENERIC_WRITE,
                    FileIOApiDeclarations.FileShare.FILE_SHARE_READWRITE,
                    IntPtr.Zero,
                    FileIOApiDeclarations.CreationDisposition.OPEN_EXISTING, 0u, 0);
            if (deviceHandle.IsInvalid)
            {
                //Unable to open device
                throw new Win32Exception();
            }
            using (deviceHandle)
            {
                HidApiDeclarations.HIDD_ATTRIBUTES Attributes = default;
                Attributes.Size = Marshal.SizeOf(Attributes);
                if (HidApiDeclarations.HidD_GetAttributes(deviceHandle, ref Attributes) == 0 || Attributes.VendorID != HidPieDeviceFinder.VendorId)
                {
                    throw new ArgumentException("Supplied device is not a PIE device");
                }
                IntPtr PreparsedData = default;
                if (!HidApiDeclarations.HidD_GetPreparsedData(deviceHandle, ref PreparsedData))
                {
                    throw new IOException($"HidD_GetPreparsedData() failed for: {path}");
                }
                HidApiDeclarations.HIDP_CAPS Capabilities = default;
                if (HidApiDeclarations.HidP_GetCaps(PreparsedData, ref Capabilities) == 0)
                {
                    throw new IOException($"HidP_GetCaps() failed for: {path}");
                }
                WriteSize = Capabilities.OutputReportByteLength;
                ReadSize = Capabilities.InputReportByteLength;
            }
        }
    }
}
