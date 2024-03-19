﻿using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Electrode_Types.Actuator_Types;
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
    public class Command(string type, List<string> inputs, List<string> outputs, string? nextName = null, Type? nextType = null, params object[] value)
    {

        public string Type { get; set; } = type;
        public List<string> InputDroplets = inputs;
        public List<Command> InputCommands = [];
        public List<string> OutputDroplets = outputs;
        public List<Command> OutputCommands = [];
        public Type? NextType = nextType;
        public string? NextName = nextName;
        public Apparature? CommandDestination = null;
        public object[] ActionValue { get; set; } = value;

        public void SetDest()
        {
            try
            {
                if (NextType == null || NextName == null)
                {
                    return;
                }
                if (NextType.IsSubclassOf(typeof(Actuator)))
                {
                    CommandDestination = Program.C.board.Actuators[NextName];
                }
                else if (NextType.Equals(typeof(Sensor)))
                {
                    CommandDestination = Program.C.board.Sensors[NextName];
                }
                else if (NextType.Equals(typeof(Output)))
                {
                    CommandDestination = Program.C.board.Output[NextName];
                }
                else // Not sure if waste is needed
                {
                    throw new ArgumentException("Invalid destination type");
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid destination target");
            }
            
        }

        public void ExecuteCommand()
        {
            Board b = Program.C.board;
            Task command;
            switch (Type) 
            {
                case "input":
                    
                    Console.WriteLine("Input");
                    command = new(() => Droplet_Actions.InputDroplet(b.Droplets[OutputDroplets[0]], b.Input[(string)ActionValue[0]], int.Parse((string)ActionValue[1]), CommandDestination));
                    b.Droplets[OutputDroplets[0]].GiveWork(command);
                    break;
                case "output":
                    Console.WriteLine("Output");
                    Task outputDroplet = new(() => Droplet_Actions.OutputDroplet(b.Droplets[InputDroplets[0]], b.Output[(string)ActionValue[0]]));
                    b.Droplets[InputDroplets[0]].GiveWork(outputDroplet);
                    break;
                case "waste":
                    Console.WriteLine("Waste");
                    Task wasteDroplet = new(() => Mission_Tasks.WasteDroplet(b.Droplets[InputDroplets[0]]));
                    b.Droplets[InputDroplets[0]].GiveWork(wasteDroplet);
                    break;
                case "merge":
                    Console.WriteLine("Merge");
                    Task mergeDroplet = new(() => Mission_Tasks.MergeDroplets(InputDroplets, b.Droplets[OutputDroplets[0]]));
                    b.Droplets[OutputDroplets[0]].GiveWork(mergeDroplet);
                    foreach (var item in InputDroplets)
                    {
                        Task awaitWork = new(() => Droplet_Actions.AwaitWork(b.Droplets[OutputDroplets[0]]));
                        b.Droplets[item].GiveWork(awaitWork);
                    }
                    break;
                case "split":
                    Console.WriteLine("Split");
                    Task splitDroplet = new(() => Mission_Tasks.SplitDroplet(b.Droplets[InputDroplets[0]], OutputDroplets));
                    b.Droplets[InputDroplets[0]].GiveWork(splitDroplet);
                    foreach (var item in OutputDroplets)
                    {
                        Task awaitWork = new(() => Droplet_Actions.AwaitWork(b.Droplets[InputDroplets[0]]));
                        b.Droplets[item].GiveWork(awaitWork);
                    }
                    break;
                case "mix":
                    Console.WriteLine("Mix");
                    Task mixDroplet = new(() => Mission_Tasks.MixDroplets(b.Droplets[InputDroplets[0]], (string)ActionValue[0], (string)ActionValue[1]));
                    b.Droplets[InputDroplets[0]].GiveWork(mixDroplet);
                    break;
                case "temp":
                    Console.WriteLine("Temp");
                    Task tempDroplet = new(() => Mission_Tasks.TempDroplet(b.Droplets[InputDroplets[0]], int.Parse((string)ActionValue[0]), (string)ActionValue[1]));
                    b.Droplets[InputDroplets[0]].GiveWork(tempDroplet);
                    break;
                case "sense":
                    Console.WriteLine("Sense");
                    Task senseDroplet = new(() => Mission_Tasks.SenseDroplet(b.Droplets[InputDroplets[0]], (string)ActionValue[0]));
                    b.Droplets[InputDroplets[0]].GiveWork(senseDroplet);
                    break;
                default:
                    throw new ArgumentException("Invalid command type");
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

        override public string ToString()
        {
            return Type + " " + string.Join(", ", InputDroplets) + " -> " + string.Join(", ", OutputDroplets) + " extra: "+ string.Join(", ", ActionValue);
        }

        internal Apparature? FindDest()
        {
            foreach (Command item in OutputCommands)
            {
                if (item.CommandDestination != null)
                {
                    return item.CommandDestination;
                }
                else
                {
                    item.CommandDestination = item.FindDest();
                    if (item.CommandDestination != null)
                    {
                        return item.CommandDestination;
                    }
                }
            }
            return null;
        }
    }

}
    