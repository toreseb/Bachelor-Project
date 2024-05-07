using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Simulation;
using Bachelor_Project.Utility;

namespace Bachelor_Project.Electrode_Types
{
    
    public abstract class Sensor(int x, int y, int sizeX, int sizeY, string name = "") : Apparature(x, y, sizeX, sizeY, name)
    {
        public int SensorID{ get; set; }

        abstract public string Type { get; set; }

        public override bool CoilInto { get; set; } = false;

        public object[]? value;

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
