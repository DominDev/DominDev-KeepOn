using Microsoft.Win32;

namespace KeepOn;

internal sealed class SessionStateService : IDisposable
{
    private readonly Action _sessionLocked;
    private bool _disposed;

    public SessionStateService(Action sessionLocked)
    {
        _sessionLocked = sessionLocked;
        SystemEvents.SessionSwitch += OnSessionSwitch;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        SystemEvents.SessionSwitch -= OnSessionSwitch;
        _disposed = true;
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason == SessionSwitchReason.SessionLock)
        {
            _sessionLocked();
        }
    }
}
