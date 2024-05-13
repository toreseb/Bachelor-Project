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
        public static bool InputDroplet(Droplet d, Input i, int volume, Apparature? destination = null)
        {
            Printer.PrintLine(d.Name + " has NextDestiantion of: " + d.nextDestination);
            Printer.PrintLine(d.Name + " : INPUTTING");
            bool result = Droplet_Actions.InputDroplet(d, i, volume, destination);
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
                        d.SnekList = [];
                        d.SnekMode = false;
                        d.MergeReady = true;
                        return false;
                    }

                    if (d.CurrentPath.Value.path.Count <= Constants.DestBuff)
                    {
                        Droplet_Actions.CoilSnek(d, d.SnekList.First());
                        return true;
                    }
                }
            }
            return result;

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
        public static bool MixDroplets(Droplet d, string pattern, string? newType = null) //TODO: Remake to make sure that droplet interference makes it try a different direction, not give up
        {
            Printer.PrintLine(d.Name + " : MIXING");
            d.Important = true;
            int retryCounter = 0;
            Program.C.RemovePath(d);
            retry:
            bool up = true; bool down = true; bool left = true; bool right = true;
            // Check if there is room to boogie
            // Only checks board bounderies
            foreach (Electrode e in d.Occupy)
            {
                // Check board bounderies
                if (e.ePosX < 1) left = false;
                if (!(e.ePosX < Program.C.board.GetXElectrodes() - 1)) right = false;
                if (e.ePosY < 1) up = false;
                if (!(e.ePosY < Program.C.board.GetYElectrodes() - 1)) down = false;
            }

            // Check for other droplets and contaminants in zone (+ boarder)
            // Needs to check for each possible direction
            List<Electrode> temp = new List<Electrode>(d.Occupy);

            if (Convert.ToInt32(up) + Convert.ToInt32(right) + Convert.ToInt32(down) + Convert.ToInt32(left) >= 2 && !((Convert.ToInt32(up) + Convert.ToInt32(down) == 0) || (Convert.ToInt32(right) + Convert.ToInt32(left) == 0)))
            {
                foreach (Electrode e in d.Occupy)
                {
                    if (up && Program.C.board.Electrodes[e.ePosX, e.ePosY - 1].Occupant != d)
                    {
                        temp.Add(Program.C.board.Electrodes[e.ePosX, e.ePosY - 1]);
                    }
                    if (right && Program.C.board.Electrodes[e.ePosX + 1, e.ePosY].Occupant != d)
                    {
                        temp.Add(Program.C.board.Electrodes[e.ePosX + 1, e.ePosY]);
                    }
                    if (down && !up && Program.C.board.Electrodes[e.ePosX, e.ePosY + 1].Occupant != d)
                    {
                        temp.Add(Program.C.board.Electrodes[e.ePosX, e.ePosY + 1]);
                    }
                    if (left && !right && Program.C.board.Electrodes[e.ePosX - 1, e.ePosY].Occupant != d)
                    {
                        temp.Add(Program.C.board.Electrodes[e.ePosX - 1, e.ePosY]);
                    }
                }
                List<Direction> directions = [];
                if (up)
                {
                    directions.Add(Direction.UP);
                }
                else
                {
                    directions.Add(Direction.DOWN);
                }
                if (right)
                {
                    directions.Add(Direction.RIGHT);
                }
                else
                {
                    directions.Add(Direction.LEFT);
                }
                if (!up)
                {
                    directions.Add(Direction.UP);
                }
                else
                {
                    directions.Add(Direction.DOWN);
                }
                if (!right)
                {
                    directions.Add(Direction.RIGHT);
                }
                else
                {
                    directions.Add(Direction.LEFT);
                }
                // Check if area is legal
                if (Droplet_Actions.CheckLegalMove(d, temp).legalmove)
                {
                    for (int i = 0; i < Constants.MixAmount; i++)
                    {
                        foreach (var item in directions)
                        {
                            Droplet_Actions.MoveDroplet(d, item);
                            Printer.PrintBoard();
                        }
                    }

                    return true;
                }
                else
                {
                    if (retryCounter > 10)
                    {
                        throw new IllegalMoveException("No space for mixing");
                    }
                    Thread.Sleep(100);
                    retryCounter++;
                    goto retry;
                }

            }
            else
            {
                if (retryCounter > 10)
                {
                    throw new IllegalMoveException("No space for mixing");
                }
                Thread.Sleep(100);
                retryCounter++;
                goto retry;
            }

        }





        internal static void WasteDroplet(Droplet droplet)
        {
            //throw new NotImplementedException();
        }

        public static bool MergeDroplets(List<string> inputDroplets, Droplet d, Task calcMerge, UsefullSemaphore beforeDone, Apparature cmdDestination)
        {
            SetupDestinations(d, cmdDestination);
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

        public static bool SplitDroplet(Droplet d, Dictionary<string, double> ratios, Dictionary<string, UsefullSemaphore> dropSem, Apparature cmdDestination)
        {
            SetupDestinations(d, cmdDestination);
            d.Important = true;
            // Run Droplet_Actions.splitDroplet
            Droplet_Actions.splitDroplet(d, ratios, dropSem);
            return true;
        }

        public static bool AwaitSplitWork(Droplet droplet, Apparature cmdDestination, UsefullSemaphore beginSem)
        {
            SetupDestinations(droplet, cmdDestination);
            beginSem.WaitOne();

            Droplet_Actions.MoveToApparature(droplet, droplet.nextDestination);
            return true;
        }

        public static bool AwaitMergeWork(Droplet d, Task<Electrode> AwaitWork, UsefullSemaphore beforeDone, UsefullSemaphore selfDone, List<string>? mergeDoplets = null) // check if beforedone is done, and then release on selfDone when done
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

        public static bool TempDroplet(Droplet d, Heater heater, int time, string newType = null)
        {
            SetupDestinations(d, heater);
            Printer.PrintLine(d.Name + " : TEMPING");
            d.Important = true;
            Electrode closest = Droplet_Actions.MoveToApparature(d, heater);
            if (time <= 0)
            {
                throw new ArgumentException("Time must be greater than 0");
            }
            d.WaitDroplet(time*1000);
            if (newType != null && d.Substance_Name != newType)
            {
                d.ChangeType(newType);
            }
            return true;
        }

        public static bool SenseDroplet(Droplet d, Sensor sensor)
        {
            SetupDestinations(d, sensor);
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
            d.WaitDroplet(time);
            d.SnekMode = false;
            d.SnekList = [];
            Printer.PrintLine(d.Name + " : DONE WAITING");
            return true;
        }


        private static void SetupDestinations(Droplet d, Apparature destination)
        {
            d.nextDestination = destination;
            if (d.Occupy.Count > 0)
            {
                d.SetNextElectrodeDestination();
            }
            
        }


    }
}
