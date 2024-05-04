using AutoRefreshHDR.Models;
using Hanssens.Net;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AutoRefreshHDR
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                bool hdrActivated = false;
                bool refreshRateChange = false;
                int currentRefreshRate = GetCurrentRefreshRate();
                int currentRefreshRatePersisted = GetCurrentRefreshRatePersisted();

                if (currentRefreshRatePersisted > 0 && currentRefreshRatePersisted != currentRefreshRate)
                {
                    ChangeRefreshRate(currentRefreshRatePersisted);
                    currentRefreshRate = currentRefreshRatePersisted;
                    DeleteRefreshRatePersisted();
                }

                var configFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                DisplayConfig displayConfig = JsonConvert.DeserializeObject<DisplayConfig>(File.ReadAllText(configFilePath)) ?? new DisplayConfig();

                Console.WriteLine("Checking for the execution of any program in the list...");
                while (true)
                {
                    foreach (var programDisplayConfig in displayConfig.ProgramDisplayConfigs)
                    {
                        if (Process.GetProcessesByName(programDisplayConfig.ProgramName.Replace(".exe", "")).Length != 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Program {programDisplayConfig.ProgramName} is running. Applying changes...");
                            Console.ResetColor();
                            PersistCurrentRefreshRate(currentRefreshRate);
                            ChangeRefreshRate(programDisplayConfig.refreshRate);
                            refreshRateChange = true;

                            if (programDisplayConfig.Hdr && hdrActivated == false)
                            {
                                Console.WriteLine($"Activating HDR for {programDisplayConfig.ProgramName}...");
                                HDRSwitchOn();
                                hdrActivated = true;
                            }
                            while (Process.GetProcessesByName(programDisplayConfig.ProgramName.Replace(".exe", "")).Length != 0)
                                Thread.Sleep(1000);
                        }

                        if (hdrActivated || refreshRateChange)
                        {
                            Console.WriteLine($"All programs are closed. Restoring settings...");
                            if (hdrActivated)
                                HDRSwitchOff();

                            if (refreshRateChange)
                            {
                                ChangeRefreshRate(currentRefreshRate);
                                DeleteRefreshRatePersisted();
                            }

                            hdrActivated = false;
                            refreshRateChange = false;
                        }
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void ChangeRefreshRate(int refreshRate)
        {
            Console.WriteLine($"Refresh rate changed to: {refreshRate} Hz.");

            var pathToQRes = Path.Combine(AppContext.BaseDirectory, "Utils", "QRes.exe");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pathToQRes,
                Arguments = $"/c QRes /r:{refreshRate}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process? process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = output.Contains("Error") ? output : "";

                Console.WriteLine(output);
                if (string.IsNullOrEmpty(error) == false)
                {
                    MessageBox.Show(error, "Error changing refresh rate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        static void HDRSwitchOn()
        {
            var pathToHdrSwitchTry = Path.Combine(AppContext.BaseDirectory, "Utils", "hdr_switch_tray.exe");

            using (Process? initialProcess = Process.Start(pathToHdrSwitchTry))
            {
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pathToHdrSwitchTry,
                Arguments = "hdr",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process? process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = output.Contains("Error") ? output : "";

                Console.WriteLine(output);
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

        static int GetCurrentRefreshRate()
        {
            var pathToQRes = Path.Combine(AppContext.BaseDirectory, "Utils", "QRes.exe");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pathToQRes,
                Arguments = $"/c QRes /s",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process? process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                string[] outputSplited = output.Split('@');
                int currentRefreshRate = int.Parse(outputSplited[1].Trim().Replace(" Hz.", ""));

                if (string.IsNullOrEmpty(error) == false)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error getting refresh rate: {error}");
                    Console.ResetColor();
                }

                return currentRefreshRate;
            }
        }

        public static void PersistCurrentRefreshRate(int currentRefreshRate)
        {
            using (var storage = new LocalStorage())
            {
                storage.Clear();
                storage.Store("refreshRate", currentRefreshRate);
                storage.Persist();
            }
        }

        public static int GetCurrentRefreshRatePersisted()
        {
            using (var storage = new LocalStorage())
            {
                if (storage.Count > 0)
                {
                    var refreshRatetring = storage.Get("refreshRate").ToString();
                    return int.Parse(refreshRatetring ?? "0");;
                }
                return 0;
            }
        }

        public static void DeleteRefreshRatePersisted()
        {
            using (var storage = new LocalStorage())
            {
                storage.Clear();
            }
        }
    }
}
