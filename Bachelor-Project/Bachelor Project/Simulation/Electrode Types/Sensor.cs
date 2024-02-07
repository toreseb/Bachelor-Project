using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Simulation;

namespace Bachelor_Project.Electrode_Types
{
    
    internal class Sensor(int x, int y, string name = "") : Electrode(x, y, name)
    {
        readonly int SensorID;

        protected override void GetIDs()
        {
            base.GetIDs();
            IDs = IDs.Append(SensorID).ToArray();
        }

        protected override void GenerateID(params int[] values)
        {
            base.GenerateID(values);

            values[3] = 5; //SensorID
        }
    }

    


}
