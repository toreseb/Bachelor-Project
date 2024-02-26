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

        public ArrayList[] commands;
        public Board board;

        public Commander(ArrayList[] commands, string boarddata)
        {
            this.commands = commands;
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
            Console.WriteLine();*/

            board.Droplets = [.. board.Droplets, new Droplet(board.Input[0], 24, "Water", "Wat1")];
            board.PrintBoardState();
            Console.WriteLine();
            Droplet_Actions.SnekMove(board.Droplets[0], Direction.DOWN);
            board.PrintBoardState();
            Console.WriteLine();
            Droplet_Actions.SnekMove(board.Droplets[0], Direction.RIGHT);
            board.PrintBoardState();
            Console.WriteLine();
            Droplet_Actions.SnekMove(board.Droplets[0], Direction.RIGHT);
            board.PrintBoardState();
            Console.WriteLine();
            Droplet_Actions.SnekMove(board.Droplets[0], Direction.UP);
            board.PrintBoardState();
            Console.WriteLine();
            Droplet_Actions.SnekMove(board.Droplets[0], Direction.DOWN);
            board.PrintBoardState();
            Console.WriteLine();
            Droplet_Actions.SnekReversal(board.Droplets[0].Occupy);
            board.PrintBoardState();
            Console.WriteLine();
            Droplet_Actions.SnekMove(board.Droplets[0], Direction.LEFT);
            board.PrintBoardState();
            Console.WriteLine();
        }

        Board GetBoard()
        {
            return board;
        }
    }
}
