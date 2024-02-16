

namespace Bachelor_Project.Outparser
{
    static class Outparser
    {
        public static void ElectrodeOn(Simulation.Electrode e)
        {
            e.Status = 1;
            Console.WriteLine("setel " + e.DriverID + " " + e.ElectrodeID + " \\r");
        }

        public static void ElectrodeOff(Simulation.Electrode e) 
        {
            e.Status = 0;
            Console.WriteLine("clrel " + e.DriverID + " " + e.ElectrodeID + " \\r");
        }

    }
}
