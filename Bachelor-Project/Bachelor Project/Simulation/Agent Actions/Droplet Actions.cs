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
            ArrayList temp = new ArrayList();
            Electrode? newE = null;
            foreach (Electrode e in d.Occupy)
            {

                switch (dir)
                {
                    case Direction.UP:
                        newE = Program.C.board.Electrodes[e.ePosX, e.ePosY - 1];
                        
                        break;
                    case Direction.RIGHT:
                        newE = Program.C.board.Electrodes[e.ePosX + 1, e.ePosY];
                        
                        break;
                    case Direction.DOWN:
                        newE = Program.C.board.Electrodes[e.ePosX, e.ePosY + 1];
                        
                        break;
                    case Direction.LEFT:
                        newE = Program.C.board.Electrodes[e.ePosX - 1, e.ePosY];
                        
                        break;
                }
                temp.Add(newE);
                newE.Occupant = d;
                e.Occupant = null;
            }
            d.Occupy = temp;
        }

    }
}
