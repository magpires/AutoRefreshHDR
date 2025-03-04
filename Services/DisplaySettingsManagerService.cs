using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static AutoRefreshHDR.Models.DisplaySettingsManager;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    }
}
