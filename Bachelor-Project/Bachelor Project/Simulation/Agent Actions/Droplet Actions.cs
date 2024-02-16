using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Outparser;
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
        public static Board Board { get; set; }
        private static readonly int mixAmount = 5;
        public static void InputDroplet(Droplet d, Input i, int size)
        {
            inputPart(d, i);
            size -= 1;
            while (size > 0)
            {
                MoveDroplet(d, Direction.RIGHT);
                inputPart(d, i);
                size -= 1;
            }
        }

        private static void inputPart(Droplet d, Input i)
        {
            d.Occupy.Add(i.point);
            i.point.Occupant = d;
        }

        public static void MoveDroplet(Droplet d, Direction dir)
        {
            bool legalMove = true;
            ArrayList temp = new ArrayList();
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

                if (e.ePosX + xChange < Board.GetWidth() && e.ePosX + xChange >= 0 && e.ePosY + yChange < Board.GetHeight() && e.ePosY + yChange >= 0)
                {
                    newE = Board.Electrodes[e.ePosX + xChange, e.ePosY + yChange];
                    temp.Add(newE);
                    newE.Occupant = d;
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
                    Outparser.Outparser.ElectrodeOn(e);
                }

                // Turn off all old electrodes second (which are not also new)
                foreach (Electrode e in d.Occupy)
                {
                    bool contains = false;
                    foreach (Electrode ee in temp)
                    {
                        if (e == ee) contains = true; break;
                    }
                    if (!contains) Outparser.Outparser.ElectrodeOff(e);

                    e.Occupant = null;

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
                if (!(e.ePosX < Board.GetWidth() - 1)) right = false;
                if (e.ePosY < 1) up = false;
                if (!(e.ePosY < Board.GetHeight() - 1)) down = false;

                // Check for other droplets in zone (+ boarder)


                // Check for contamination
            }

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

        // Used to check if new droplet position upholds boarder
        public static bool CheckBorder(Droplet d, ArrayList temp)
        {
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

                    if (!(xCheck < 0 || xCheck > Board.GetWidth() || yCheck < 0 || yCheck > Board.GetHeight()))
                    {
                        Droplet? occupant = Board.Electrodes[xCheck, yCheck].Occupant;
                        if (!(occupant.Equals(d) || occupant == null))
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

    }
}
