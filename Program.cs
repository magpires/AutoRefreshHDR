using System.Diagnostics;

namespace AutoRefreshHDR
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[] programPaths = { "Diablo IV.exe", "pcsx2-qt.exe", "Cemu.exe" };
            string[] hdrPrograms = { "Diablo IV.exe" };
            int newRefreshRate = 144;
            int oldRefreshRate = 120;
            bool hdrActivated = false;
            bool monitorIn144hzMode = false;

            try
            {
                Console.WriteLine("Checking for the execution of any program in the list...");
                while (true)
                {
                    foreach (var program in programPaths)
                    {
                        if (Process.GetProcessesByName(program.Replace(".exe", "")).Length != 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Program {program} is running. Applying changes...");
                            Console.ResetColor();
                            ChangeRefreshRate(newRefreshRate);
                            monitorIn144hzMode = true;

                            if (hdrPrograms.Contains(program) && !hdrActivated)
                            {
                                Console.WriteLine($"Activating HDR for {program}...");
                                HDRSwitch();
                                hdrActivated = true;
                            }
                            while (Process.GetProcessesByName(program.Replace(".exe", "")).Length != 0)
                                Thread.Sleep(1000);
                        }

                        if (hdrActivated || monitorIn144hzMode)
                        {
                            Console.WriteLine($"All programs are closed. Restoring settings...");
                            if (hdrActivated)
                                HDRSwitch();

                            ChangeRefreshRate(oldRefreshRate);
                            hdrActivated = false;
                            monitorIn144hzMode = false;
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

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
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

        static void HDRSwitch()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c hdr_switch_tray.exe hdr",
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
    }
}
