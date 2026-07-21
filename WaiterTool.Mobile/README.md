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

- **Layout:** three tabs (Current / Waiters / History) instead of three
  fixed side-by-side panels. Each tab is its own full-height screen — the
  waiter list and history list fill all available vertical space and
  scroll internally, so you're not scrolling the whole page to reach
  content. Content is centered with a max width on tablets so it doesn't
  stretch edge-to-edge into an awkwardly wide single column.
- **Camera:** mobile has no way to embed a continuous live preview without
  an extra native camera plugin, so tapping **Open Camera** launches the
  device's own camera app; the photo you take comes back into the app. This
  replaces the desktop's embedded live preview, but the end result (a photo
  captured and logged per turn) is the same.
- **Skip:** a standalone button in the bottom-right of the Current Turn /
  Last Capture card (not inside the table-number popup) — logs a photo +
  "Skipped" entry and cycles the waiter to the back of the queue without a
  table number.

## Data storage

Everything persists in the app's private data directory
(`FileSystem.AppDataDirectory`): `queue.json`, `history.json`, and a
`Photos/` folder of captured JPEGs, mirroring the desktop app's format.
