﻿using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Electrode_Types.Actuator_Types;
using Bachelor_Project.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation.Agent_Actions
{
    // This class contains the more complicated missions the agents will have.
    public class Mission_Tasks
    {
        public static bool InputDroplet(Droplet d, Input i, int volume, Apparature? destination = null)
        {
            return Droplet_Actions.InputDroplet(d, i, volume, destination);


        }
        public static void OutputDroplet(Droplet droplet, Output output)
        {
            droplet.Important = true;
            Droplet_Actions.MoveToApparature(droplet, output);
            Droplet_Actions.Output(droplet, output);
        }



        private static readonly int mixAmount = 5;

        // Droplets needing mixing are assumed to have been merged into one drop.
        // Does not take contaminants into account yet.
        public static bool MixDroplets(Droplet d, string pattern, string? newType = null) //TODO: Remake to make sure that droplet interference makes it try a different direction, not give up
        {
            d.Important = true;
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
                if (Droplet_Actions.CheckLegalMove(d, temp))
                {
                    for (int i = 0; i < mixAmount; i++)
                    {
                        foreach (var item in directions)
                        {
                            Droplet_Actions.MoveDroplet(d, item);
                            Program.C.board.PrintBoardState();
                        }
                    }

                    return true;
                }
                else
                {
                    throw new IllegalMoveException();
                }

            }
            else
            {
                throw new IllegalMoveException();
            }

        }





        internal static void WasteDroplet(Droplet droplet)
        {
            //throw new NotImplementedException();
        }

        internal static void MergeDroplets(List<string> inputDroplets, Droplet droplet)
        {
            //throw new NotImplementedException();
        }

        internal static void SplitDroplet(Droplet droplet, List<string> outputDroplets)
        {
            //throw new NotImplementedException();
        }

        internal static void AwaitWork(Droplet droplet)
        {
            //throw new NotImplementedException();
        }

        internal static void TempDroplet(Droplet droplet1, Heater heater, string newType)
        {
            droplet1.Important = true;
            Electrode closest = Droplet_Actions.MoveToApparature(droplet1, heater);
            Droplet_Actions.CoilSnek(droplet1, center: closest, into: heater);
            Thread.Sleep(1000); // Time to heat?
            droplet1.ChangeType(newType);
        }

        internal static void SenseDroplet(Droplet droplet1, Sensor sensor)
        {
            droplet1.Important = true;
            Electrode closest = Droplet_Actions.MoveToApparature(droplet1, sensor);
            Droplet_Actions.CoilSnek(droplet1, center: closest, into: sensor); // Depends if sensor needs to see the entire droplet
            Thread.Sleep(1000); // Time to sense?
        }

        


    }
}