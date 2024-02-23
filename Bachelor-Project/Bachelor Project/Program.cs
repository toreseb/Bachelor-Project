// See https://aka.ms/new-console-template for more information

using Bachelor_Project;
using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Electrode_Types.Actuator_Types;
using Bachelor_Project.Parser;

using Bachelor_Project.Simulation;
using Bachelor_Project.Simulation.Agent_Actions;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.Json;

static class Program
{

    public static Commander C;

    static void Main(string[] args)
    {

        string inputfiles = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Input Files";
        Console.WriteLine(inputfiles);
        


        string programcode = "Program.txt";
        string testprogramcode = "testProgram.txt";
        string boarddata = "BoardData.json";
        string testdata = "test.json";

        ArrayList[] Commands = Parser.ParseFile(inputfiles + "\\" + testprogramcode);
        C = new Commander(Commands, inputfiles + "\\" + boarddata);
        C.Start();

    }

    

}

