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

            while (true)
            {
                foreach (var program in programPaths)
                {
                    Console.WriteLine($"Checking the {program} program");
                    if (Process.GetProcessesByName(program.Replace(".exe", "")).Length != 0)
                    {
                        Console.WriteLine($"Program {program} is running. Applying changes...");
                        ChangeRefreshRate(newRefreshRate);
                        monitorIn144hzMode = true;

                        if (hdrPrograms.Contains(program) && !hdrActivated)
                        {
                            Console.WriteLine($"Activating HDR for {program}...");
                            ActivateHDR();
                            hdrActivated = true;
                        }
                        while(Process.GetProcessesByName(program.Replace(".exe", "")).Length != 0)
                            Thread.Sleep(1000);
                    }

                    if (hdrActivated || monitorIn144hzMode)
                    {
                        Console.WriteLine($"All programs are closed. Restoring settings...");
                        if (hdrActivated)
                            DeactivateHDR();

                        ChangeRefreshRate(oldRefreshRate);
                        hdrActivated = false;
                        monitorIn144hzMode = false;
                    }

                    // Wait for a bit before checking the next program
                    Thread.Sleep(1000);
                }
            }
        }

        static void ChangeRefreshRate(int refreshRate)
        {
            // Implement the logic to change the refresh rate
        }

        static void ActivateHDR()
        {
            // Implement the logic to activate HDR
        }

        static void DeactivateHDR()
        {
            // Implement the logic to deactivate HDR
        }
    }
}
