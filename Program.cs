using AutoRefreshHDR.Models;
using AutoRefreshHDR.Services;
using Hanssens.Net;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
                                HDRSwitchOn();
                                hdrActivated = true;
                            }
                            while (Process.GetProcessesByName(programDisplayConfig.ProgramName.Replace(".exe", "")).Length != 0)
                                Thread.Sleep(1000);
                        }

                        if (hdrActivated || refreshRateChange)
                        {
                            if (displayConfig.UseAutoHDR && hdrActivated)
                                HDRSwitchOff();

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

        static void HDRSwitchOn()
        {
            string pathToHdrSwitchTry = Path.Combine(AppContext.BaseDirectory, "Utils", "hdr_switch_tray.exe");

            using (Process? initialProcess = Process.Start(pathToHdrSwitchTry))
            {
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pathToHdrSwitchTry,
                Arguments = "hdr",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using (Process process = Process.Start(startInfo) ?? new Process())
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = output.Contains("Error") ? output : "";

                if (string.IsNullOrEmpty(error) == false)
                {
                    MessageBox.Show(error, "Error switching HDR", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        static void HDRSwitchOff()
        {
            foreach (Process proc in Process.GetProcessesByName("hdr_switch_tray"))
            {
                proc.Kill();
            }
        }

        public static void PersistCurrentRefreshRate(int currentRefreshRate)
        {
            using (LocalStorage storage = new LocalStorage())
            {
                storage.Clear();
                storage.Store("refreshRate", currentRefreshRate);
                storage.Persist();
            }
        }

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

        public static void DeleteRefreshRatePersisted()
        {
            using (LocalStorage storage = new LocalStorage())
            {
                storage.Clear();
            }
        }
    }
}
