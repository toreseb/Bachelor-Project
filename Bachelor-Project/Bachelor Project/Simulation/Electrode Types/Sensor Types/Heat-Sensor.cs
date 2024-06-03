using Bachelor_Project.Simulation;
using Bachelor_Project.Electrode_Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation.Electrode_Types.Sensor_Types
{
    /// <summary>
    /// A <see cref="HeatSensor"/> on the <see cref="Board"/> that can be used to sense the <see cref="Droplet.Temperature"/> of a <see cref="Droplet"/>.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="sizeX"></param>
    /// <param name="sizeY"></param>
    internal class HeatSensor(int x, int y, int sizeX, int sizeY) : Sensor(x, y, sizeX, sizeY)
    {
        override public string Type { get; set; } = "heat";
        /// <summary>
        /// The <see cref="HeatSensor"/>s specific method to sense <see cref="Droplet.Temperature"/>
        /// </summary>
        /// <returns>The <see cref="Droplet.Temperature"/> of the current <see cref="Droplet"/> on the <see cref="Sensor"/></returns>
        public override object[]? SenseFunc() // Returns [Color]
        {
            if (pointers[0].Occupant != null)
            {
                return [pointers[0].Occupant.Temperature];
            }
            return null;
        }
    }
}
