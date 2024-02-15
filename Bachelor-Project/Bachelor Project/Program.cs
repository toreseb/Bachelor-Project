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
using System.Text.Json;

class Program
{
    static private readonly JsonSerializerOptions options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };
    static void Main(string[] args)
    {
        string inputfiles = "C:\\GitHub\\Bachelor-Project\\Bachelor-Project\\Bachelor Project\\Input Files";
        
        string programcode = "Program.txt";
        string boarddata = "BoardData.json";
        string testdata = "test.json";

        Parser.ParseFile(inputfiles+"\\"+programcode);

        

        string json = File.ReadAllText(inputfiles+"\\"+boarddata);
        Board B = Board.ImportBoardData(json);
        Droplet_Actions.Board = B;

        //Actuator e = JsonSerializer.Deserialize<Actuator>(json, options);
        //Console.WriteLine(e);

        B.Droplets = [.. B.Droplets, new Droplet(B.Input[0], 16, "Water", "WaterDroplet1")];
        B.PrintBoardState();
        Console.WriteLine();
        Droplet_Actions.MoveDroplet(B.Droplets[0], Direction.DOWN);
        B.PrintBoardState();
        Console.WriteLine();
        Droplet_Actions.MoveDroplet(B.Droplets[0], Direction.UP);
        B.PrintBoardState();
        Console.WriteLine();
        Droplet_Actions.MoveDroplet(B.Droplets[0], Direction.RIGHT);
        B.PrintBoardState();
        Console.WriteLine();
        Droplet_Actions.MoveDroplet(B.Droplets[0], Direction.DOWN);
        B.PrintBoardState();
        Console.WriteLine();


    }

    

}

