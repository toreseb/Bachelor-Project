using Bachelor_Project.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Electrode_Types.Actuator_Types
{
    /// <summary>
    /// A <see cref="Heater"/> placed on the board with a given size, where a <see cref="Droplet"/> can be heated and change <see cref="Droplet.Temperature"/> and <see cref="Droplet.Substance_Name"/>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="sizeX"></param>
    /// <param name="sizeY"></param>
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
