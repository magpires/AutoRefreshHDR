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

        const int millisecondsToSetbrightness = 2000;

        try
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.jsonc", false, true)
                .Build();

            var displayConfig = configuration.Get<DisplayConfig>() ?? new DisplayConfig();

            if (displayConfig is { UseAutoRefreshRate: false, UseAutoHdr: false, UseBrightnessLevel: false })
                Environment.Exit(0);
            
            var currentRefreshRate = DisplaySettingsManagerService.GetCurrentRefreshRate();
            var currentBrightnessLevel = DisplaySettingsManagerService.GetCurrentBrightness();
            var currentRefreshRatePersisted = GetCurrentRefreshRatePersisted();
            var currentBrightnessLevelPersisted = GetCurrentBrightnessLevelPersisted();

            if (displayConfig.UseAutoRefreshRate &&
                currentRefreshRatePersisted is not null &&
                currentRefreshRatePersisted != currentRefreshRate)
            {
                DisplaySettingsManagerService.SetRefreshRate(currentRefreshRatePersisted.Value);
                PersistCurrentRefreshRate(null);
            }
            
            if (displayConfig.UseBrightnessLevel &&
                currentBrightnessLevelPersisted is not null &&
                currentBrightnessLevelPersisted != currentBrightnessLevel)
            {
                DisplaySettingsManagerService.SetBrightness(currentBrightnessLevelPersisted.Value);
                PersistCurrentBrightnessLevel(null);
            }

            var hdrActivated = false;
            var refreshRateChange = false;
            var brightnessLevelChange = false;
            
            while (true)
            {
                while (processCount == Process.GetProcesses().Length)
                    Thread.Sleep(1000);

                processCount = Process.GetProcesses().Length;

                foreach (var programDisplayConfig in displayConfig.ProgramDisplayConfigs)
                {
                    if (programDisplayConfig.Active == false)
                        continue;

                    if (Process.GetProcessesByName(programDisplayConfig.ProgramName.Replace(".exe", "")).Length ==
                        0) continue;
                    
                    currentRefreshRate = DisplaySettingsManagerService.GetCurrentRefreshRate();
                    currentBrightnessLevel = DisplaySettingsManagerService.GetCurrentBrightness();

                    if (displayConfig.UseAutoRefreshRate &&
                        programDisplayConfig.RefreshRate is not null &&
                        currentRefreshRate != programDisplayConfig.RefreshRate)
                    {
                        PersistCurrentRefreshRate(currentRefreshRate);
                        DisplaySettingsManagerService.SetRefreshRate(programDisplayConfig.RefreshRate.Value);
                        refreshRateChange = true;
                    }

                    if (displayConfig.UseAutoHdr && programDisplayConfig.Hdr && hdrActivated == false)
                    {
                        DisplaySettingsManagerService.HdrSwitchOn();
                        hdrActivated = true;
                    }

                    if (displayConfig.UseBrightnessLevel &&
                        programDisplayConfig.BrightnessLevel is not null &&
                        currentBrightnessLevel != programDisplayConfig.BrightnessLevel)
                    {
                        Thread.Sleep(millisecondsToSetbrightness);
                        PersistCurrentBrightnessLevel(currentBrightnessLevel);
                        DisplaySettingsManagerService.SetBrightness(programDisplayConfig.BrightnessLevel.Value);
                        brightnessLevelChange = true;
                    }

                    while (Process.GetProcessesByName(programDisplayConfig.ProgramName.Replace(".exe", ""))
                               .Length != 0)
                        Thread.Sleep(1000);
                        
                    if (hdrActivated == false && refreshRateChange == false && brightnessLevelChange == false) continue;

                    if (displayConfig.UseAutoHdr && hdrActivated)
                        DisplaySettingsManagerService.HdrSwitchOff();

                    if (displayConfig.UseAutoRefreshRate && refreshRateChange)
                        DisplaySettingsManagerService.SetRefreshRate(currentRefreshRate);

                    if (displayConfig.UseBrightnessLevel && brightnessLevelChange)
                    {
                        Thread.Sleep(millisecondsToSetbrightness);
                        DisplaySettingsManagerService.SetBrightness(currentBrightnessLevel);
                    }
                    
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
    ///     Gets the current refresh rate persisted in LocalStorage.
    /// </summary>
    private static uint? GetCurrentRefreshRatePersisted()
    {
        using var storage = new LocalStorage();
        if (storage.Count <= 0) return null;

        string? refreshRatetring;
        
        try
        {
            refreshRatetring = storage.Get("refreshRate").ToString();
        }
        catch (Exception)
        {
            return null;
        }
        
        if (uint.TryParse(refreshRatetring, out var value))
            return value;

        return null;
    }

    /// <summary>
    ///     Persists the current refresh rate in LocalStorage.
    /// </summary>
    /// <param name="currentRefreshRate">The current refresh rate of the monitor.</param>
    private static void PersistCurrentRefreshRate(uint? currentRefreshRate)
    {
        using var storage = new LocalStorage();

        if (currentRefreshRate is null)
        {
            storage.Remove("refreshRate");
        }
        else
        {
            storage.Store("refreshRate", currentRefreshRate.Value);
        }

        storage.Persist();
    }

    /// <summary>
    ///     Gets the current brightness level persisted in LocalStorage.
    /// </summary>
    private static uint? GetCurrentBrightnessLevelPersisted()
    {
        using var storage = new LocalStorage();
        if (storage.Count <= 0) return null;

        string? brightnessLevel;
        
        try
        {
            brightnessLevel = storage.Get("brightnessLevel").ToString();
        }
        catch (Exception)
        {
            return null;
        }
        
        if (uint.TryParse(brightnessLevel, out var value))
            return value;

        return null;
    }

    /// <summary>
    ///     Persists the current brightness level in LocalStorage.
    /// </summary>
    /// <param name="currentBrightnessLevel">The current brightness level of the monitor.</param>
    private static void PersistCurrentBrightnessLevel(uint? currentBrightnessLevel)
    {
        using var storage = new LocalStorage();

        if (currentBrightnessLevel is null)
        {
            storage.Remove("brightnessLevel");
        }
        else
        {
            storage.Store("brightnessLevel", currentBrightnessLevel.Value);
        }

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