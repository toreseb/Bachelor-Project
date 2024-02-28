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
    internal class Commander
    {

        static private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        (List<Command> commands, Dictionary<String,String> dropletpairs, Dictionary<String,List<String>> contaminated, Dictionary<String,List<String>> contaminates) data;
        public Board board;
        List<Command> currentCommands = [];

        public Commander((List<Command> commands, Dictionary<String,String> dropletpairs, Dictionary<String,List<String>> contaminated, Dictionary<String,List<String>> contaminates) data, string boarddata)
        {
            this.data = data;
            string json = File.ReadAllText(boarddata);
            board = Board.ImportBoardData(json);
        }

        public void Start()
        {

            foreach (var item in data.dropletpairs)
            {
                Droplet nDrop = new Droplet(item.Value, item.Key);
                if (data.contaminated.ContainsKey(item.Value)){
                    nDrop.SetContam(data.contaminated[item.Value]);
                }
                else
                {
                    nDrop.SetContam([]);
                }
                if (!data.contaminates.ContainsKey(item.Value))
                {
                    data.contaminates.Add(item.Value, []);
                }
                nDrop.ContamLevel = data.contaminates[item.Value].Count;
                board.Droplets.Add(item.Key, nDrop);
                nDrop.StartAgent();
            }
            
            board.Droplets = board.Droplets.OrderBy(x => x.Value.ContamLevel).ToDictionary();
            board.Droplets.Values.ToList().ForEach(x => Console.WriteLine(x.ContamLevel));

            foreach (var item1 in data.commands)
            {
                foreach (var item2 in data.commands)
                {
                    if(item1 != item2 && !item1.InputCommands.Contains(item2) && !item2.InputCommands.Contains(item1))
                    {
                        foreach (var item in item1.OutputDroplets)
                        {
                            if (item == "drop6")
                            {
                                int a = 2;
                            }
                            if(item2.InputDroplets.Contains(item))
                            {
                                item2.InputCommands.Add(item1);
                                item1.OutputCommands.Add(item2);
                            }
                        }
                    }
                }
            }
            foreach (var item in data.commands)
            {
                Console.WriteLine(item.Type + " needs commands: " +item.InputCommands.Count + " and allows: " + item.OutputCommands.Count);
            }
            data.commands = data.commands.OrderBy(x => x.InputCommands.Count).ToList();

            data.commands.FindAll(x => x.InputCommands.Count == 0).ForEach(x => currentCommands.Add(x));

            while (currentCommands.Count > 0)
            {
                
                currentCommands.OrderBy(x => x.InputDroplets.Select(y => board.Droplets[y]).Min());

                Command cCommand = currentCommands[0];
                cCommand.ExecuteCommand();
                cCommand.OutputCommands.ForEach(x => x.InputCommands.Remove(cCommand));
                cCommand.OutputCommands.ForEach(x => 
                    {
                        if (x.InputCommands.Count == 0) {
                            currentCommands.Add(x);
                        }
                    });
                currentCommands.Remove(cCommand);
                
                board.PrintBoardState();
            }

            Thread.Sleep(5000);
            board.PrintBoardState();

            Console.WriteLine("Done");

            //Actuator e = JsonSerializer.Deserialize<Actuator>(json, options);
            //Console.WriteLine(e);


            /*
            board.Droplets.Add("Wat1", new Droplet(board.Input["in0"], 16, "Water", "Wat1"));
            board.PrintBoardState();
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.DOWN);
            board.PrintBoardState();
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.UP);
            board.PrintBoardState();
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);
            board.PrintBoardState();
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.DOWN);
            board.PrintBoardState();

            /*
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"],24);
            board.PrintBoardState();
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.DOWN);
            board.PrintBoardState();
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);
            board.PrintBoardState();
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);
            board.PrintBoardState();
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.UP);
            board.PrintBoardState();
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.DOWN);
            board.PrintBoardState();
            Droplet_Actions.SnekReversal(board.Droplets["Wat1"]);
            board.PrintBoardState();
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.LEFT);
            board.PrintBoardState();
            */
        }

        Board GetBoard()
        {
            return board;
        }
    }
}
