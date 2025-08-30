using System.Diagnostics;
using System.Runtime.InteropServices;
using static AutoRefreshHDR.Models.DisplaySettingsManager;

namespace AutoRefreshHDR.Services;

public abstract class DisplaySettingsManagerService
{
    [DllImport("user32.dll", CharSet = CharSet.Ansi)]
    private static extern bool EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

    [DllImport("user32.dll")]
    private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);
    
    private const int MonitorDefaulttoprimary = 1;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetMonitorBrightness(IntPtr hMonitor,
        out uint pdwMinimumBrightness,
        out uint pdwCurrentBrightness,
        out uint pdwMaximumBrightness);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool SetMonitorBrightness(IntPtr hMonitor, uint dwNewBrightness);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(
        IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetPhysicalMonitorsFromHMONITOR(
        IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool DestroyPhysicalMonitors(
        uint dwPhysicalMonitorArraySize, PHYSICAL_MONITOR[] pPhysicalMonitorArray);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct PHYSICAL_MONITOR
    {
        public IntPtr hPhysicalMonitor;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szPhysicalMonitorDescription;
    }

    /// <summary>
    ///     Returns the current refresh rate of the monitor.
    /// </summary>
    public static uint GetCurrentRefreshRate()
    {
        var dm = new DEVMODE
        {
            dmSize = (short)Marshal.SizeOf(typeof(DEVMODE))
        };
        
        if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm)) return dm.dmDisplayFrequency;

        MessageBox.Show("Error getting current refresh rate", "Error getting current refresh rate",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        return 0;
    }

    /// <summary>
    ///     Changes the refresh rate to the specified value.
    /// </summary>
    /// <param name="newRefreshRate">The new refresh rate (Hz) chosen.</param>
    public static void ChangeRefreshRate(uint newRefreshRate)
    {
        var dm = new DEVMODE
        {
            dmSize = (short)Marshal.SizeOf(typeof(DEVMODE))
        };

        if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
            throw new Exception("Unable to get video settings.");

        dm.dmDisplayFrequency = newRefreshRate;
        dm.dmFields |= DM_DISPLAYFREQUENCY;

        var result = ChangeDisplaySettings(ref dm, CDS_TEST);
        if (result == DISP_CHANGE_SUCCESSFUL)
        {
            result = ChangeDisplaySettings(ref dm, CDS_UPDATEREGISTRY);

            if (result == DISP_CHANGE_SUCCESSFUL)
            {
            }
            else if (result == DISP_CHANGE_RESTART)
            {
                MessageBox.Show(
                    "The change has been applied, but you must restart your computer for it to take effect.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                MessageBox.Show($"Failed to change refresh rate. Error code: {result}", "Error changing refresh rate",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            MessageBox.Show($"The refresh rate {newRefreshRate} Hz is not supported.", "Error changing refresh rate",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    /// <summary>
    ///     Enable HDR via hdr_switch_tray.
    /// </summary>
    public static void HdrSwitchOn()
    {
        var pathToHdrSwitchTry = Path.Combine(AppContext.BaseDirectory, "Utils", "hdr_switch_tray.exe");

        using (Process.Start(pathToHdrSwitchTry))
        {
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = pathToHdrSwitchTry,
            Arguments = "hdr",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        using (var process = Process.Start(startInfo) ?? new Process())
        {
            var output = process.StandardOutput.ReadToEnd();
            var error = output.Contains("Error") ? output : "";

            if (!string.IsNullOrEmpty(error))
                MessageBox.Show(error, "Error switching HDR", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    /// <summary>
    ///     Disable HDR when shutting down the hdr switch tray.
    /// </summary>
    public static void HdrSwitchOff()
    {
        foreach (var proc in Process.GetProcessesByName("hdr_switch_tray")) proc.Kill();
    }

    /// <summary>
    ///     Returns the current brightness of the monitor as a percentage (0–100).
    /// </summary>
    public static uint GetBrightness()
    {
        var hMonitor = MonitorFromWindow(IntPtr.Zero, MonitorDefaulttoprimary);

        if (GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out var numberOfMonitors) == false ||
            numberOfMonitors <= 0) return 0;

        var physicalMonitors = new PHYSICAL_MONITOR[numberOfMonitors];

        if (GetPhysicalMonitorsFromHMONITOR(hMonitor, numberOfMonitors, physicalMonitors) == false) return 0;

        foreach (var monitor in physicalMonitors)
        {
            if (GetMonitorBrightness(monitor.hPhysicalMonitor, out var min, out var current, out var max) == false)
                continue;

            DestroyPhysicalMonitors(numberOfMonitors, physicalMonitors);

            if (max <= min) return 0;

            var percent = (uint)Math.Round((double)(current - min) / (max - min) * 100);

            return percent;
        }

        return 0;
    }
    
    /// <summary>
    ///     Changes the brightness to the specified percentage (0–100).
    /// </summary>
    /// <param name="brightnessPercentage">The new brightness chosen (percentage).</param>
    public static void SetBrightness(uint brightnessPercentage)
    {
        var hMonitor = MonitorFromWindow(IntPtr.Zero, MonitorDefaulttoprimary);

        if (GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out var numberOfMonitors) == false ||
            numberOfMonitors <= 0) return;

        var physicalMonitors = new PHYSICAL_MONITOR[numberOfMonitors];

        if (GetPhysicalMonitorsFromHMONITOR(hMonitor, numberOfMonitors, physicalMonitors) == false) return;

        foreach (var monitor in physicalMonitors)
        {
            if (GetMonitorBrightness(monitor.hPhysicalMonitor, out var min, out var current, out var max) == false) 
                continue;

            var realBrightness = min + brightnessPercentage * (max - min) / 100;

            SetMonitorBrightness(monitor.hPhysicalMonitor, realBrightness);
        }

        DestroyPhysicalMonitors(numberOfMonitors, physicalMonitors);
    }
}