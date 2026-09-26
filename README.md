# Plex Video Converter

This tool upconverts H.264 video (.mp4/.avi/.mkv) to H.265 (.mkv) to save space on a Plex server without losing
much, if any, noticeable quality.

It was originally a console application that watched a fixed folder with a `FileSystemWatcher` and auto-queued
anything dropped into it. It's now an ASP.NET Core web app (with an Angular front end) where you browse to a
folder using the built-in file browser, review the H.264/size stats for the videos it contains, and choose which
files to queue for conversion. A `ConversionQueueService` manages the queue (queued/active/completed) and
`FfmpegCoreService` does the actual conversion, reporting progress to the browser in real time over SignalR.

The old folder-watching code (`FileListenerService`/`fileListenerSettings.json`) is still present but is not
started automatically anymore — queuing now happens by selecting files in the UI instead of dropping them into a
watched directory.

## How it works

1. Run the app and open the Angular UI (served by the .NET app).
2. Use the file browser to navigate to a folder containing video files.
3. The folder stats view analyzes the videos in that folder (via ffprobe) and estimates space savings for any
   H.264 files found.
4. Select files and queue them for conversion. Progress and queue status stream to the browser live via the
   `pvcConversionHub` SignalR hub.
5. Converted files are written to the configured export folder, and the original is moved to the configured
   post-import folder.

## Requirements

- .NET 8 SDK
- Node.js (for building/running the Angular `ClientApp`)
- FFmpeg and FFprobe available on the `PATH` (used via the `FFMpegCore` library)

## appsettings.json

```json
"FfmpegSettings": {
  "videoQuality": 22,
  "reportPercentProgressFrontend": 1,
  "reportPercentProgressLogging": 20,
  "ffmpegSettingsLocation": ""
}
```

- `videoQuality`: the CRF value passed to ffmpeg's H.265 encoder.
- `reportPercentProgressFrontend`: minimum percent change before a progress update is pushed to the browser.
- `reportPercentProgressLogging`: minimum percent change before progress is written to the log.
- `ffmpegSettingsLocation`: folder containing `fileListenerSettings.json` (used for the legacy import/export/
  post-import folder definitions). Leave empty to use the app's own folder if `fileListenerSettings.json` is
  found there, otherwise a per-user app data folder is used (see Windows/macOS setup below).

## fileListenerSettings.json

Still used to define the EXPORT and POST-IMPORT destination folders (and, if you re-enable
`FileListenerService.StartFileSystemWatcher()`, the IMPORT folders it would watch).

1. FolderID (string)
   Unique identifier of the combination File type/folder
   Arbitrary number (for instance 001, 002, and so on)

2. FolderEnabled (boolean)
   If TRUE: the file type and folder will be monitored, otherwise ignored

3. FolderType (string)
   Type of folder: EXPORT, IMPORT, POST-IMPORT

4. FolderDescription (string)
   Description of the type of files and folder location – Just for documentation purpose

5. FolderFilter (string)
   Filter to select the type of files to be monitored.
   (Examples: *.mkv, *.*, Ted.Lasso.S1*.mp4)

6. FolderPath (string)
   Full path to be monitored (i.e.: D:\files\movies\TedLasso\ on Windows,
   /Users/me/Movies/TedLasso/ on macOS)

7. FolderIncludeSub (boolean)
   If TRUE: the folder and its subfolders will be monitored

8. ExecutableFile (string)
   Specifies the command or action to be executed after an event has raised

9. ExecutableArguments (string)
   List of arguments to be passed to the executable file

## Running on Windows

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download) and [Node.js](https://nodejs.org/).
2. Install [FFmpeg](https://ffmpeg.org/download.html) and make sure `ffmpeg.exe`/`ffprobe.exe` are on your `PATH`
   (e.g. via `winget install Gyan.FFmpeg`, or by adding the extracted `bin` folder to your `PATH` environment
   variable).
3. Build/run from the repo root:
   ```powershell
   dotnet build PlexVideoConverter/PlexVideoConverter.csproj
   dotnet run --project PlexVideoConverter/PlexVideoConverter.csproj
   ```
4. If `ffmpegSettingsLocation` is left empty, settings default to
   `%LOCALAPPDATA%\PlexVideoConverter\fileListenerSettings.json` — create that folder/file if it doesn't exist,
   or point `ffmpegSettingsLocation` at a folder of your choice.
5. Use Windows-style paths (`C:\Exports\Videos\...`) in `fileListenerSettings.json`.

## Running on macOS

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download) and Node.js (e.g. `brew install node`).
2. Install FFmpeg: `brew install ffmpeg`. This puts `ffmpeg`/`ffprobe` on your `PATH` automatically.
3. Trust the local HTTPS dev certificate (only needed once): `dotnet dev-certs https --trust`.
4. Build/run from the repo root:
   ```bash
   dotnet build PlexVideoConverter/PlexVideoConverter.csproj
   dotnet run --project PlexVideoConverter/PlexVideoConverter.csproj
   ```
5. If `ffmpegSettingsLocation` is left empty, settings default to
   `~/.local/share/PlexVideoConverter/fileListenerSettings.json` — create that folder/file if it doesn't exist,
   or point `ffmpegSettingsLocation` at a folder of your choice.
6. Use Unix-style paths (`/Users/you/Movies/ffmpeg-ToConvert/...`) in `fileListenerSettings.json`. The file
   browser lists `/` plus any mounted volumes under `/Volumes` instead of drive letters.

The Angular front end itself requires no platform-specific setup — it runs the same on both OSes.


