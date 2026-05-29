# KeepOn

[![Release](https://img.shields.io/badge/release-v1.6.2-22C55E.svg)](dist/v1.6.2)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Windows](https://img.shields.io/badge/platform-Windows-2563EB.svg)](#runtime)
[![Code signing](https://img.shields.io/badge/code%20signing-policy-64748B.svg)](CODE_SIGNING_POLICY.md)
[![Support](https://img.shields.io/badge/support-buycoffee.to-F59E0B.svg)](https://buycoffee.to/domindev)

KeepOn is a small Windows tray application for temporarily keeping your PC awake.
It uses the native Windows Power Request API, so it does not simulate mouse
movement, press keys, run hidden browser tabs, or use any workaround activity.

The app is designed for simple desktop use: start it, choose a mode from the
tray icon or control panel, and switch it off when you no longer need it.

![KeepOn dashboard](docs/screenshot-dashboard.png)

## Highlights

- Runs quietly in the Windows notification area.
- Can keep only the system awake, or keep both the system and display awake.
- Can disable active power requests automatically when the Windows session is locked.
- Supports Start with Windows for the current user.
- Includes a modern control panel with Dashboard, Guide and About views.
- Stores settings locally and does not require an account or network access.
- Built for Windows with .NET 10 and Windows Forms.

## Support the Project

If KeepOn is useful to you, you can support development here:

[buycoffee.to/domindev](https://buycoffee.to/domindev)

## Modes

| Mode | What it does | Typical use |
| --- | --- | --- |
| Disabled | KeepOn does not hold any power request. Windows follows its normal sleep and display settings. | Daily default state. |
| System | Prevents system sleep. The display may still turn off according to Windows settings. | Long downloads, background jobs, remote sessions, local scripts. |
| System + display | Prevents system sleep and keeps the display awake. | Monitoring dashboards, presentations, visible progress windows. |

## Settings

| Setting | Description |
| --- | --- |
| Start with Windows | Adds KeepOn to the current user's Windows startup entry. |
| Disable on session lock | Clears active power requests when the Windows session is locked. |

The session-lock option is enabled by default in the current app flow because it is
a safer behavior for normal workstation use.

## Download Variants

Ready-to-download ZIP files are included in this repository under
[`dist/v1.6.2`](dist/v1.6.2).

| Download | .NET required on target PC | Notes |
| --- | --- | --- |
| [KeepOn-framework-dependent-win-x64.zip](dist/v1.6.2/KeepOn-framework-dependent-win-x64.zip) | Yes, .NET 10 Desktop Runtime | Recommended for managed/corporate environments. Not self-contained and not single-file. |
| [KeepOn-portable-self-contained-win-x64.zip](dist/v1.6.2/KeepOn-portable-self-contained-win-x64.zip) | No | Includes the .NET runtime. Larger after extraction, but still not single-file packed. |
| [SHA256SUMS.txt](dist/v1.6.2/SHA256SUMS.txt) | No | SHA-256 checksums for validating downloaded ZIP files. |

Both current ZIP variants contain unpacked application files rather than packed
single-file executables. This is intentional: it gives EDR tools a simpler file
profile to inspect. The old compressed single-file variant was removed.

The same variants can also be generated locally:

| Variant | Local publish output |
| --- | --- |
| Portable self-contained | `artifacts\publish\portable-self-contained\` |
| Framework-dependent | `artifacts\publish\framework-dependent\` |

## Code Signing Policy

KeepOn is applying for free open source code signing through SignPath Foundation.
See [CODE_SIGNING_POLICY.md](CODE_SIGNING_POLICY.md).

Planned signed releases will use:

```text
Free code signing provided by SignPath.io, certificate by SignPath Foundation.
```

Until signing is fully configured, release artifacts may be unsigned. For
security-sensitive environments, prefer the framework-dependent package and
validate it together with [SECURITY.md](SECURITY.md), [PRIVACY.md](PRIVACY.md)
and `SHA256SUMS.txt`.

## Installation

KeepOn does not require a traditional installer.

1. Choose one of the published ZIP packages.
2. Extract the whole ZIP to your preferred application folder, for example:

   ```text
   D:\Program Files\KeepOn\
   ```

3. Run `KeepOn.exe` from the extracted folder.
4. Open the tray icon or control panel.
5. Enable `Start with Windows` if you want KeepOn to start after sign-in.

Keep the extracted files together. Do not move only `KeepOn.exe` out of the
folder, especially when using the framework-dependent or self-contained ZIP
packages.

If you move KeepOn to another folder and use autostart, open KeepOn once from
the new location and toggle `Start with Windows` off and on again. This updates
the Windows startup entry to the new path.

## Usage

After launching KeepOn, use the tray icon or the control panel:

- `Dashboard` shows the current mode and quick settings.
- `Guide` explains modes and actions inside the app.
- `About` shows version and project information.
- `Website` opens [domindev.com](https://domindev.com).
- `Support` opens [buycoffee.to/domindev](https://buycoffee.to/domindev).
- `Exit` closes KeepOn and clears active power requests.

When KeepOn exits, it releases any active power request before the process closes.

## Privacy

KeepOn is local-only by design. See [PRIVACY.md](PRIVACY.md).

- It does not collect telemetry.
- It does not require login.
- It does not send settings anywhere.
- It does not monitor keyboard, mouse, applications, windows, browser activity, or files.
- It only uses Windows APIs needed for tray UI, startup registration, session lock detection and power requests.

## Local Data

KeepOn stores settings and logs under the current Windows user profile:

```text
%LOCALAPPDATA%\KeepOn\
%LOCALAPPDATA%\KeepOn\settings.json
%LOCALAPPDATA%\KeepOn\Logs\
```

For compatibility with early builds, KeepOn can migrate settings or startup data
from the previous internal name `DominTray`. New data is stored under `KeepOn`.

## Verify Active Power Requests

Windows can show active power requests with:

```powershell
powercfg /requests
```

Expected behavior:

- In `Disabled`, there should be no active `KeepOn.exe` entries.
- In `System`, `KeepOn.exe` should appear under `SYSTEM`.
- In `System + display`, `KeepOn.exe` should appear under `SYSTEM` and `DISPLAY`.

## Build From Source

Requirements:

- Windows 11 x64
- .NET 10 SDK

Build:

```powershell
dotnet build .\KeepOn.slnx -c Release
```

Publish all variants:

```powershell
.\publish-all.ps1
```

Publish outputs:

```text
artifacts\publish\portable-self-contained\KeepOn.exe
artifacts\publish\framework-dependent\KeepOn.exe
artifacts\publish\SHA256SUMS.txt
```

For security-sensitive environments, start with the framework-dependent variant.
See [SECURITY.md](SECURITY.md) for validation notes.

## Release Automation

This repository includes a GitHub Actions workflow that builds all release
variants and attaches ZIP files to a GitHub Release.

Code signing integration is planned through SignPath Foundation. Preparation
notes are kept in [docs/signpath-application.md](docs/signpath-application.md).

Create a release by pushing a version tag:

```powershell
git tag v1.6.2
git push origin v1.6.2
```

You can also run the `Release` workflow manually from GitHub Actions and provide
the release tag.

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

## Project Metadata

| Field | Value |
| --- | --- |
| Product name | KeepOn |
| File description | KeepOn |
| Company | DominDev |
| Website | https://domindev.com |
| Version | 1.6.2 |
| File version | 1.6.2.0 |
| Runtime | .NET 10 |
| UI | Windows Forms |

## Troubleshooting

### KeepOn does not start with Windows

Open KeepOn manually and toggle `Start with Windows` off and on again. The app
stores startup configuration in:

```text
HKCU\Software\Microsoft\Windows\CurrentVersion\Run
```

The entry should point to the current `KeepOn.exe` path.

### Windows still sleeps

Check that the selected mode is not `Disabled`, then run:

```powershell
powercfg /requests
```

If there is no `KeepOn.exe` entry, restart KeepOn and select the mode again.

### The app was moved to another folder

If autostart was enabled before moving the executable, toggle `Start with Windows`
off and on again from the new location.

### Antivirus or EDR warning

Unsigned or newly signed KeepOn builds may be treated with extra caution by
Windows or endpoint security tools until they build reputation. Code signing can
reduce these warnings, but the app itself does not perform stealthy behavior,
network communication, injection, encryption, persistence outside the current
user's Run key, or input simulation.

For endpoint security validation, review [SECURITY.md](SECURITY.md),
[PRIVACY.md](PRIVACY.md) and [CODE_SIGNING_POLICY.md](CODE_SIGNING_POLICY.md).

## License

KeepOn is released under the MIT License. See [LICENSE](LICENSE).
