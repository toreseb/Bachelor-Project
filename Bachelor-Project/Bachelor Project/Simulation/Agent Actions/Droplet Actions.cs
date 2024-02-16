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

    }
}
