using Bachelor_Project.Electrode_Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation.Electrode_Types.Sensor_Types
{
    internal class Heat_Sensor(int x, int y, int sizeX, int sizeY) : Sensor(x, y, sizeX, sizeY)
    {
        override public string Type { get; set; } = "heat";
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
