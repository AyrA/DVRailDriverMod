using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace DVRailDriverMod.HID
{
    internal sealed class FileIOApiDeclarations
    {
        [Flags]
        public enum DesiredAccess : uint
        {
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000
        }

        [Flags]
        public enum FileShare : uint
        {
            FILE_SHARE_READ = 1,
            FILE_SHARE_WRITE = 2,
            FILE_SHARE_READWRITE = FILE_SHARE_READ | FILE_SHARE_WRITE,
        }

        public enum CreationDisposition : int
        {
            OPEN_EXISTING = 3
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(string lpFileName, DesiredAccess dwDesiredAccess, FileShare dwShareMode, IntPtr lpSecurityAttributes, CreationDisposition dwCreationDisposition, uint dwFlagsAndAttributes, int hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ReadFile(SafeFileHandle hFile, IntPtr lpBuffer, int nNumberOfBytesToRead, ref int lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int WriteFile(SafeFileHandle hFile, IntPtr lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, IntPtr lpOverlapped);
    }
}
