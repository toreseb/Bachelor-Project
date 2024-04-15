

using Bachelor_Project.Utility;

namespace Bachelor_Project.Outparser
{
    static public class Outparser
    {
        public static void ElectrodeOn(Simulation.Electrode e)
        {
            e.Status = 1;
            Printer.PrintLine("setel " + e.DriverID + " " + e.ElectrodeID + " \\r");
        }

        public static void ElectrodeOff(Simulation.Electrode e) 
        {
            e.Status = 0;
            Printer.PrintLine("clrel " + e.DriverID + " " + e.ElectrodeID + " \\r");
        }

    }
}
