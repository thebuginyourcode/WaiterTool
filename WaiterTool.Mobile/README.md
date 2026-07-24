# Waiter Tool (Mobile)

Android port of the Windows Waiter Tool, built with .NET MAUI. Same
concept — a cycling waiter queue with photo-logged table assignments —
adapted for phone and tablet screens.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (net8.0-android
  is no longer a supported workload target as of current .NET tooling)
- MAUI Android workload: `dotnet workload install maui-android`
- An Android device or emulator to install the APK on

## Build the APK

```
dotnet workload install maui-android
dotnet restore WaiterTool.Mobile/WaiterTool.Mobile.csproj
dotnet build WaiterTool.Mobile/WaiterTool.Mobile.csproj -f net10.0-android -c Debug
```

The build produces multiple `.apk` files in
`WaiterTool.Mobile/bin/Debug/net10.0-android/` — **only the one ending in
`-Signed.apk` is installable.** The other(s) are unsigned intermediates
that Android's installer will reject with
`INSTALL_PARSE_FAILED_NO_CERTIFICATES` if you try to install them. Copy
the `-Signed.apk` file to a device and open it (allow "install from
unknown sources" if prompted), or use `adb install <path-to-signed-apk>`.

> **Note:** this was written without a way to run Android tooling, an
> emulator, or a physical device in the environment it was built in, so it
> has **not** been run. Treat the first install as a smoke test and report
> anything that misbehaves.

## How it differs from the desktop version

- **Layout:** a single responsive page instead of three fixed panels. Cards
  (Waiters, Current Turn / Last Capture, History) wrap to one column on a
  narrow phone screen and sit side-by-side on a wider tablet screen,
  reflowing automatically as the window/orientation changes.
- **Camera:** an embedded live front-camera preview (via
  CommunityToolkit.Maui.Camera) right in the Confirm and Skip screens —
  no separate Camera app is launched. Tap **Capture Photo** to take the
  shot, **Retake** to redo it.
- **Skip:** a standalone button in the bottom-right of the Current Turn /
  Last Capture card (not inside the table-number popup) — opens the same
  embedded front-camera flow, logs a "Skipped" entry, and cycles the
  waiter to the back of the queue without a table number.
- **Buttons:** white with a colored outline/text (rather than solid color
  fills) — semantic colors (red for Remove Selected, orange for Skip, gray
  for Cancel) are carried by the border/text instead of the background.

## Data storage

Everything persists in the app's private data directory
(`FileSystem.AppDataDirectory`): `queue.json`, `history.json`, and a
`Photos/` folder of captured JPEGs, mirroring the desktop app's format.
