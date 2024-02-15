using static Bachelor_Project.Simulation.Electrode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Outparser
{
    static class Outpasser
    {
        public static void ElectrodeOn(Simulation.Electrode e)
        {
            Console.WriteLine("setel " + e.DriverID + " " + e.ElectrodeID + " \\r");
        }

        public static void ElectrodeOff(Simulation.Electrode e) 
        {
            Console.WriteLine("clrel " + e.DriverID + " " + e.ElectrodeID + " \\r");
        }

    }
}
