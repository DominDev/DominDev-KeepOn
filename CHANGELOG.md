# Changelog

All notable changes to KeepOn are documented in this file.

## [1.6.0] - 2026-05-28

### Added

- Initial public GitHub-ready release.
- Modern Windows Forms control panel with Dashboard, Guide and About views.
- Tray menu for status, modes, startup setting, session-lock behavior and exit.
- Power modes:
  - Disabled.
  - System awake.
  - System and display awake.
- Start with Windows support for the current user.
- Disable on session lock support.
- Local settings and log storage under `%LOCALAPPDATA%\KeepOn`.
- Migration support for early `DominTray` settings and startup entries.
- Three publish variants:
  - Portable self-contained.
  - Portable compressed.
  - Framework-dependent.
- MIT license.
- GitHub Actions release workflow.

### Notes

- KeepOn uses the native Windows Power Request API.
- KeepOn does not simulate input, collect telemetry or require network access.
