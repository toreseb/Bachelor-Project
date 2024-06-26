using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Utility
{
    /// <summary>
    /// Contains settings relavent to how the program runs. These are more input out focused that <see cref="Constants"/>.
    /// </summary>
    public static class Settings
    {
        public const bool ConnectedToHardware = false;
        public static bool Printing = true;
        public static bool Outputting = true;

        public const int TimeStep = 100; //100 milliseconds for each time step in the simulation
        public const int TimeStepOnSingleTick = 1000;

        public const string BoardName = "DilutionSeriesBoardData";
        public const string BoardFileLoc = "Input Files";        
        public const string BoardFile = BoardFileLoc + "\\" + BoardName + ".json";

        public const string ProtocolName = "DilutionSeries";
        public const string ProtocolFileLoc = "Input Files";
        public const string ProtocolFile = ProtocolFileLoc + "\\" + ProtocolName + ".txt";

    }
}
