using System;
using System.Runtime.InteropServices;

namespace AutoRefreshHDR.Services;

public static class MonitorBrightnessService
{
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
    ///     Changes the brightness to the specified value.
    /// </summary>
    /// <param name="brightness">The new brightness chosen.</param>
    public static void SetBrightness(uint brightness)
    {
        var hMonitor = MonitorFromWindow(IntPtr.Zero, MonitorDefaulttoprimary);

        if (GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out var numberOfMonitors) == false ||
            numberOfMonitors <= 0) return;
        
        var physicalMonitors = new PHYSICAL_MONITOR[numberOfMonitors];

        if (GetPhysicalMonitorsFromHMONITOR(hMonitor, numberOfMonitors, physicalMonitors) == false) return;
        
        foreach (var monitor in physicalMonitors)
        {
            SetMonitorBrightness(monitor.hPhysicalMonitor, brightness);
        }

        DestroyPhysicalMonitors(numberOfMonitors, physicalMonitors);
    }

    /// <summary>
    ///     Returns the current brightness of the monitor.
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
            if (GetMonitorBrightness(monitor.hPhysicalMonitor, out var min, out var current, out var max) == false) continue;
            
            DestroyPhysicalMonitors(numberOfMonitors, physicalMonitors);
            return current;
        }

        return 0;
    }
}