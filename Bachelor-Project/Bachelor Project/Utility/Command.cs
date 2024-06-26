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
        public string? NextName = nextName;
        public Apparatus? CommandDestination = null;
        public object[] ActionValue { get; set; } = value;

        private static int Inputted = 0;
        private static UsefulSemaphore InputSem = new UsefulSemaphore(1,1);

        /// <summary>
        /// Sets the desination this <see cref="Command"/>s <see cref="OutputDroplets"/> want to go.
        /// </summary>
        public void SetDest()
        {
            if (Type == "temp")
            {
                CommandDestination = Program.C.board.Actuators[NextName];
            }
            else if (Type == "sense")
            {
                CommandDestination = Program.C.board.Sensors[NextName];
            }
            else if (Type == "output")
            {
                CommandDestination = Program.C.board.Output[NextName];
            }
            else // Not sure if waste is needed
            {
                return;
            }
        }

        /// <summary>
        /// Assigns this <see cref="Command"/> to the <see cref="Droplet"/>s in <see cref="InputDroplets"/> and <see cref="OutputDroplets"/> as <see cref="Mission_Tasks"/>. 
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void ExecuteCommand()
        {
            Board b = Program.C.board;
            Task command;

            switch (Type) 
            {
                case "input":
                    
                    Printer.PrintLine("Input");
                    while (Inputted != (int)ActionValue[0])
                    {
                        InputSem.CheckOne();
                    }
                    InputSem.WaitOne();
                    Inputted += 1;
                    command = new(() => Mission_Tasks.InputDroplet(b.Droplets[OutputDroplets[0]], b.Input[(string)ActionValue[1]], int.Parse((string)ActionValue[2]), InputSem, CommandDestination));
                    b.Droplets[OutputDroplets[0]].GiveWork(command);
                    break;
                case "output":
                    Printer.PrintLine("OutputDroplet");
                    Task outputDroplet = new(() => Mission_Tasks.OutputDroplet(b.Droplets[InputDroplets[0]], b.Output[NextName]));
                    b.Droplets[InputDroplets[0]].GiveWork(outputDroplet);
                    break;
                case "merge":
                    Printer.PrintLine("Merge");
                    UsefulSemaphore sem0 = new(InputDroplets.Count); // For the merging droplet to tell each other that they are ready
                    UsefulSemaphore sem1 = new(InputDroplets.Count); // For the merged droplet to tell the merging droplets that the location is calculated
                    Task<Electrode> calcMerge = new(() => Droplet_Actions.MergeCalc(InputDroplets, b.Droplets[OutputDroplets[0]], sem1));

                    UsefulSemaphore sem2 = new(InputDroplets.Count); // For the merging droplets to tell the merged droplet they have finished.
                    
                    foreach (var item in InputDroplets)
                    {
                        Task awaitWork = new(() => Mission_Tasks.AwaitMergeWork(b.Droplets[item], calcMerge, sem0, sem1, sem2, InputDroplets));
                        b.Droplets[item].GiveWork(awaitWork);
                    }
                    Task mergeDroplet = new(() => Mission_Tasks.MergeDroplets(InputDroplets, b.Droplets[OutputDroplets[0]], calcMerge, sem0, sem2, CommandDestination));

                    b.Droplets[OutputDroplets[0]].GiveWork(mergeDroplet);


                    break;
                case "split":
                    Printer.PrintLine("Split");
                    Dictionary<string, double> percentages = [];


                    percentages = Calc.FindPercentages(ActionValue.Length > 0 ? (Dictionary<string, int>)ActionValue[0] : null, OutputDroplets);

                    Dictionary<string, UsefulSemaphore> dropSem = new Dictionary<string, UsefulSemaphore>();

                    foreach (string dName in OutputDroplets)
                    {
                        UsefulSemaphore sem = new UsefulSemaphore(0, 2);
                        dropSem.Add(dName, sem);
                    }

                    Task splitDroplet = new(() => Mission_Tasks.SplitDroplet(b.Droplets[InputDroplets[0]], percentages, dropSem));
                    b.Droplets[InputDroplets[0]].GiveWork(splitDroplet);
                    foreach (var item in OutputDroplets)
                    {
                        Task awaitSplitWork = new(() => Mission_Tasks.AwaitSplitWork(b.Droplets[item], CommandDestination, dropSem[item]));
                        b.Droplets[item].GiveWork(awaitSplitWork);
                    }
                    break;
                case "mix":
                    Printer.PrintLine("Mix");
                    Task mixDroplet = new(() => Mission_Tasks.MixDroplet(b.Droplets[InputDroplets[0]], (string)ActionValue[0], (string)ActionValue[1]));
                    b.Droplets[InputDroplets[0]].GiveWork(mixDroplet);
                    break;
                case "temp":
                    Printer.PrintLine("Temp");
                    Task tempDroplet = new(() => Mission_Tasks.TempDroplet(b.Droplets[InputDroplets[0]], (Heater)b.Actuators[NextName], int.Parse((string)ActionValue[1]), newType: (string)ActionValue[0]));
                    b.Droplets[InputDroplets[0]].GiveWork(tempDroplet);
                    break;
                case "sense":
                    Printer.PrintLine("Sense");
                    Task senseDroplet = new(() => Mission_Tasks.SenseDroplet(b.Droplets[InputDroplets[0]], b.Sensors[NextName]));
                    b.Droplets[InputDroplets[0]].GiveWork(senseDroplet);
                    break;
                case "wait":
                    Printer.PrintLine("Wait");
                    Task waitDroplet = new(() => Mission_Tasks.WaitDroplet(b.Droplets[InputDroplets[0]], (int)ActionValue[0]));
                    b.Droplets[InputDroplets[0]].GiveWork(waitDroplet);
                    break;
                default:
                    throw new ArgumentException("Invalid command type");
            }
            Printer.Print("[ ");
            foreach (var item in InputDroplets)
            {
                Printer.Print(item + ", ");
            }
            Printer.Print("] -> [ ");
            foreach (var item in OutputDroplets)
            {
                Printer.Print(item + ", ");
            }
            Printer.PrintLine("]");


        }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns></returns>
        override public string ToString()
        {
            return Type + " " + string.Join(", ", InputDroplets) + " -> " + string.Join(", ", OutputDroplets) + " extra: "+ string.Join(", ", ActionValue);
        }

        /// <summary>
        /// Used by the <see cref="Commander"/> to traverse the <see cref="Command"/> tree and find a destination if none is set.
        /// </summary>
        /// <returns>The found destination as a <see cref="Apparatus"/></returns>
        internal Apparatus? FindDest()
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

        /// <summary>
        /// Resets the static command input number, used by tests to reset the environment.
        /// </summary>
        public static void Reset()
        {
            Inputted = 0;
        }
    }

    

}
    