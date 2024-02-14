// See https://aka.ms/new-console-template for more information

using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Parser;
using Bachelor_Project.Simulation;
using System.Collections;
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

        Board e = JsonSerializer.Deserialize<Board>(json, options);
        e.Electrodes[12].Contaminate("A");
        e.PrintBoardState();


    }
}

