# Security Notes

KeepOn is a small Windows tray utility that uses supported Windows APIs to hold
temporary power requests. This document is intended for users, administrators and
security teams reviewing the application.

## Expected Behavior

KeepOn can:

- run as a normal user-mode desktop application,
- show a Windows notification-area icon,
- create and clear Windows Power Request API requests,
- listen for Windows session lock events,
- optionally create or remove a current-user startup entry,
- open the project website or support page in the default browser when the user
  clicks those links.

KeepOn does not:

- simulate keyboard or mouse input,
- inject into other processes,
- install services or drivers,
- require administrator privileges,
- encrypt, modify or exfiltrate user files,
- collect telemetry,
- communicate with a backend service,
- download or execute additional payloads,
- create scheduled tasks,
- change system-wide security settings.

## Windows APIs Used

KeepOn uses these notable Windows facilities:

- `PowerCreateRequest`
- `PowerSetRequest`
- `PowerClearRequest`
- `SystemEvents.SessionSwitch`
- `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` for optional per-user
  startup

The startup entry is only written when the user enables `Start with Windows`.

The project enables `AllowUnsafeBlocks` because .NET source-generated
`LibraryImport` interop requires it. KeepOn does not contain hand-written unsafe
memory manipulation code; the flag is required by generated interop wrappers for
the Windows Power Request API calls.

## Distribution Guidance

For security-sensitive environments, prefer the framework-dependent build:

```text
KeepOn-framework-dependent-win-x64.zip
```

This build is not self-contained and is not published as a single-file executable.
It requires the .NET 10 Desktop Runtime on the target machine, but it has the
simplest executable profile.

Avoid the old compressed single-file build in managed environments. It was removed
from current releases because packed single-file binaries can look suspicious to
heuristic EDR engines even when the application behavior is benign.

## Validation Checklist

Recommended validation steps:

1. Download the ZIP from a tagged repository release or from `dist/<version>`.
2. Verify the SHA-256 hash from `SHA256SUMS.txt`.
3. Review the source code for:
   - `Infrastructure\NativePowerApi.cs`
   - `Services\PowerRequestService.cs`
   - `Services\AutostartService.cs`
   - `Services\SessionStateService.cs`
4. Run `powercfg /requests` before and after changing modes.
5. Confirm that no network connection is made unless the user explicitly clicks
   `Website` or `Support`.
6. If organizational policy requires it, distribute a code-signed build from an
   approved internal software channel.

## Code Signing

KeepOn is applying for free open source code signing through SignPath Foundation.
See [CODE_SIGNING_POLICY.md](CODE_SIGNING_POLICY.md).

Until signing is fully configured, release artifacts may be unsigned. For
enterprise deployment, prefer signed tagged releases once available and validate
the Authenticode signature before distribution.

Prefer validation by signed publisher, file hash and documented behavior rather
than broad path-based exclusions.

## Reporting Security Issues

Please report security concerns through the repository issue tracker or contact
DominDev through the project website:

```text
https://domindev.com
```
