using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Electrode_Types.Actuator_Types
{
    public class Heater(int x, int y, int sizeX, int sizeY) : Actuator(x, y, sizeX, sizeY)
    {
        public override string Type { get; set; } = "heater";
        public int ActualTemperature { get; set; }
        public int DesiredTemperature { get; set; }
        public int NextDesiredTemperature { get; set; }

        public Heater() : this(0, 0, 1, 1)
        {
        }
        
    }
}
