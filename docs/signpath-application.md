# SignPath Foundation Application Notes

This document collects the information needed to apply for SignPath Foundation
open source code signing for KeepOn.

## Project Summary

KeepOn is a small Windows tray application that uses the native Windows Power
Request API to temporarily keep the system awake.

It does not simulate user input, inject into other processes, install services
or drivers, download payloads, collect telemetry, or communicate with a backend
service.

## Repository

```text
https://github.com/DominDev/DominDev-KeepOn
```

## License

```text
MIT
```

## Release Artifacts

Current release artifacts are ZIP packages:

```text
KeepOn-framework-dependent-win-x64.zip
KeepOn-portable-self-contained-win-x64.zip
SHA256SUMS.txt
```

The preferred artifact for corporate and managed environments is:

```text
KeepOn-framework-dependent-win-x64.zip
```

This package is not self-contained and is not a packed single-file executable.

## Why Signing Is Needed

Unsigned Windows executables can be treated with extra caution by endpoint
security products, especially in managed corporate environments. KeepOn uses
documented Windows APIs and has a narrow behavior profile, so Authenticode
signing is intended to provide artifact integrity and publisher traceability.

## Required Repository Documents

- `README.md`
- `LICENSE`
- `SECURITY.md`
- `PRIVACY.md`
- `CODE_SIGNING_POLICY.md`
- `.github/CODEOWNERS`

## Behavior Documents

Security behavior is documented in:

```text
SECURITY.md
```

Privacy behavior is documented in:

```text
PRIVACY.md
```

Code signing policy is documented in:

```text
CODE_SIGNING_POLICY.md
```

## Suggested SignPath Application Text

```text
KeepOn is an open source Windows tray utility for temporary power request
control. It uses the native Windows Power Request API to keep the system awake
or keep the system and display awake, based on explicit user selection.

The application does not simulate keyboard or mouse input, does not inject into
other processes, does not install services or drivers, does not require
administrator privileges, does not collect telemetry, does not communicate with
a backend service, and does not download or execute additional payloads.

The repository includes MIT licensing, security notes, a privacy policy, and a
code signing policy. Release artifacts are built by GitHub Actions and are
distributed as unpacked ZIP packages rather than packed single-file executables.

The project is applying for SignPath Foundation signing to provide Authenticode
integrity and origin verification for official open source releases.
```

## After Acceptance

After SignPath Foundation accepts the project:

1. Install and authorize the SignPath GitHub App for this repository.
2. Create the SignPath project for KeepOn.
3. Create an artifact configuration for the release ZIP package.
4. Create a signing policy for tagged releases.
5. Add the SignPath API token as a GitHub Actions secret.
6. Update `.github/workflows/release.yml` to submit the unsigned workflow
   artifact to SignPath before creating the GitHub Release.
7. Ensure release checksums are generated after signing.
8. Publish the signed release and verify `Get-AuthenticodeSignature`.

Do not submit locally built artifacts for official signing. Official signed
artifacts should come from the GitHub Actions release workflow.
