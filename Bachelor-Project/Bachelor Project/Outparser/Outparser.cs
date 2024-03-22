

using Bachelor_Project.Utility;

namespace Bachelor_Project.Outparser
{
    static public class Outparser
    {
        public static void ElectrodeOn(Simulation.Electrode e)
        {
            e.Status = 1;
            Printer.Print("setel " + e.DriverID + " " + e.ElectrodeID + " \\r");
        }

        public static void ElectrodeOff(Simulation.Electrode e) 
        {
            e.Status = 0;
            Printer.Print("clrel " + e.DriverID + " " + e.ElectrodeID + " \\r");
        }

    }
}
