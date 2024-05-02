using AutoRefreshHDR.Models;
using System.Diagnostics;

namespace AutoRefreshHDR
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IEnumerable<ProgramDisplayConfig> programDisplayConfigs =
            [
                new ProgramDisplayConfig { ProgramName = "Diablo IV.exe", refreshRate = 144, Hdr = true },
                new ProgramDisplayConfig { ProgramName = "pcsx2-qt.exe", refreshRate = 144},
                new ProgramDisplayConfig { ProgramName = "pcsx2-qt.exe", refreshRate = 144 },
                new ProgramDisplayConfig { ProgramName = "snes9x-x64.exe", refreshRate = 60 },
                new ProgramDisplayConfig { ProgramName = "Cemu.exe", refreshRate = 144 },
            ];

            DisplayConfig displayConfig = new DisplayConfig { CurrentRefreshRate = 120 };

            bool hdrActivated = false;
            bool refreshRateChange = false;

            try
            {
                Console.WriteLine("Checking for the execution of any program in the list...");
                while (true)
                {
                    foreach (var programDisplayConfig in programDisplayConfigs)
                    {
                        if (Process.GetProcessesByName(programDisplayConfig.ProgramName.Replace(".exe", "")).Length != 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Program {programDisplayConfig.ProgramName} is running. Applying changes...");
                            Console.ResetColor();
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
                                ChangeRefreshRate(displayConfig.CurrentRefreshRate);

                            hdrActivated = false;
                            refreshRateChange = false;
                        }
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{e}");
                Console.ResetColor();
            }
        }

        static void ChangeRefreshRate(int refreshRate)
        {
            Console.WriteLine($"Refresh rate changed to: {refreshRate} Hz.");

            var currentDirectory = AppContext.BaseDirectory;
            var pathToQRes = Path.Combine(currentDirectory, "Utils", "QRes.exe");

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
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                Console.WriteLine(output);
                if (string.IsNullOrEmpty(error) == false)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error changing refresh rate: {error}");
                    Console.ResetColor();
                }
            }
        }

        static void HDRSwitchOn()
        {
            var currentDirectory = AppContext.BaseDirectory;
            var pathToHdrSwitchTry = Path.Combine(currentDirectory, "Utils", "hdr_switch_tray.exe");

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
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                Console.WriteLine(output);
                if (string.IsNullOrEmpty(error) == false)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"error switching HDR: {error}");
                    Console.ResetColor();
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
    }
}
