using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Simulation;
using Bachelor_Project.Simulation.Agent_Actions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Utility
{
    internal class Command(string type, List<string> inputs, List<string> outputs, params Object[] value)
    {
        public string Type { get; set; } = type;
        public List<string> InputDroplets = inputs;
        public List<Command> InputCommands = [];
        public List<string> OutputDroplets = outputs;
        public List<Command> OutputCommands = [];
        public Object[] ActionValue { get; set; } = value;

        public void ExecuteCommand()
        {
            Board b = Program.C.board;
            switch (Type) 
            {
                case "input":
                    Console.WriteLine("Input");
                    Task inputDroplet = new Task(() => Droplet_Actions.InputDroplet(b.Droplets[OutputDroplets[0]], b.Input[(string)ActionValue[0]], int.Parse((string)ActionValue[1])));
                    b.Droplets[OutputDroplets[0]].GiveWork(inputDroplet);
                    break;
                case "output":
                    Console.WriteLine("Output");
                    // Output droplets
                    break;
                case "waste":
                    Console.WriteLine("Waste");
                    // Waste droplets
                    break;
                case "merge":
                    Console.WriteLine("Merge");
                    // Merge droplets
                    break;
                case "split":
                    Console.WriteLine("Split");
                    // Split droplets
                    break;
                case "mix":
                    Console.WriteLine("Mix");
                    // Mix droplets
                    break;
                case "temp":
                    Console.WriteLine("Temp");
                    // Temperature droplets
                    break;
                case "sense":
                    Console.WriteLine("Sense");
                    // Sense droplets
                    break;
            }
        }

    }

}
    