using Bachelor_Project.Simulation;
using Bachelor_Project.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Electrode_Types.Sensor_Types
{
    internal class RGBSensor(int x, int y, int sizeX, int sizeY) : Sensor(x, y, sizeX, sizeY)
    {

        override public string Type { get; set; } = "RGB_color";
        public override object[]? SenseFunc() // Returns [Color]
        {
            if (pointers[0].Occupant != null)
            {
                return [pointers[0].Occupant.Color];
            }
            return null;
        }
    }
}
