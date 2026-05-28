using System.Runtime.InteropServices;

namespace KeepOn;

internal static partial class NativePowerApi
{
    internal const uint PowerRequestContextVersion = 0;
    internal const uint PowerRequestContextSimpleString = 0x1;

    [LibraryImport("kernel32.dll", SetLastError = true)]
    internal static partial IntPtr PowerCreateRequest(ref PowerRequestContext context);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool PowerSetRequest(SafePowerRequestHandle powerRequestHandle, PowerRequestType requestType);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool PowerClearRequest(SafePowerRequestHandle powerRequestHandle, PowerRequestType requestType);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CloseHandle(IntPtr handle);
}

[StructLayout(LayoutKind.Sequential)]
internal struct PowerRequestContext
{
    public uint Version;
    public uint Flags;
    public IntPtr SimpleReasonString;
}

internal enum PowerRequestType
{
    DisplayRequired = 0,
    SystemRequired = 1,
    AwayModeRequired = 2,
    ExecutionRequired = 3
}
