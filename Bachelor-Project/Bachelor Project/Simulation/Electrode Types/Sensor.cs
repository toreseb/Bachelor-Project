using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Simulation;

namespace Bachelor_Project.Electrode_Types
{
    
    internal class Sensor(int x, int y, int sizeX, int sizeY, string name = "") : Apparature(x, y, sizeX, sizeY, name)
    {
        readonly int SensorID;

        protected override void GetIDs()
        {
            base.GetIDs();
            IDs = [.. IDs, SensorID];
        }

        protected override void GenerateID(params int[] values)
        {
            base.GenerateID(values);

            values[3] = 5; //SensorID
        }
    }

    


}
