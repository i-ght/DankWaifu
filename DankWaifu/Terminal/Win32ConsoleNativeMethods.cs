using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DankWaifu.Terminal
{
    public delegate bool ConsoleEventHandler(CtrlType sig);

    public static class Win32ConsoleNativeMethods
    {
        [DllImport("Kernel32.dll", EntryPoint = "WriteConsoleOutputW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool WriteConsoleOutput(IntPtr stdout, CharInfo[] lpBuffer, Coord bufferSize, Coord bufferCords, ref SmallRect rect);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool WriteConsoleOutputCharacter(
            IntPtr hConsoleOutput,
            StringBuilder lpCharacter,
            uint nLength,
            Coord dwWriteCoord,
            out uint lpNumberOfCharsWritten);

        [DllImport("Kernel32.dll")]
        internal static extern bool SetConsoleCtrlHandler(ConsoleEventHandler handler, bool add);
    }
}