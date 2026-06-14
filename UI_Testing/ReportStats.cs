using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI_Testing
{
    public class ReportStats
    {
        public int TotalTests { get; set; }

        public int Planned { get; set; }

        public int Blocked { get; set; }

        public int AutoTests { get; set; }

        public int Review { get; set; }
    }
}
