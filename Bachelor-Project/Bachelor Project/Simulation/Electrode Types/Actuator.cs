using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Simulation;

namespace Bachelor_Project
{
    internal class Actuator(int x, int y, string name = "") : Electrode(x, y, name)
    {
        readonly int ActuatorID;

        protected override void GetIDs()
        {
            base.GetIDs();
            IDs = IDs.Append(ActuatorID).ToArray();
        }

        protected override void GenerateID(params int[] values)
        {
            base.GenerateID(values);

            values[3] = 5; //ActuatorID
        }
    }
}
