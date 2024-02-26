using Bachelor_Project.Simulation.Agent_Actions;
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

        (ArrayList[] commands, List<String> dropletnames, List<String> droplettypes) data;
        public Board board;

        public Commander((ArrayList[] commands, List<String> dropletnames, List<String> droplettypes) data, string boarddata)
        {
            this.data = data;
            string json = File.ReadAllText(boarddata);
            board = Board.ImportBoardData(json);
        }

        public void Start()
        {

            //Actuator e = JsonSerializer.Deserialize<Actuator>(json, options);
            //Console.WriteLine(e);



            board.Droplets.Add("Wat1", new Droplet(board.Input["in0"], 16, "Water", "Wat1"));
            board.PrintBoardState();
            Console.WriteLine();
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.DOWN);
            board.PrintBoardState();
            Console.WriteLine();
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.UP);
            board.PrintBoardState();
            Console.WriteLine();
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);
            board.PrintBoardState();
            Console.WriteLine();
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.DOWN);
            board.PrintBoardState();
            Console.WriteLine();
            
        }

        Board GetBoard()
        {
            return board;
        }
    }
}
