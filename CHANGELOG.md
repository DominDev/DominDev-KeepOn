# Changelog

All notable changes to KeepOn are documented in this file.

## Unreleased

### Added

- Added `CODE_SIGNING_POLICY.md` for SignPath Foundation open source signing
  readiness.
- Added `PRIVACY.md` with explicit local-only and no-telemetry behavior.
- Added SignPath Foundation application notes under `docs/`.
- Added repository `CODEOWNERS`.
- Linked signing, privacy and security validation documents from README.

## [1.6.2] - 2026-05-29

### Changed

- Removed the compressed single-file distribution variant.
- Changed release builds to unpacked framework-dependent and unpacked
  self-contained packages.
- Added SHA-256 checksum generation.
- Added `SECURITY.md` for validation and enterprise review.
- Documented why `AllowUnsafeBlocks` is required for source-generated Windows interop.

## [1.6.1] - 2026-05-29

### Added

- Added support links to README.
- Added `Support` action in the app sidebar.
- Added clickable support link in the About view.

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
