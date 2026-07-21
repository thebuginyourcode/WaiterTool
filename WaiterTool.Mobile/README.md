# Waiter Tool (Mobile)

Android port of the Windows Waiter Tool, built with .NET MAUI. Same
concept — a cycling waiter queue with photo-logged table assignments —
adapted for phone and tablet screens.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MAUI Android workload: `dotnet workload install maui-android`
- An Android device or emulator to install the APK on

## Build the APK

```
dotnet workload install maui-android
dotnet restore WaiterTool.Mobile/WaiterTool.Mobile.csproj
dotnet build WaiterTool.Mobile/WaiterTool.Mobile.csproj -f net8.0-android -c Debug
```

The installable (debug-signed) APK lands in
`WaiterTool.Mobile/bin/Debug/net8.0-android/`. Copy it to a device and open
it (allow "install from unknown sources" if prompted), or use
`adb install <path-to-apk>`.

> **Note:** this was written without a way to run Android tooling, an
> emulator, or a physical device in the environment it was built in, so it
> has **not** been run. Treat the first install as a smoke test and report
> anything that misbehaves.

## How it differs from the desktop version

- **Layout:** a single responsive page instead of three fixed panels. Cards
  (Waiters, Current Turn / Last Capture, History) wrap to one column on a
  narrow phone screen and sit side-by-side on a wider tablet screen,
  reflowing automatically as the window/orientation changes.
- **Camera:** mobile has no way to embed a continuous live preview without
  an extra native camera plugin, so tapping **Open Camera** launches the
  device's own camera app; the photo you take comes back into the app. This
  replaces the desktop's embedded live preview, but the end result (a photo
  captured and logged per turn) is the same.
- **Skip:** same as desktop — logs a photo + "Skipped" entry and cycles the
  waiter to the back of the queue without a table number.

## Data storage

Everything persists in the app's private data directory
(`FileSystem.AppDataDirectory`): `queue.json`, `history.json`, and a
`Photos/` folder of captured JPEGs, mirroring the desktop app's format.
