using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRefreshHDR.Models
{
    public class DisplayConfig
    {
        public List<ProgramDisplayConfig> ProgramDisplayConfigs { get; set; } = new List<ProgramDisplayConfig>();
    }
}
