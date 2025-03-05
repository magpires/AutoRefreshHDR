using AutoRefreshHDR.Models;
using AutoRefreshHDR.Services;
using Hanssens.Net;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace AutoRefreshHDR
{
    internal class Program
    {
        static void Main(string[] args)
        {            
            try
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", false, true)
                    .Build();

                DisplayConfig displayConfig = configuration.Get<DisplayConfig>() ?? new DisplayConfig();

                if (displayConfig.UseAutoRefreshRate == false && displayConfig.UseAutoHDR == false)
                    Environment.Exit(0);

                int processCount = 0;
                bool hdrActivated = false;
                bool refreshRateChange = false;
                int currentRefreshRate = DisplaySettingsManagerService.GetCurrentRefreshRate();
                int currentRefreshRatePersisted = GetCurrentRefreshRatePersisted();

                if (displayConfig.UseAutoRefreshRate && currentRefreshRatePersisted > 0 && currentRefreshRatePersisted != currentRefreshRate)
                {
                    DisplaySettingsManagerService.ChangeRefreshRate(currentRefreshRatePersisted);
                    currentRefreshRate = currentRefreshRatePersisted;
                    DeleteRefreshRatePersisted();
                }

                while (true)
                {
                    while(processCount == Process.GetProcesses().Length)
                        Thread.Sleep(1000);

                    processCount = Process.GetProcesses().Length;

                    foreach (ProgramDisplayConfig programDisplayConfig in displayConfig.ProgramDisplayConfigs)
                    {
                        if (Process.GetProcessesByName(programDisplayConfig.ProgramName.Replace(".exe", "")).Length != 0)
                        {
                            if (displayConfig.UseAutoRefreshRate)
                            {
                                PersistCurrentRefreshRate(currentRefreshRate);
                                DisplaySettingsManagerService.ChangeRefreshRate(programDisplayConfig.refreshRate);
                                refreshRateChange = true;
                            }

                            if (displayConfig.UseAutoHDR && programDisplayConfig.Hdr && hdrActivated == false)
                            {
                                DisplaySettingsManagerService.HDRSwitchOn();
                                hdrActivated = true;
                            }
                            while (Process.GetProcessesByName(programDisplayConfig.ProgramName.Replace(".exe", "")).Length != 0)
                                Thread.Sleep(1000);
                        }

                        if (hdrActivated || refreshRateChange)
                        {
                            if (displayConfig.UseAutoHDR && hdrActivated)
                                DisplaySettingsManagerService.HDRSwitchOff();

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
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Persists the current refresh rate in LocalStorage.
        /// </summary>
        /// <param name="currentRefreshRate">The current refresh rate of the monitor.</param>
        public static void PersistCurrentRefreshRate(int currentRefreshRate)
        {
            using (LocalStorage storage = new LocalStorage())
            {
                storage.Clear();
                storage.Store("refreshRate", currentRefreshRate);
                storage.Persist();
            }
        }

        /// <summary>
        /// Gets the current refresh rate persisted in LocalStorage.
        /// </summary>
        public static int GetCurrentRefreshRatePersisted()
        {
            using (LocalStorage storage = new LocalStorage())
            {
                if (storage.Count > 0)
                {
                    string? refreshRatetring = storage.Get("refreshRate").ToString();
                    return int.Parse(refreshRatetring ?? "0"); ;
                }
                return 0;
            }
        }

        /// <summary>
        /// Deletes the current refresh rate persisted in LocalStorage.
        /// </summary>
        public static void DeleteRefreshRatePersisted()
        {
            using (LocalStorage storage = new LocalStorage())
            {
                storage.Clear();
            }
        }
    }
}
