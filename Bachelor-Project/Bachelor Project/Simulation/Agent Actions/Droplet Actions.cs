using Bachelor_Project.Electrode_Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation.Agent_Actions
{
    internal static class Droplet_Actions
    {
        private static readonly int mixAmount = 5;
        public static async void InputDroplet(Droplet d, Input i, int volume)
        {
            d.SetSizes(volume);
            d.PositionX = i.point.ePosX;
            d.PositionY = i.point.ePosY;
            InputPart(d, i);
            int size = d.Size;
            size -= 1;
            while (size > 0)
            {
                MoveDroplet(d, Direction.RIGHT);
                InputPart(d, i);
                size -= 1;
            }
        }

        private static void InputPart(Droplet d, Input i)
        {
            Outparser.Outparser.ElectrodeOn(i.point);
            d.Occupy.Add(i.point);
            i.point.Occupant = d;
        }

        public static void MoveDroplet(Droplet d, Direction dir)
        {
            bool legalMove = true;
            List<Electrode> temp = new List<Electrode>();
            Electrode? newE = null;
            foreach (Electrode e in d.Occupy)
            {
                int xChange = 0;
                int yChange = 0;
                switch (dir)
                {
                    case Direction.UP:
                        yChange = -1;
                        
                        break;
                    case Direction.RIGHT:
                        xChange = 1;
                        
                        break;
                    case Direction.DOWN:
                        yChange = 1;
                        
                        break;
                    case Direction.LEFT:
                        xChange = -1;
                        
                        break;
                }

                if (e.ePosX + xChange < Program.C.board.GetXElectrodes() && e.ePosX + xChange >= 0 && e.ePosY + yChange < Program.C.board.GetYElectrodes() && e.ePosY + yChange >= 0)
                {
                    newE = Program.C.board.Electrodes[e.ePosX + xChange, e.ePosY + yChange];
                    temp.Add(newE);
                }
                else
                {
                    legalMove = false;
                }
            }

            if (legalMove)
            {
                // Turn on all new electrodes first
                foreach (Electrode e in temp)
                {
                    if (e.Status == 0)
                    {
                        Outparser.Outparser.ElectrodeOn(e);
                        e.Occupant = d;
                    }
                }

                // Turn off all old electrodes second (which are not also new)
                foreach (Electrode e in d.Occupy)
                {
                    bool contains = false;
                    foreach (Electrode ee in temp)
                    {
                        if (e.Equals(ee)) { contains = true; break; }
                    }
                    if (!contains) {
                        Outparser.Outparser.ElectrodeOff(e);
                        e.Occupant = null;
                    }

                    

                }

                d.Occupy = temp;
            }
        }

        // Droplets needing mixing are assumed to have been merged into one drop.
        // Does not take contaminants into account yet.
        public static void Mix(Droplet d)
        {
            bool up = true; bool down = true; bool left = true; bool right = true;
            // Check if there is room to boogie
            // Only checks board bounderies
            foreach (Electrode e in d.Occupy)
            {
                // Check board bounderies
                if (e.ePosX < 1) left = false;
                if (!(e.ePosX < Program.C.board.GetWidth() - 1)) right = false;
                if (e.ePosY < 1) up = false;
                if (!(e.ePosY < Program.C.board.GetHeight() - 1)) down = false;

                // Check for other droplets in zone (+ boarder)


                // Check for contamination
            }

            // Make good movement
            if (up)
            {
                if (left)
                {
                    for (int i = 0; i < mixAmount; i++)
                    {
                        MoveDroplet(d,Direction.UP);
                        MoveDroplet(d,Direction.LEFT);
                        MoveDroplet(d,Direction.DOWN);
                        MoveDroplet(d,Direction.RIGHT);
                    }
                }else if (right)
                {
                    for (int i = 0; i < mixAmount; i++)
                    {
                        MoveDroplet(d,Direction.RIGHT);
                        MoveDroplet(d,Direction.UP);
                        MoveDroplet(d,Direction.LEFT);
                        MoveDroplet(d,Direction.DOWN);
                    }
                }
            }else if (down)
            {
                if (left)
                {
                    for (int i = 0; i < mixAmount; i++)
                    {
                        MoveDroplet(d,Direction.LEFT);
                        MoveDroplet(d,Direction.DOWN);
                        MoveDroplet(d,Direction.RIGHT);
                        MoveDroplet(d,Direction.UP);
                    }
                }
                else if (right)
                {
                    for (int i = 0; i < mixAmount; i++)
                    {
                        MoveDroplet(d,Direction.DOWN);
                        MoveDroplet(d,Direction.RIGHT);
                        MoveDroplet(d,Direction.UP);
                        MoveDroplet(d,Direction.LEFT);
                    }
                }
            }
        }

        // Used to check if new droplet position upholds border
        public static bool CheckBorder(Droplet d, List<Electrode> temp)
        {
            // For snek, just put in head instead of all positions
            bool legalMove = true;
            foreach (Electrode e in temp)
            {
                // Check neighbors all the way around electrode for occupancy
                // If same droplet, fine. If blank, fine. If other droplet, not fine.

                int xCheck = e.ePosX;
                int yCheck = e.ePosY;
                for(int i = 1; i <= 8;i++)
                {
                    switch(i)
                    {
                        case 1:
                            xCheck--;
                            yCheck--;
                            break;
                        case 2:
                            xCheck++;
                            break;
                        case 3:
                            xCheck++;
                            break;
                        case 4:
                            xCheck -= 2;
                            yCheck++;
                            break;
                        case 5:
                            xCheck += 2;
                            break;
                        case 6:
                            xCheck -= 2;
                            yCheck++;
                            break;
                        case 7:
                            xCheck++;
                            break;
                        case 8:
                            xCheck++;
                            break;
                    }

                    if (!(xCheck < 0 || xCheck >= Program.C.board.GetXElectrodes() || yCheck < 0 || yCheck >= Program.C.board.GetYElectrodes()))
                    {
                        Droplet? occupant = Program.C.board.Electrodes[xCheck, yCheck].Occupant;
                        if (!(occupant == null || occupant.Equals(d)))
                        {
                            legalMove = false;
                            goto destination;
                        }
                    }
                    
                }
            }
            destination:
            return legalMove;
        }

        public static bool SnekCheck(Electrode newHead)
        {
            if (newHead.Occupant == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        // Non-protected snake move forward 1
        // Assumes that the list of occupied electrodes are in the form of a snake.
        public static void SnekMove(Droplet d, Direction dir)
        {
            List<Electrode> newOcc = new List<Electrode>();
            List<Electrode> newHead = new List<Electrode>(); // Needs to be a list containing one electrode for a snekcheck.
            Electrode head = d.Occupy.FirstOrDefault();

            try
            {
                switch (dir)
                {
                    case Direction.UP:
                        newHead.Add(Program.C.board.Electrodes[head.ePosX, head.ePosY - 1]);
                        break;
                    case Direction.LEFT:
                        newHead.Add(Program.C.board.Electrodes[head.ePosX - 1, head.ePosY]);
                        break;
                    case Direction.DOWN:
                        newHead.Add(Program.C.board.Electrodes[head.ePosX, head.ePosY + 1]);
                        break;
                    case Direction.RIGHT:
                        newHead.Add(Program.C.board.Electrodes[head.ePosX + 1, head.ePosY]);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Movement out of bounds");
                return;
            }
            

            // Do a snekcheck
            // If move is legal, do the thing
            if (CheckBorder(d, newHead) && SnekCheck(newHead[0]))
            {
                Console.WriteLine("New head: " + newHead[0].ePosX + " " + newHead[0].ePosY);
                Console.WriteLine("Old head: " + head.ePosX + " " + head.ePosY);
                newOcc = newHead;
                Outparser.Outparser.ElectrodeOn(newHead[0]);
                newHead[0].Occupant = d;
                newOcc = newOcc.Concat(d.Occupy).ToList();
                Outparser.Outparser.ElectrodeOff(newOcc[newOcc.Count - 1]);
                newOcc[newOcc.Count - 1].Occupant = null;
                newOcc.RemoveAt(newOcc.Count - 1);
                d.Occupy = newOcc;
                Console.WriteLine("Droplet moved");
            }
            else
            {
                Console.WriteLine("Droplet not moved");
            }
        }


        // Switch head and tail of snake
        public static void SnekReversal(Droplet d)
        {
            d.Occupy.Reverse();
        }

        internal static void OutputDroplet(Droplet droplet, Output output)
        {
            throw new NotImplementedException();
        }

        internal static void WasteDroplet(Droplet droplet)
        {
            throw new NotImplementedException();
        }

        internal static void MergeDroplets(List<string> inputDroplets, Droplet droplet)
        {
            throw new NotImplementedException();
        }

        internal static void SplitDroplet(Droplet droplet, List<string> outputDroplets)
        {
            throw new NotImplementedException();
        }

        internal static void MixDroplets(Droplet droplet1, Droplet droplet2, string pattern, string newType)
        {
            throw new NotImplementedException();
        }

        internal static void TempDroplet(Droplet droplet1, Droplet droplet2, int temp, string newType)
        {
            throw new NotImplementedException();
        }

        internal static void SenseDroplet(Droplet droplet1, Droplet droplet2, string sensorType)
        {
            throw new NotImplementedException();
        }


        // Uncoil snake



        // Coil snake


        // Fix snake - If snake is broken, remake it.
    }
}
