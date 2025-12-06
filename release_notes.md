# Release Notes - feature/control-brightness-with-DDC-protocol

This version marks a significant evolution of AutoRefreshHDR, introducing major new features for display management, a dedicated configuration editor, and numerous performance and stability improvements.

### ✨ New Features

*   **Automated Brightness Control**:
    *   The application can now automatically adjust your monitor's brightness based on the running application, in addition to managing HDR and refresh rates.
    *   Brightness is configured as a percentage value within the `appsettings.jsonc` file for each specific program.
    *   The core logic has been refactored into a `DisplaySettingsManagerService` for better maintainability.

*   **Configuration Editor (AutoRefreshHDR.ConfigEditor)**:
    *   A new graphical user interface (GUI) tool has been introduced to easily manage the `appsettings.jsonc` configuration file. This removes the need for manual JSON editing, making the setup process much more user-friendly.

### 🚀 Improvements & Optimizations

*   **Performance**: The application now checks the current monitor settings (brightness, refresh rate) before applying new ones. It will skip the change if the target setting is already active, reducing unnecessary operations.
*   **User Experience**: A delay has been added when updating brightness, but only if the HDR status or refresh rate has also been changed, leading to smoother transitions.
*   **Project Structure**: The solution has been refactored into separate projects (`AutoRefreshHDR` for the main application and `AutoRefreshHDR.ConfigEditor` for the GUI tool), improving code organization and separation of concerns.
*   **Configuration**:
    *   The configuration file was renamed from `appsettings.json` to `appsettings.jsonc` to officially support comments.
    *   The configuration schema was updated to include `BrightnessLevel` and `Active` properties for program-specific settings.

### 🛠️ Bug Fixes

*   Fixed a critical bug where, upon closing a configured application, the monitor settings would revert to a generic profile instead of the correct one for the previously running application.
*   Corrected an issue that occurred when attempting to update the monitor's refresh rate.
*   Addressed a bug related to handling null values in the application's local storage.