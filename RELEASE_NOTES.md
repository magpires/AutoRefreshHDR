# AutoRefreshHDR 2.0.0 – Release Notes

## Overview
This release introduces support for controlling monitor brightness via the DDC/CI protocol, expands the configuration model, and refactors the display settings service and main program flow. It is a breaking update over version 1.1.2.

## Versioning
- Bumped application version from `1.1.2` to `2.0.0` in `AutoRefreshHDR.csproj`.

## New Features
- **Brightness control via DDC/CI**
  - Added support for reading and setting monitor brightness through the DDC/CI API (P/Invoke bindings to `dxva2.dll`).
  - New service methods to:
    - Get the current brightness level of the active monitor.
    - Set the brightness level for the active monitor.
  - Brightness can now be driven from configuration alongside refresh rate and HDR.

- **Program-level brightness and state control**
  - Extended per-program configuration to include:
    - `BrightnessLevel` (optional): desired brightness for the program.
    - `Active` (bool): whether the entry is enabled.
  - Program actions (refresh rate/HDR/brightness) are only applied when `Active` is `true`.

## Configuration Changes
- **Config file rename and format**
  - Switched main configuration file from `appsettings.json` to `appsettings.jsonc` (JSON with comments support).

- **Global config options**
  - `UseAutoHdr`: controls whether AutoHDR switching is enabled (renamed from `UseAutoHDR`).
  - `UseBrightnessLevel`: new flag to enable brightness control from configuration.

- **Per-program configuration model**
  - `ProgramDisplayConfig` now includes:
    - `RefreshRate` (`uint?`): optional target refresh rate.
    - `BrightnessLevel` (`uint?`): optional target brightness level.
    - `Active` (`bool`): whether this configuration is applied.
  - Updated examples in `appsettings.jsonc` to demonstrate combinations of refresh rate, brightness, and HDR settings.

## Behavior Changes
- **Main program flow (`Program.cs`)**
  - Now reads configuration from `appsettings.jsonc`.
  - Introduced persistence for last-used values via local storage keys:
    - `refreshRate`
    - `brightnessLevel`
  - New helper methods to get/persist/delete persisted refresh rate and brightness values.
  - Applies settings in a more controlled sequence:
    - Adjusts refresh rate when configured.
    - Toggles HDR on/off where applicable.
    - Applies brightness level with a delay when refresh rate or HDR were changed, to ensure the monitor state is stable.

- **Display settings service (`DisplaySettingsManagerService`)**
  - Refactored and expanded to handle both refresh rate and brightness operations.
  - Renamed methods for clarity:
    - `ChangeRefreshRate` → `SetRefreshRate`.
    - HDR methods normalized to `HdrSwitchOn` / `HdrSwitchOff`.
  - Uses monitor handles (`MonitorFromWindow`, physical monitor APIs) to support DDC/CI calls for brightness.

- **Display settings model (`DisplayConfig`, `DisplaySettingsManager`)**
  - Properties in `DisplayConfig` changed to init-only and updated naming (e.g., `UseAutoHDR` → `UseAutoHdr`).
  - Added `UseBrightnessLevel` to enable global brightness handling.
  - Updated display settings structure so `dmDisplayFrequency` is now `uint` instead of `int`.

## Documentation
- Added `WARP.md` with:
  - Build and run instructions.
  - Detailed description of the configuration schema, including brightness and `Active` flag.
  - High-level architecture overview of services and program flow.

## Miscellaneous
- Updated `.gitignore` to ignore JetBrains Rider/IDEA project files (e.g., `.idea/`).
