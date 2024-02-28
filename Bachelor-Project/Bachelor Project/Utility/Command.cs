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
                    Task inputDroplet = new(() => Droplet_Actions.InputDroplet(b.Droplets[OutputDroplets[0]], b.Input[(string)ActionValue[0]], int.Parse((string)ActionValue[1])));
                    b.Droplets[OutputDroplets[0]].GiveWork(inputDroplet);
                    break;
                case "output":
                    Console.WriteLine("Output");
                    Task outputDroplet = new(() => Droplet_Actions.OutputDroplet(b.Droplets[InputDroplets[0]], b.Output[(string)ActionValue[0]]));
                    b.Droplets[OutputDroplets[0]].GiveWork(outputDroplet);
                    break;
                case "waste":
                    Console.WriteLine("Waste");
                    Task wasteDroplet = new(() => Droplet_Actions.WasteDroplet(b.Droplets[InputDroplets[0]]));
                    b.Droplets[InputDroplets[0]].GiveWork(wasteDroplet);
                    break;
                case "merge":
                    Console.WriteLine("Merge");
                    Task mergeDroplet = new(() => Droplet_Actions.MergeDroplets(InputDroplets, b.Droplets[OutputDroplets[0]]));
                    b.Droplets[OutputDroplets[0]].GiveWork(mergeDroplet);
                    break;
                case "split":
                    Console.WriteLine("Split");
                    Task splitDroplet = new(() => Droplet_Actions.SplitDroplet(b.Droplets[InputDroplets[0]], OutputDroplets));
                    foreach (var item in OutputDroplets)
                    b.Droplets[InputDroplets[0]].GiveWork(splitDroplet);
                    break;
                case "mix":
                    Console.WriteLine("Mix");
                    Task mixDroplet = new(() => Droplet_Actions.MixDroplets(b.Droplets[InputDroplets[0]], b.Droplets[OutputDroplets[0]], (string)ActionValue[0], (string)ActionValue[1]));
                    b.Droplets[InputDroplets[0]].GiveWork(mixDroplet);
                    break;
                case "temp":
                    Console.WriteLine("Temp");
                    Task tempDroplet = new(() => Droplet_Actions.TempDroplet(b.Droplets[InputDroplets[0]], b.Droplets[OutputDroplets[0]], int.Parse((string)ActionValue[0]), (string)ActionValue[1]));
                    b.Droplets[InputDroplets[0]].GiveWork(tempDroplet);
                    break;
                case "sense":
                    Console.WriteLine("Sense");
                    Task senseDroplet = new(() => Droplet_Actions.SenseDroplet(b.Droplets[InputDroplets[0]], b.Droplets[OutputDroplets[0]], (string)ActionValue[0]));
                    b.Droplets[InputDroplets[0]].GiveWork(senseDroplet);
                    break;
            }
            Console.Write("[ ");
            foreach (var item in InputDroplets)
            {
                Console.Write(item + ", ");
            }
            Console.Write("] -> [ ");
            foreach (var item in OutputDroplets)
            {
                Console.Write(item + ", ");
            }
            Console.WriteLine("]");


        }
        

    }

}
    