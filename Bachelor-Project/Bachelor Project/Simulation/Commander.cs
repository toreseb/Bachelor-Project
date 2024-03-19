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
            foreach (var dropletpair in dropletpairs)
            {
                Droplet nDrop = new Droplet(dropletpair.Value, dropletpair.Key);
                if (contaminated.ContainsKey(dropletpair.Value))
                {
                    nDrop.SetContam(contaminated[dropletpair.Value]);
                }
                else
                {
                    nDrop.SetContam([]);
                }
                if (!contaminates.ContainsKey(dropletpair.Value))
                {
                    contaminates.Add(dropletpair.Value, []);
                }
                nDrop.ContamLevel = contaminates[dropletpair.Value].Count;
                Console.WriteLine(nDrop.Substance_Name +" is contaminated by");
                board.Droplets.Add(dropletpair.Key, nDrop);
                nDrop.StartAgent();
            }

            board.Droplets = board.Droplets.OrderBy(x => x.Value.ContamLevel).ToDictionary();
            board.Droplets.Values.ToList().ForEach(x => Console.WriteLine(x.ContamLevel));

            foreach (var item in commands)
            {
                item.SetDest();
            }

            foreach (var item1 in commands)
            {
                
                
                foreach (var item2 in commands)
                {
                    if (item1 != item2 && !item1.InputCommands.Contains(item2) && !item2.InputCommands.Contains(item1))
                    {
                        foreach (var item in item1.OutputDroplets)
                        {
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
                Console.WriteLine(item.Type + " needs commands: " + item.InputCommands.Count + " and allows: " + item.OutputCommands.Count + " and has nextdest of " + item.CommandDestination);
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
                cCommand.CommandDestination ??= cCommand.FindDest();
                Console.WriteLine(cCommand.ToString() + cCommand.CommandDestination);
                cCommand.ExecuteCommand();
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

            //Console.WriteLine("Done");

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

            /*
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"],36);
            board.PrintBoardState();
            Console.WriteLine("Uncoil start");
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[3,1]);
            board.PrintBoardState();
            Console.WriteLine("Uncoil done");
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);
            board.PrintBoardState();
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);
            board.PrintBoardState();
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);
            board.PrintBoardState();
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);
            board.PrintBoardState();
            Droplet_Actions.Mix(board.Droplets["Wat1"]);
            */

            /*
            Droplet Wat1 = new Droplet("Water", "Wat1");
            Wat1.Contamintants.Add("Blood");
            board.Droplets.Add(Wat1.Name,Wat1);
            
            board.Electrodes[5, 3].Contaminate("Blood");
            board.Electrodes[5, 4].Contaminate("Blood");
            board.Electrodes[5, 2].Contaminate("Blood");
            board.Electrodes[5, 5].Contaminate("Blood");
            board.Electrodes[5, 6].Contaminate("Blood");
            board.Electrodes[5, 7].Contaminate("Water");
            Droplet_Actions.InputDroplet(Wat1, board.Input["in0"], 290);
            Program.C.board.PrintBoardState();
            //board.Electrodes[4, 5].Contaminate("water");
            //board.Electrodes[5, 5].Contaminate("water");
            //board.Electrodes[6, 5].Contaminate("water");
            //board.Electrodes[3, 4].Contaminate("water");
            //roplet_Actions.UncoilSnek(Wat1, board.Output["out0"].pointers[0]);
            //Droplet_Actions.MoveToDest(Wat1, board.Output["out0"].pointers[0]);
            */


            // Test of uncoil with dest og algorithm
            /*
            Droplet Wat1 = new Droplet("Water", "Wat1");
            Wat1.Contamintants.Add("Blood");
            board.Droplets.Add(Wat1.Name, Wat1);
            Droplet_Actions.InputDroplet(Wat1, board.Input["in0"], 60, board.Output["out0"]);
            */
            //Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[7,2]);




            //board.PrintBoardState();

            Thread.Sleep(100000000); //TODO: make it sleep until all commands have been executed

        }

        Board GetBoard()
        {
            return board;
        }
    }
}
