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

        try
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.jsonc", false, true)
                .Build();

            var displayConfig = configuration.Get<DisplayConfig>() ?? new DisplayConfig();

            if (displayConfig is { UseAutoRefreshRate: false, UseAutoHdr: false, UseBrightnessLevel: false })
                Environment.Exit(0);

            var hdrActivated = false;
            var refreshRateChange = false;
            var brightnessLevelChange = false;
            var currentRefreshRate = DisplaySettingsManagerService.GetCurrentRefreshRate();
            var currentBrightnessLevel = DisplaySettingsManagerService.GetCurrentBrightness();
            var currentRefreshRatePersisted = GetCurrentRefreshRatePersisted();
            var currentBrightnessLevelPersisted = GetCurrentBrightnessLevelPersisted();

            if (displayConfig.UseAutoRefreshRate && currentRefreshRatePersisted > 0 &&
                currentRefreshRatePersisted != currentRefreshRate)
            {
                DisplaySettingsManagerService.SetRefreshRate(currentRefreshRatePersisted);
                currentRefreshRate = currentRefreshRatePersisted;
                DeleteLocalStorage();
            }
            
            if (displayConfig.UseBrightnessLevel && currentBrightnessLevelPersisted > 0 &&
                currentBrightnessLevelPersisted != currentBrightnessLevel)
            {
                DisplaySettingsManagerService.SetBrightness(currentRefreshRatePersisted);
                currentRefreshRate = currentRefreshRatePersisted;
                DeleteLocalStorage();
            }

            while (true)
            {
                while (processCount == Process.GetProcesses().Length)
                    Thread.Sleep(1000);

                processCount = Process.GetProcesses().Length;

                foreach (var programDisplayConfig in displayConfig.ProgramDisplayConfigs)
                {
                    if (programDisplayConfig.Active == false)
                        continue;
                    
                    if (Process.GetProcessesByName(programDisplayConfig.ProgramName.Replace(".exe", "")).Length != 0)
                    {
                        if (displayConfig.UseBrightnessLevel && currentBrightnessLevel != programDisplayConfig.BrightnessLevel)
                        {
                            PersistCurrentBrightnessLevel(currentBrightnessLevel);
                            DisplaySettingsManagerService.SetBrightness(programDisplayConfig.BrightnessLevel);
                            brightnessLevelChange = true;
                        }
                        
                        if (displayConfig.UseAutoRefreshRate && currentRefreshRate != programDisplayConfig.RefreshRate)
                        {
                            PersistCurrentRefreshRate(currentRefreshRate);
                            DisplaySettingsManagerService.SetRefreshRate(programDisplayConfig.RefreshRate);
                            refreshRateChange = true;
                        }

                        if (displayConfig.UseAutoHdr && programDisplayConfig.Hdr && hdrActivated == false)
                        {
                            DisplaySettingsManagerService.HdrSwitchOn();
                            hdrActivated = true;
                        }

                        while (Process.GetProcessesByName(programDisplayConfig.ProgramName.Replace(".exe", ""))
                                   .Length != 0)
                            Thread.Sleep(1000);
                    }

                    if (hdrActivated == false && refreshRateChange == false && brightnessLevelChange == false) continue;
                    
                    if (displayConfig.UseBrightnessLevel && brightnessLevelChange)
                        DisplaySettingsManagerService.SetBrightness(currentBrightnessLevel);

                    if (displayConfig.UseAutoHdr && hdrActivated)
                        DisplaySettingsManagerService.HdrSwitchOff();

                    if (displayConfig.UseAutoRefreshRate && refreshRateChange)
                        DisplaySettingsManagerService.SetRefreshRate(currentRefreshRate);
                    
                    DeleteLocalStorage();

                    hdrActivated = false;
                    refreshRateChange = false;
                    brightnessLevelChange = false;
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
    ///     Gets the current brightness level persisted in LocalStorage.
    /// </summary>
    private static uint GetCurrentBrightnessLevelPersisted()
    {
        using var storage = new LocalStorage();
        if (storage.Count <= 0) return 0;
        var brightnessLevel = storage.Get("brightnessLevel").ToString();
        return uint.Parse(brightnessLevel ?? "0");
    }

    /// <summary>
    ///     Persists the current brightness level in LocalStorage.
    /// </summary>
    /// <param name="currentBrightnessLevel">The current brightness level of the monitor.</param>
    private static void PersistCurrentBrightnessLevel(uint currentBrightnessLevel)
    {
        using var storage = new LocalStorage();
        storage.Clear();
        storage.Store("brightnessLevel", currentBrightnessLevel);
        storage.Persist();
    }

    /// <summary>
    ///     Deletes the LocalStorage.
    /// </summary>
    private static void DeleteLocalStorage()
    {
        using var storage = new LocalStorage();
        storage.Clear();
    }
}