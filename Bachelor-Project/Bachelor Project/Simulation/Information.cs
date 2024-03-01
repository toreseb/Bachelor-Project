using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    public class Information
    {
        public string platform_name { get; set; }
        public string platform_type { get; set; }
        public string platform_ID   { get; set; }
        public int sizeX            { get; set; }
        public int sizeY            { get; set; }

        public int electrode_size { get; set; }
        public int eRow { get; set; } // Electrodes per Row
        public int eCol { get; set; } // Electrodes per Column


    }
}
