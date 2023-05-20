using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace DVRailDriverMod.HID
{
    internal sealed class HidPieDevice : IDisposable
    {
        private SafeFileHandle hRead;
        private SafeFileHandle hWrite;

        private byte[] lastInput;

        public readonly PieDeviceInformation DeviceInfo;

        public bool SuppressIdenticalInputs { get; set; }

        public bool IsOpen =>
            hRead != null && hWrite != null &&
            !hRead.IsInvalid && !hWrite.IsInvalid &&
            !hRead.IsClosed && !hWrite.IsClosed;

        public HidPieDevice(PieDeviceInformation info)
        {
            DeviceInfo = info ?? throw new ArgumentNullException(nameof(info));
            lastInput = new byte[info.ReadSize];
        }

        public void WriteData(byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (!IsOpen)
            {
                throw new InvalidOperationException("Device not open");
            }
            int written = 0;
            var dataPtr = Marshal.AllocHGlobal(DeviceInfo.WriteSize);
            try
            {
                Marshal.Copy(data, 0, dataPtr, data.Length);

                var ret = FileIOApiDeclarations.WriteFile(hWrite, dataPtr, DeviceInfo.WriteSize,
                    ref written, IntPtr.Zero);
                Debug.Print("Wrote {0} bytes of {2}. Result was {1}", written, ret, DeviceInfo.WriteSize);
                if (ret == 0)
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(dataPtr);
            }
        }

        public byte[] ReadData(CancellationToken ct)
        {
            if (!IsOpen)
            {
                throw new InvalidOperationException("Device not open");
            }
            byte[] dest = new byte[DeviceInfo.ReadSize];
            int read = 0;
            var buffer = Marshal.AllocHGlobal(DeviceInfo.ReadSize);
            try
            {
                do
                {
                    var ret = FileIOApiDeclarations.ReadFile(hRead, buffer, DeviceInfo.ReadSize, ref read, IntPtr.Zero);
                    Logging.LogDebug("Read {0} bytes of {2}. Result was {1}", read, ret, DeviceInfo.ReadSize);
                    if (ret == 0)
                    {
                        throw new Win32Exception();
                    }
                    Marshal.Copy(buffer, dest, 0, DeviceInfo.ReadSize);
                } while (!ct.IsCancellationRequested && SuppressIdenticalInputs && lastInput.SequenceEqual(dest));
                lastInput = (byte[])dest.Clone();
                return dest;
            }
            catch (ThreadAbortException)
            {
                //NOOP
                return (byte[])lastInput.Clone();
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public void Open()
        {
            if (hRead != null || hWrite != null)
            {
                throw new InvalidOperationException("Device already open");
            }
            hRead = FileIOApiDeclarations.CreateFile(DeviceInfo.Path,
                FileIOApiDeclarations.DesiredAccess.GENERIC_READ,
                FileIOApiDeclarations.FileShare.FILE_SHARE_READWRITE,
                IntPtr.Zero,
                FileIOApiDeclarations.CreationDisposition.OPEN_EXISTING,
                0,
                0);
            if (hRead.IsInvalid)
            {
                Close();
                throw new Win32Exception();
            }
            hWrite = FileIOApiDeclarations.CreateFile(DeviceInfo.Path,
                FileIOApiDeclarations.DesiredAccess.GENERIC_WRITE,
                FileIOApiDeclarations.FileShare.FILE_SHARE_READWRITE,
                IntPtr.Zero,
                FileIOApiDeclarations.CreationDisposition.OPEN_EXISTING,
                0,
                0);
            if (hWrite.IsInvalid)
            {
                Close();
                throw new Win32Exception();
            }
        }

        public void Close()
        {
            if (hRead != null)
            {
                using (hRead)
                {
                    hRead.Close();
                }
            }
            if (hWrite != null)
            {
                using (hWrite)
                {
                    hWrite.Close();
                }
            }
            hRead = hWrite = null;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
