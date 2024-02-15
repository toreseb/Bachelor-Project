using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Electrode_Types.Actuator_Types
{
    internal class Heater(int x, int y, int sizeX, int sizeY) : Actuator(x, y, sizeX, sizeY)
    {
        public string Type { get; private set; } = "heater";
        public int ActualTemprature { get; private set; }
        public int DesiredTemprature { get; private set; }
        public int NextDesiredTemprature { get; private set; }

        public Heater() : this(0, 0, 1, 1)
        {
        }
        
    }
}
