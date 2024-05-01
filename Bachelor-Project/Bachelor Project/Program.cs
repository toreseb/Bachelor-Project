// See https://aka.ms/new-console-template for more information

using Bachelor_Project;
using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Electrode_Types.Actuator_Types;
using Bachelor_Project.Outparser;
using Bachelor_Project.Parsing;

using Bachelor_Project.Simulation;
using Bachelor_Project.Simulation.Agent_Actions;
using Bachelor_Project.Utility;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.Json;

static public class Program
{

    public static Commander C;

    static void Main(string[] args)
    {
        Printer.PrintLine("Starting Program");
        string dir = Directory.GetCurrentDirectory() + "\\..\\..\\..";
        Printer.PrintLine(dir);
        


        var data = Parsing.ParseFile(dir + "\\" + Settings.ProtocolFile);
        C = new Commander(data, dir + "\\" + Settings.BoardFile);
        C.Setup();
        C.Start();

    }

    

}

