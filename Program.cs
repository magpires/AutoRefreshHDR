using System.Diagnostics;
using AutoRefreshHDR.Models;
using AutoRefreshHDR.Services;
using Hanssens.Net;
using Microsoft.Extensions.Configuration;

namespace AutoRefreshHDR;

internal abstract class Program
{
    private static void Main()
    {
        var processCount = 0;
        // var brightness = DisplaySettingsManagerService.GetBrightness();
        // DisplaySettingsManagerService.SetBrightness(100);

        try
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.jsonc", false, true)
                .Build();

            var displayConfig = configuration.Get<DisplayConfig>() ?? new DisplayConfig();

            if (displayConfig is { UseAutoRefreshRate: false, UseAutoHdr: false })
                Environment.Exit(0);

            var hdrActivated = false;
            var refreshRateChange = false;
            var currentRefreshRate = DisplaySettingsManagerService.GetCurrentRefreshRate();
            var currentRefreshRatePersisted = GetCurrentRefreshRatePersisted();

            if (displayConfig.UseAutoRefreshRate && currentRefreshRatePersisted > 0 &&
                currentRefreshRatePersisted != currentRefreshRate)
            {
                DisplaySettingsManagerService.ChangeRefreshRate(currentRefreshRatePersisted);
                currentRefreshRate = currentRefreshRatePersisted;
                DeleteRefreshRatePersisted();
            }

            while (true)
            {
                while (processCount == Process.GetProcesses().Length)
                    Thread.Sleep(1000);

                processCount = Process.GetProcesses().Length;

                foreach (var programDisplayConfig in displayConfig.ProgramDisplayConfigs)
                {
                    if (Process.GetProcessesByName(programDisplayConfig.ProgramName.Replace(".exe", "")).Length != 0)
                    {
                        if (displayConfig.UseAutoRefreshRate)
                        {
                            PersistCurrentRefreshRate(currentRefreshRate);
                            DisplaySettingsManagerService.ChangeRefreshRate(programDisplayConfig.RefreshRate);
                            refreshRateChange = true;
                        }

                        if (displayConfig.UseAutoHdr && programDisplayConfig.Hdr && !hdrActivated)
                        {
                            DisplaySettingsManagerService.HdrSwitchOn();
                            hdrActivated = true;
                        }

                        while (Process.GetProcessesByName(programDisplayConfig.ProgramName.Replace(".exe", ""))
                                   .Length != 0)
                            Thread.Sleep(1000);
                    }

                    if (!hdrActivated && !refreshRateChange) continue;

                    if (displayConfig.UseAutoHdr && hdrActivated)
                        DisplaySettingsManagerService.HdrSwitchOff();

                    if (displayConfig.UseAutoRefreshRate && refreshRateChange)
                    {
                        DisplaySettingsManagerService.ChangeRefreshRate(currentRefreshRate);
                        DeleteRefreshRatePersisted();
                    }

                    hdrActivated = false;
                    refreshRateChange = false;
                }
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    ///     Persists the current refresh rate in LocalStorage.
    /// </summary>
    /// <param name="currentRefreshRate">The current refresh rate of the monitor.</param>
    private static void PersistCurrentRefreshRate(uint currentRefreshRate)
    {
        using var storage = new LocalStorage();
        storage.Clear();
        storage.Store("refreshRate", currentRefreshRate);
        storage.Persist();
    }

    /// <summary>
    ///     Gets the current refresh rate persisted in LocalStorage.
    /// </summary>
    private static uint GetCurrentRefreshRatePersisted()
    {
        using var storage = new LocalStorage();
        if (storage.Count <= 0) return 0;
        var refreshRatetring = storage.Get("refreshRate").ToString();
        return uint.Parse(refreshRatetring ?? "0");
    }

    /// <summary>
    ///     Deletes the current refresh rate persisted in LocalStorage.
    /// </summary>
    private static void DeleteRefreshRatePersisted()
    {
        using var storage = new LocalStorage();
        storage.Clear();
    }
}