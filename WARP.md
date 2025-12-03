# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Tech stack and targets

- .NET 8 (`net8.0-windows10.0.22621.0`) WinExe using Windows Forms for UI primitives (e.g., `MessageBox`).
- Windows-only tool that runs in the background and manipulates display settings (refresh rate, HDR, brightness) based on running processes.

## Build and run

From the repository root (`AutoRefreshHDR`):

- Restore and build (Debug):
  - `dotnet restore`
  - `dotnet build AutoRefreshHDR.sln -c Debug`
- Build optimized (Release):
  - `dotnet build AutoRefreshHDR.sln -c Release`
- Run the app directly from source (Debug build):
  - `dotnet run --project AutoRefreshHDR.csproj -c Debug`
- Run the compiled binary (after a build):
  - Debug: `./bin/Debug/net8.0-windows10.0.22621.0/AutoRefreshHDR.exe`
  - Release: `./bin/Release/net8.0-windows10.0.22621.0/AutoRefreshHDR.exe`

### Tests

- There is currently no test project in this repository. If/when test projects are added (e.g., `*.Tests.csproj`):
  - Run all tests: `dotnet test`
  - Run tests in a specific project: `dotnet test path/to/YourProject.Tests.csproj`
  - Run a single test (example filter): `dotnet test path/to/YourProject.Tests.csproj --filter "FullyQualifiedName~YourTestName"`

## Configuration and runtime behavior

Configuration lives in `appsettings.jsonc` in the repo root and is copied to the output directory on build. It uses JSON with comments.

Key structure (mirrors `Models/DisplayConfig.cs` and `Models/ProgramDisplayConfig.cs`):

- `ProgramDisplayConfigs`: array of per-program rules.
  - `ProgramName` (string): executable name of the target app, e.g. `game.exe`.
  - `RefreshRate` (uint?, Hz): desired refresh rate while the program is running.
  - `Hdr` (bool): whether HDR should be turned on while the program is running.
  - `BrightnessLevel` (uint?, 0–100): desired brightness percentage while the program is running.
  - `Active` (bool): whether this rule is considered at runtime.
- Global flags (on the root object):
  - `UseAutoRefreshRate` (bool): master switch for applying per-program refresh rate changes.
  - `UseAutoHDR` (bool): master switch for HDR toggling.
  - `UseBrightnessLevel` (bool): master switch for brightness adjustments.

Important behaviors:

- At startup, `Program` deserializes `DisplayConfig` from `appsettings.jsonc`.
- If **all** three flags (`UseAutoRefreshRate`, `UseAutoHdr`, `UseBrightnessLevel`) are `false`, the process exits immediately without doing anything.
- Otherwise, `Program`:
  - Reads the current refresh rate and brightness via `DisplaySettingsManagerService`.
  - Checks `LocalStorage` (Hanssens.Net) for previously persisted values and restores them if present (this handles recovery when the tool restarts while a session was in progress).
  - Enters an infinite loop that watches for changes in the process list and applies rules when matching processes appear.

## Batch scripts and typical usage

These scripts are intended to control the background tool after it has been built/published:

- `RestartAutoRefreshHDR.bat`:
  - Kills any running `AutoRefreshHDR.exe` process.
  - Waits briefly, then starts `AutoRefreshHDR.exe` from the current directory.
  - Use after editing `appsettings.jsonc` to reload configuration.
- `KillAutoRefreshHDR.bat`:
  - Kills `AutoRefreshHDR.exe` and exits.
  - Use when you want to stop the tool without restarting it.

The `Utils/hdr_switch_tray.exe` binary is required for HDR toggling. It is copied to the output directory on build and is invoked by `DisplaySettingsManagerService`.

## High-level architecture

**Entry point and orchestration (`Program.cs`)**

- `Program.Main` is the only entry point and coordinates the entire lifecycle:
  - Sets the process priority to `Idle` so it does not interfere with foreground apps.
  - Builds an `IConfiguration` and binds it to `DisplayConfig` from `appsettings.jsonc`.
  - Reads the current refresh rate and brightness, and any previously persisted values (via `LocalStorage`).
  - If there are persisted values and the corresponding global features are enabled, it restores them once at startup.
  - Enters an infinite loop:
    - Waits until the total number of processes changes, then re-scans.
    - For each `ProgramDisplayConfig` whose `Active` flag is `true`:
      - If the corresponding process (`ProgramName` without `.exe`) is running:
        - Optionally persists the current refresh rate/brightness to `LocalStorage` before changing them.
        - Calls `DisplaySettingsManagerService` to:
          - Change refresh rate.
          - Toggle HDR on.
          - Change brightness, optionally after a small delay if refresh/HDR were just changed to avoid timing issues.
        - Blocks in a loop until the process exits.
        - On exit, restores original refresh rate, brightness, and HDR state (if those features are enabled), then clears `LocalStorage`.
  - Wraps all logic in a `try/catch` that shows any fatal exception in a `MessageBox`.

**Configuration models (`Models/DisplayConfig.cs`, `Models/ProgramDisplayConfig.cs`)**

- `DisplayConfig`:
  - Represents the root of `appsettings.jsonc`.
  - Holds `ProgramDisplayConfigs` plus the three global feature switches.
- `ProgramDisplayConfig`:
  - Represents one per-program rule.
  - Includes program identity (`ProgramName`), the desired display attributes (`RefreshRate`, `Hdr`, `BrightnessLevel`), and an `Active` toggle.

These classes are designed purely as configuration contracts and are not responsible for any logic.

**Display interop model (`Models/DisplaySettingsManager.cs`)**

- Contains the `DEVMODE` struct and constants that mirror the Win32 display configuration API.
- Used exclusively by `DisplaySettingsManagerService` for P/Invoke calls into `user32.dll`.

**Display and HDR service layer (`Services/DisplaySettingsManagerService.cs`)**

- Static service responsible for all interactions with the Windows display stack and the external HDR utility:
  - **Refresh rate control**
    - `GetCurrentRefreshRate()` uses `EnumDisplaySettings` to read the current monitor refresh rate.
    - `SetRefreshRate(uint)` uses `ChangeDisplaySettings` to test and apply a new refresh rate and shows user-friendly `MessageBox` messages for unsupported values or required restarts.
  - **HDR control**
    - `HdrSwitchOn()` launches `Utils/hdr_switch_tray.exe` and then invokes it with the `hdr` argument, capturing output to detect and display any errors.
    - `HdrSwitchOff()` kills all running `hdr_switch_tray` processes.
  - **Brightness control**
    - `GetCurrentBrightness()` enumerates physical monitors via `dxva2.dll`, reads min/current/max brightness, and converts the current level to a 0–100 percentage.
    - `SetBrightness(uint)` converts a target percentage back to the real hardware value and calls `SetMonitorBrightness` for each physical monitor.

`Program` is the orchestration layer; `DisplaySettingsManagerService` encapsulates all platform-specific logic. Changes to how display settings are applied should generally be isolated to the service.

## Important information from README

- Requires **.NET 8** on Windows 10 or higher (x64) to build and run from source.
- End users configure `appsettings.jsonc` and typically interact with the built `AutoRefreshHDR.exe` plus the provided batch scripts.
- The tool does **not** overclock the monitor; unsupported refresh rates or HDR modes result in informative warnings rather than silent failure.
- Dynamic refresh rate in Windows may be disabled while the tool is managing refresh rate; the README suggests turning off `UseAutoRefreshRate` if this is undesirable.
