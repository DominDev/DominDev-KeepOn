using System.Runtime.InteropServices;

namespace KeepOn;

internal sealed class PowerRequestService : IDisposable
{
    private const string RequestReason = "KeepOn active user mode";

    private SafePowerRequestHandle? _handle;
    private bool _systemRequestActive;
    private bool _displayRequestActive;
    private bool _disposed;

    public AwakeMode CurrentMode
    {
        get
        {
            if (_systemRequestActive && _displayRequestActive)
            {
                return AwakeMode.SystemAndDisplayRequired;
            }

            if (_systemRequestActive)
            {
                return AwakeMode.SystemRequired;
            }

            return AwakeMode.Disabled;
        }
    }

    public void SetMode(AwakeMode mode)
    {
        ThrowIfDisposed();

        switch (mode)
        {
            case AwakeMode.Disabled:
                Disable();
                break;
            case AwakeMode.SystemRequired:
                SetSystemRequired();
                ClearDisplayRequired();
                break;
            case AwakeMode.SystemAndDisplayRequired:
                SetSystemRequired();
                SetDisplayRequired();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    public void Disable()
    {
        ThrowIfDisposed();
        ClearDisplayRequired();
        ClearSystemRequired();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_displayRequestActive)
        {
            TryClearRequest(PowerRequestType.DisplayRequired);
            _displayRequestActive = false;
        }

        if (_systemRequestActive)
        {
            TryClearRequest(PowerRequestType.SystemRequired);
            _systemRequestActive = false;
        }

        _handle?.Dispose();
        _handle = null;
        _disposed = true;
    }

    private void SetSystemRequired()
    {
        if (_systemRequestActive)
        {
            return;
        }

        SetRequest(PowerRequestType.SystemRequired);
        _systemRequestActive = true;
    }

    private void SetDisplayRequired()
    {
        if (_displayRequestActive)
        {
            return;
        }

        SetRequest(PowerRequestType.DisplayRequired);
        _displayRequestActive = true;
    }

    private void ClearSystemRequired()
    {
        if (!_systemRequestActive)
        {
            return;
        }

        ClearRequest(PowerRequestType.SystemRequired);
        _systemRequestActive = false;
    }

    private void ClearDisplayRequired()
    {
        if (!_displayRequestActive)
        {
            return;
        }

        ClearRequest(PowerRequestType.DisplayRequired);
        _displayRequestActive = false;
    }

    private void SetRequest(PowerRequestType requestType)
    {
        var handle = EnsureHandle();
        if (!NativePowerApi.PowerSetRequest(handle, requestType))
        {
            throw CreatePowerRequestException("PowerSetRequest");
        }
    }

    private void ClearRequest(PowerRequestType requestType)
    {
        if (_handle is null || _handle.IsInvalid)
        {
            return;
        }

        if (!NativePowerApi.PowerClearRequest(_handle, requestType))
        {
            throw CreatePowerRequestException("PowerClearRequest");
        }
    }

    private void TryClearRequest(PowerRequestType requestType)
    {
        if (_handle is null || _handle.IsInvalid)
        {
            return;
        }

        _ = NativePowerApi.PowerClearRequest(_handle, requestType);
    }

    private SafePowerRequestHandle EnsureHandle()
    {
        if (_handle is { IsInvalid: false, IsClosed: false })
        {
            return _handle;
        }

        var reasonString = Marshal.StringToHGlobalUni(RequestReason);
        try
        {
            var context = new PowerRequestContext
            {
                Version = NativePowerApi.PowerRequestContextVersion,
                Flags = NativePowerApi.PowerRequestContextSimpleString,
                SimpleReasonString = reasonString
            };

            var rawHandle = NativePowerApi.PowerCreateRequest(ref context);
            _handle = new SafePowerRequestHandle(rawHandle);
            if (_handle.IsInvalid)
            {
                throw CreatePowerRequestException("PowerCreateRequest");
            }

            return _handle;
        }
        finally
        {
            Marshal.FreeHGlobal(reasonString);
        }
    }

    private static PowerRequestException CreatePowerRequestException(string operation)
    {
        return new PowerRequestException(operation, Marshal.GetLastPInvokeError());
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
