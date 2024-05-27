using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Electrode_Types.Actuator_Types;
using Bachelor_Project.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation.Agent_Actions
{
    // This class contains the more complicated missions the agents will have.
    public static class Mission_Tasks
    {
        public static bool InputDroplet(Droplet d, Input i, int volume, UsefulSemaphore? InputSem = null, Apparature? destination = null)
        {
            Printer.PrintLine(d.Name + " has NextDestiantion of: " + d.nextDestination);
            Printer.PrintLine(d.Name + " : INPUTTING");
            Droplet_Actions.InputDroplet(d, i, volume, destination);
            InputSem?.TryReleaseOne();
            if (destination != null && d.GetWork().Count == 0)
            {
                Electrode destElectrode = d.GetClosestFreePointer(destination);
                d.CurrentPath = ModifiedAStar.FindPath(d, destElectrode);
                while (d.CurrentPath.Value.path.Count > Constants.DestBuff)
                {
                    d.Waiting = true;
                    try
                    {
                        Droplet_Actions.MoveTowardDest(d, destElectrode);
                    }
                    catch (NewWorkException)
                    {
                        Printer.PrintBoard();
                        d.Waiting = false;
                        d.MergeReady = true;
                        return false;
                    }

                    if (d.CurrentPath.Value.path.Count <= Constants.DestBuff)
                    {
                        Droplet_Actions.CoilSnek(d, d.SnekList.First());
                        Program.C.RemovePath(d);
                        return true;
                    }
                }
            }
            return true;

        }
        public static bool OutputDroplet(Droplet droplet, Output output)
        {
            Printer.PrintLine(droplet.Name + " : OUTPUTTING");
            Printer.PrintBoard();
            droplet.Important = true;
            Droplet_Actions.MoveToApparature(droplet, output);
            Droplet_Actions.Output(droplet, output);
            return true;
        }

        // Droplets needing mixing are assumed to have been merged into one drop.
        // Does not take contaminants into account yet.
        public static bool MixDroplet(Droplet d, string pattern, string? newType = null) //TODO: Remake to make sure that droplet interference makes it try a different direction, not give up
        {
            Droplet_Actions.MixDroplet(d, pattern);
            if (newType != null)
            {
                d.ChangeType(newType);
            }
            return true;

        }

        internal static void WasteDroplet(Droplet droplet)
        {
            //throw new NotImplementedException();
        }

        public static bool MergeDroplets(List<string> inputDroplets, Droplet d, Task calcMerge, UsefulSemaphore beforeDone, Apparature cmdDestination)
        {
            Droplet_Actions.SetupDestinations(d, cmdDestination);
            Printer.PrintLine(d.Name + " : MERGING");
            foreach (var item1 in inputDroplets)
            {
                foreach (var item2 in inputDroplets)
                {
                    Droplet id1 = Program.C.board.Droplets[item1];
                    Droplet id2 = Program.C.board.Droplets[item2];

                    if(id1 != id2)
                    {
                        id1.Contamintants.Remove(id2.Substance_Name);
                    }

                }

            }
            calcMerge.Start();
            beforeDone.Wait(inputDroplets.Count);
            foreach (var inputDroplet in inputDroplets)
            {
                Droplet other = Program.C.board.Droplets[inputDroplet];
                if (!other.Removed)
                {
                    d.TakeOver(other);
                }
            }
            Printer.PrintBoard();
            return true;
        }

        public static bool SplitDroplet(Droplet d, Dictionary<string, double> percentages, Dictionary<string, UsefulSemaphore> dropSem, Apparature cmdDestination)
        {
            //Droplet_Actions.SetupDestinations(d, cmdDestination);
            d.Important = true;

            // Run Droplet_Actions.splitDroplet
            Droplet_Actions.SplitDroplet(d, percentages, dropSem);
            return true;
        }

        public static bool AwaitSplitWork(Droplet droplet, Apparature cmdDestination, UsefulSemaphore beginSem)
        {
            // Set destinations and release one semaphore
            Droplet_Actions.SetupDestinations(droplet, cmdDestination);
            beginSem.TryReleaseOne();

            // Wait for SplitDroplet to release 2 semaphore
            beginSem.Wait(2);

            Droplet_Actions.MoveToApparature(droplet, droplet.nextDestination);
            return true;
        }

        public static bool AwaitMergeWork(Droplet d, Task<Electrode> AwaitWork, UsefulSemaphore beforeDone, UsefulSemaphore selfDone, List<string>? mergeDoplets = null) // check if beforedone is done, and then release on selfDone when done
        {
            d.Important = true;
            d.SnekList = [];
            d.SnekMode = false;
            beforeDone.WaitOne();
            Electrode location = AwaitWork.Result;
            try
            {
                d.CurrentPath = null;
                d.nextElectrodeDestination = location;
                Droplet_Actions.MoveToDest(d, location, mergeDoplets);
            }
            catch (ThreadInterruptedException e)
            {
                selfDone.TryReleaseOne();
                throw e;
            }
            selfDone.TryReleaseOne();
            return true;

        }

        public static bool TempDroplet(Droplet d, Heater heater, int time, string? newType = null)
        {
            Droplet_Actions.SetupDestinations(d, heater);
            d.Important = true;
            Droplet_Actions.MoveToApparature(d, heater);
            if (time <= 0)
            {
                throw new ArgumentException("Time must be greater than 0");
            }
            Droplet_Actions.WaitDroplet(d, time*1000);
            if (newType != null && d.Substance_Name != newType)
            {
                d.ChangeType(newType);
            }
            return true;
        }

        public static bool SenseDroplet(Droplet d, Sensor sensor)
        {
            Droplet_Actions.SetupDestinations(d, sensor);
            Printer.PrintLine(d.Name + " : SENSING");
            d.Important = true;
            Electrode closest = Droplet_Actions.MoveToApparature(d, sensor);
            Droplet_Actions.CoilSnek(d, center: closest, app: sensor); // Depends if sensor needs to see the entire droplet
            sensor.Sense();
            return true;
        }
        public static bool WaitDroplet(Droplet d, int time)
        {
            d.Important = true;
            Printer.PrintLine(d.Name + " : WAITING");
            Droplet_Actions.WaitDroplet(d,time);
            Printer.PrintLine(d.Name + " : DONE WAITING");
            return true;
        }





    }
}
