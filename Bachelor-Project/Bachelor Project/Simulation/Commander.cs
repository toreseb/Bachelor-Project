using Bachelor_Project.Simulation.Agent_Actions;
using Bachelor_Project.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    public class Commander
    {

        static private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        (List<Command> commands, Dictionary<string, string> dropletpairs, Dictionary<string, List<string>> contaminated, Dictionary<string, List<string>> contaminates)? data;
        public Board board;
        List<Command> currentCommands = [];

        public Commander((List<Command>, Dictionary<string, string>, Dictionary<string, List<string>>, Dictionary<string, List<string>>)? data, string boarddata)
        {
            this.data = data;
            string json = File.ReadAllText(boarddata);
            board = Board.ImportBoardData(json);
        }

        public void Setup()
        {
            if (!data.HasValue)
            {
                return;
            }
            (List<Command> commands, Dictionary<string, string> dropletpairs, Dictionary<string, List<string>> contaminated, Dictionary<string, List<string>> contaminates) = data.Value;
            foreach (var item in dropletpairs)
            {
                Droplet nDrop = new Droplet(item.Value, item.Key);
                if (contaminated.ContainsKey(item.Value))
                {
                    nDrop.SetContam(contaminated[item.Value]);
                }
                else
                {
                    nDrop.SetContam([]);
                }
                if (!contaminates.ContainsKey(item.Value))
                {
                    contaminates.Add(item.Value, []);
                }
                nDrop.ContamLevel = contaminates[item.Value].Count;
                board.Droplets.Add(item.Key, nDrop);
                nDrop.StartAgent();
            }

            board.Droplets = board.Droplets.OrderBy(x => x.Value.ContamLevel).ToDictionary();
            board.Droplets.Values.ToList().ForEach(x => Console.WriteLine(x.ContamLevel));

            foreach (var item1 in commands)
            {
                foreach (var item2 in commands)
                {
                    if (item1 != item2 && !item1.InputCommands.Contains(item2) && !item2.InputCommands.Contains(item1))
                    {
                        foreach (var item in item1.OutputDroplets)
                        {
                            if (item == "drop6")
                            {
                                int a = 2;
                            }
                            if (item2.InputDroplets.Contains(item))
                            {
                                item2.InputCommands.Add(item1);
                                item1.OutputCommands.Add(item2);
                            }
                        }
                    }
                }
            }
            foreach (var item in commands)
            {
                Console.WriteLine(item.Type + " needs commands: " + item.InputCommands.Count + " and allows: " + item.OutputCommands.Count);
            }
            commands = commands.OrderBy(x => x.InputCommands.Count).ToList();

            commands.FindAll(x => x.InputCommands.Count == 0).ForEach(x => currentCommands.Add(x));
        }

        public Board SetBoard(string boarddata)
        {
            string json = File.ReadAllText(boarddata);
            board = Board.ImportBoardData(json);
            return board;
        }

        public void Start()
        {

            while (currentCommands.Count > 0)
            {
                
                currentCommands.OrderBy(x => x.InputDroplets.Select(y => board.Droplets[y]).Min());

                Command cCommand = currentCommands[0];
                // cCommand.ExecuteCommand();
                cCommand.OutputCommands.ForEach(x => x.InputCommands.Remove(cCommand));
                cCommand.OutputCommands.ForEach(x => 
                    {
                        if (x.InputCommands.Count == 0) {
                            currentCommands.Add(x);
                        }
                    });
                currentCommands.Remove(cCommand);
                
                // board.PrintBoardState();
            }

            // Thread.Sleep(5000);
            // board.PrintBoardState();

            Console.WriteLine("Done");

            //Actuator e = JsonSerializer.Deserialize<Actuator>(json, options);
            //Console.WriteLine(e);


            /*
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 36);
            board.PrintBoardState();
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);
            board.PrintBoardState();
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);
            board.PrintBoardState();
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);
            board.PrintBoardState();
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);
            board.PrintBoardState();
            */
            
            
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"],36);
            board.PrintBoardState();
            /*Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.DOWN);
            board.PrintBoardState();
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.LEFT);
            board.PrintBoardState();
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.UP);
            board.PrintBoardState();
            */
            

        }

        Board GetBoard()
        {
            return board;
        }
    }
}
