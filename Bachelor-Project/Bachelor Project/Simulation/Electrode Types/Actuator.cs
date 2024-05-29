using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Simulation;

namespace Bachelor_Project
{
    public abstract class Actuator(int x, int y, int sizeX, int sizeY, string name = "") : Apparature(x, y, sizeX, sizeY, name)
    {
        public int ActuatorID { get; set; }
        public bool Status { get; set; }
        public bool NextStatus { get; set; }

        public override bool CoilInto { get; set; } = true;
    }
}
