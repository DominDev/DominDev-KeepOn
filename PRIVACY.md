# Privacy Policy

KeepOn is local-only by design.

## Data Collection

KeepOn does not collect, store or transmit telemetry, analytics, usage metrics,
personal data, machine inventory, file contents, browser activity, window titles,
keyboard input or mouse input.

## Network Access

KeepOn does not communicate with any backend service.

Network access only occurs when the user explicitly clicks one of these links in
the application:

- `Website`, which opens `https://domindev.com` in the default browser,
- `Support`, which opens `https://buycoffee.to/domindev` in the default browser.

Those websites are opened by the user's default browser and are subject to their
own privacy practices.

## Local Settings and Logs

KeepOn stores local settings and logs under the current Windows user profile:

```text
%LOCALAPPDATA%\KeepOn\
%LOCALAPPDATA%\KeepOn\settings.json
%LOCALAPPDATA%\KeepOn\Logs\
```

Settings are used to remember application preferences such as the selected mode,
`Start with Windows`, and `Disable on session lock`.

Logs are local diagnostic text files. They are not uploaded by KeepOn.

## Windows Startup Entry

If the user enables `Start with Windows`, KeepOn writes a current-user startup
entry under:

```text
HKCU\Software\Microsoft\Windows\CurrentVersion\Run
```

This setting can be disabled from the application.

## Contact

For privacy or security questions, use the repository issue tracker or contact
DominDev through:

```text
https://domindev.com
```
