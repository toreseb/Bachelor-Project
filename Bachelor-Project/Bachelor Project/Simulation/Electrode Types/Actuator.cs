using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Simulation;

namespace Bachelor_Project
{
    /// <summary>
    /// The superclass of all <see cref="Actuator"/>s on the <see cref="Board"/>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="sizeX"></param>
    /// <param name="sizeY"></param>
    /// <param name="name"></param>
    public abstract class Actuator(int x, int y, int sizeX, int sizeY, string name = "") : Apparatus(x, y, sizeX, sizeY, name)
    {
        public int ActuatorID { get; set; }
        public bool Status { get; set; }
        public bool NextStatus { get; set; }

        public override bool CoilInto { get; set; } = true;
    }
}
