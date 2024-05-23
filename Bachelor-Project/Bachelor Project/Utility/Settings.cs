using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Utility
{
    public static class Settings
    {
        public const bool ConnectedToHardware = false;
        public const bool Printing = true;
        public static bool Outputting = true;

        public const string BoardName = "BoardData";
        public const string BoardFileLoc = "Input Files";        
        public const string BoardFile = BoardFileLoc + "\\" + BoardName + ".json";

        public const string ProtocolName = "splitTestProgram";
        public const string ProtocolFileLoc = "Input Files";
        public const string ProtocolFile = ProtocolFileLoc + "\\" + ProtocolName + ".txt";

    }
}
