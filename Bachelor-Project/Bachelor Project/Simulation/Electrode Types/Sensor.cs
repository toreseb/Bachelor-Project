using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Simulation;
using Bachelor_Project.Utility;

namespace Bachelor_Project.Electrode_Types
{
    /// <summary>
    /// The superclass of all <see cref="Sensor"/>s on the <see cref="Board"/>.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="sizeX"></param>
    /// <param name="sizeY"></param>
    /// <param name="name"></param>
    public abstract class Sensor(int x, int y, int sizeX, int sizeY, string name = "") : Apparatus(x, y, sizeX, sizeY, name)
    {
        public int SensorID{ get; set; }

        

        public override bool CoilInto { get; set; } = false;

        public object[]? value;

        /// <summary>
        /// Sense func to be used if data needs to be retrieved from the physical hardware.
        /// </summary>
        /// <returns>A <see cref="Object[]"/> value that represents the unique parameters of each <see cref="Sensor"/></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object[]? Sense()
        {
            if (Settings.ConnectedToHardware)
            {
                //TODO: Somehow gain data from the Hardware
                throw new NotImplementedException();
            }
            else
            {
                value = SenseFunc();
                return value;

            }
        }

        public abstract object[]? SenseFunc();

    }

    


}
