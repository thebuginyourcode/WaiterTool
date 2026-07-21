# Waiter Tool

A Windows desktop app for cycling through a list of waiters. Add names to
a queue; click **Next Waiter** to bring up the next person in rotation;
enter a table number and take a webcam photo to confirm the assignment.
The waiter then cycles back to the end of the queue, and the photo/table/time
is logged in the history panel.

## Requirements

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A webcam (the app still works without one — it'll just skip the photo)

## Build & run

```
dotnet restore
dotnet build
dotnet run --project WaiterTool
```

Or open `WaiterTool.sln` in Visual Studio 2022+ and press F5.

> **Note:** this project was written in a Linux container without a Windows
> machine, .NET SDK, or webcam available, so it has **not** been compiled or
> run. Please build it on a Windows machine before relying on it, and file
> any build errors you hit so they can be fixed — the most likely snag is
> the pinned `OpenCvSharp4*` NuGet package versions in `WaiterTool.csproj`;
> if `dotnet restore` can't find that exact version, bump it to the latest
> `4.x` release from NuGet.

## Publish a standalone .exe

```
dotnet publish WaiterTool -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The output lands in `WaiterTool/bin/Release/net8.0-windows/win-x64/publish/`.

## How it works

- **Waiters panel (left):** add/remove names in the rotation queue.
- **Current Turn (center):** shows the waiter at the front of the queue.
  Click **Next Waiter** to pop them and open the table-number dialog.
- **Table Number dialog:** shows a live camera preview, prompts for a table
  number, and on **Confirm & Capture** snaps the current frame as the photo
  and returns the waiter to the *back* of the queue (the "cycling" part).
  **Cancel** puts the waiter back at the front instead, so their turn isn't
  lost.
- **History panel (right):** every confirmed entry (waiter, table, time,
  photo thumbnail) newest-first.

## Data storage

Everything persists across restarts in:

```
%AppData%\WaiterTool\
  queue.json     # current waiter rotation
  history.json   # log of past table assignments
  Photos\        # captured JPEGs, named by entry ID
```

## Tech notes

- WPF on .NET 8.
- Webcam capture via [OpenCvSharp4](https://github.com/shimat/opencvsharp)
  (`VideoCaptureAPIs.DSHOW` backend), opened only while the table-number
  dialog is open.
- No external services or network calls — everything is local.
