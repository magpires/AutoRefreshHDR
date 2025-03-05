using System.Diagnostics;
using System.Runtime.InteropServices;
using static AutoRefreshHDR.Models.DisplaySettingsManager;

namespace AutoRefreshHDR.Services
{
    public class DisplaySettingsManagerService
    {
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern bool EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        /// <summary>
        /// Returns the current refresh rate of the monitor.
        /// </summary>
        public static int GetCurrentRefreshRate()
        {
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
            {
                return dm.dmDisplayFrequency;
            }
            else
            {
                MessageBox.Show("Error getting current refresh rate", "Error getting current refresh rate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return 0;
            }
        }

        /// <summary>
        /// Changes the refresh rate to the specified value.
        /// </summary>
        /// <param name="newRefreshRate">The new refresh rate (Hz) chosen.</param>
        public static void ChangeRefreshRate(int newRefreshRate)
        {
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm) == false)
            {
                throw new Exception("Unable to get video settings.");
            }

            dm.dmDisplayFrequency = newRefreshRate;
            dm.dmFields |= DM_DISPLAYFREQUENCY;

            int result = ChangeDisplaySettings(ref dm, CDS_TEST);
            if (result == DISP_CHANGE_SUCCESSFUL)
            {
                result = ChangeDisplaySettings(ref dm, CDS_UPDATEREGISTRY);

                if (result == DISP_CHANGE_SUCCESSFUL)
                {
                    return;
                }
                else if (result == DISP_CHANGE_RESTART)
                {
                    MessageBox.Show("The change has been applied, but you must restart your computer for it to take effect.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    MessageBox.Show($"Failed to change refresh rate. Error code: {result}", "Error changing refresh rate", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show($"The refresh rate {newRefreshRate} Hz is not supported.", "Error changing refresh rate", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Enable HDR via hdr_switch_tray.
        /// </summary>
        public static void HDRSwitchOn()
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

        /// <summary>
        /// Disable HDR when shutting down the hdr switch tray.
        /// </summary>
        public static void HDRSwitchOff()
        {
            foreach (Process proc in Process.GetProcessesByName("hdr_switch_tray"))
            {
                proc.Kill();
            }
        }
    }
}
