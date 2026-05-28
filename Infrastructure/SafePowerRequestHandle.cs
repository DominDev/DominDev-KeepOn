using Microsoft.Win32.SafeHandles;

namespace KeepOn;

internal sealed class SafePowerRequestHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafePowerRequestHandle(IntPtr handle)
        : base(true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        return NativePowerApi.CloseHandle(handle);
    }
}
