using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Electrode_Types.Sensor_Types
{
    /// <summary>
    /// This was a mistake, and is not actually part of the program. Depricated.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="sizeX"></param>
    /// <param name="sizeY"></param>
    internal class SizeSensor(int x, int y, int sizeX, int sizeY) : Sensor(x, y, sizeX, sizeY)
    {

        override public string Type { get; set; } = "size";
        public override object[]? SenseFunc() // Returns [Volume, Size]
        {
            if (pointers[0].Occupant != null)
            {
                return [pointers[0].Occupant.Volume, pointers[0].Occupant.Size];
            }
            return null;
        }
    }
}
