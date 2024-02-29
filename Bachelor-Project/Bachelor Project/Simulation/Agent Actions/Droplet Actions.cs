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
        
        public static void InputDroplet(Droplet d, Input i, int volume)
        {
            d.SetSizes(volume);
            d.PositionX = i.pointers[0].ePosX;
            d.PositionY = i.pointers[0].ePosY;
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
            AwaiLegalMove(d, i.pointers);
            Outparser.Outparser.ElectrodeOn(i.pointers[0]);
            d.Occupy.Add(i.pointers[0]);
            i.pointers[0].Occupant = d;
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

        public static bool CheckPlacement(Droplet d, List<Electrode> temp)
        {
            bool legalMove = true;
            foreach (Electrode e in temp) {
                if (!(e.Occupant == null || e.Occupant.Equals(d)))
                {
                    legalMove = false;
                    break;
                }

                foreach (string c in e.GetContaminants())
                {
                    if (d.Contamintants.Contains(c))
                    {
                        legalMove = false;
                        goto destination;
                    }
                }
            }

            destination:
            return legalMove;
        }

        public static bool CheckLegalMove(Droplet d, List<Electrode> temp)
        {
            bool legalMove = true;

            if (!(CheckBorder(d, temp) && CheckPlacement(d, temp))){
                legalMove = false;
            }

            return legalMove;
        }

        public static void AwaiLegalMove(Droplet d, List<Electrode> temp)
        {
            while (!CheckLegalMove(d, temp))
            {
                Console.WriteLine(d.Name + " waiting for space");
                Thread.Sleep(100);
            }
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
            //throw new NotImplementedException();
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

        internal static void MixDroplets(Droplet droplet1, Droplet droplet2, string pattern, string newType)
        {
            //throw new NotImplementedException();
        }

        internal static void TempDroplet(Droplet droplet1, Droplet droplet2, int temp, string newType)
        {
            //throw new NotImplementedException();
        }

        internal static void SenseDroplet(Droplet droplet1, Droplet droplet2, string sensorType)
        {
            //throw new NotImplementedException();
        }


        // Uncoil snake



        // Coil snake
        public static void CoilSnek(Droplet d)
        {
            // Droplet cannot coil if it is not in snekMode
            // Snake is already considered coiled if it is 2 or less long
            if (!d.snekMode || d.Occupy.Count <= 2)
            {
                return;
            }


            int len = d.Occupy.Count;

            int w = (int)Math.Ceiling(Math.Sqrt(len));

            Console.WriteLine("w = " + w);

            // Find area to coil into.

            (Direction dir, int sideRem, string side) = FindArea(d, w);

            Console.WriteLine("FindArea returns: dir = " + dir + ", sideRem = " + sideRem + ", side = " + side);

            // If unable to find area, return without moving.
            if (side.Equals("none"))
            {
                return;
            }

            // Move to fill full side
            for (int i = 0; i < sideRem; i++)
            {
                Console.WriteLine("I made a move!");
                SnekMove(d, dir);
            }

            // Fill rest of area
            // Prototype with dir = UP and side = left
            Direction tempDir = dir;
            Direction sideDir = dir;

            switch (dir)
            {
                case Direction.UP:
                    if (side.Equals("left")) { sideDir = Direction.LEFT; }
                    else {  sideDir = Direction.RIGHT; }
                    break;
                case Direction.LEFT:
                    if (side.Equals("left")) { sideDir = Direction.DOWN; }
                    else { sideDir = Direction.UP; }
                    break;
                case Direction.DOWN:
                    if (side.Equals("left")) { sideDir = Direction.RIGHT; }
                    else { sideDir = Direction.LEFT; }
                    break;
                case Direction.RIGHT:
                    if (side.Equals("left")) { sideDir = Direction.UP; }
                    else { sideDir = Direction.DOWN; }
                    break;
            }

            // Start at 1 because the first (0th) row is already done
            for (int i = 1; i < w; i++)
            {
                // Go to the side
                SnekMove(d, sideDir);

                // Find direction for this pass
                switch (dir) { 
                    case Direction.UP:
                        if (i%2 == 0) { tempDir = Direction.UP; } else { tempDir = Direction.DOWN;}
                        break;
                    case Direction.LEFT:
                        if (i % 2 == 0) { tempDir = Direction.LEFT; } else { tempDir = Direction.RIGHT; }
                        break;
                    case Direction.DOWN:
                        if (i % 2 == 0) { tempDir = Direction.DOWN; } else { tempDir = Direction.UP; }
                        break;
                    case Direction.RIGHT:
                        if (i % 2 == 0) { tempDir = Direction.RIGHT; } else { tempDir = Direction.LEFT; }
                        break;
                }

                for (int j = 1; j < w; j++)
                {
                    SnekMove(d, tempDir);
                }
                
            }
            //Movement is done!
        }

        // Check area for coil
        public static (Direction dir, int sideRemainder, string side) FindArea(Droplet d, int w)
        {
            // Needs to find an area around where snake is which is big enough to fit the coiled snake.
            // Parts of the snake may be there, but needs to be gone once coil reaches.

            // It is easiest if there is already a space where the snake can just continue its path as the start of the coil.

            // If theat is not possible, it needs to find a free space nearby - how far until we say it cannot do it?

            // Should return a start position and two directions, first is for w and second is for h.

            // Function knows w, h, and position of head.



            // Check space behind left/right snake head, then move further up until space is found
            // Go back along snakes body as far as w and as long as it is straight behind the head - begin check from there.
            // Check on one side if there is room for the snake (+ buffer?). If a problem is found, check the other.
            // If the other side also has a problem, begin checking from the problem point on the side with the "earliest" problem.
            // Continue until a clear area is found.

            // Check with +2 for boundery.

            // Follow along snake to find start point and direction (the direction the snake is moving)
            Direction dir;
            Electrode head = d.Occupy[0];

            if (d.Occupy[1].ePosX == head.ePosX)
            {
                if (d.Occupy[1].ePosY == head.ePosY + 1)
                {
                    dir = Direction.UP;
                }
                else
                {
                    dir = Direction.DOWN;
                }
            }
            else
            {
                if (d.Occupy[1].ePosX == head.ePosX - 1)
                {
                    dir = Direction.RIGHT;
                }
                else
                {
                    dir = Direction.LEFT;
                }
            }

            // init to 2 for head and Occypy[1]
            int straightCount = 2;
            int startX = 0;
            int startY = 0;

            // Find start point for looking
            switch (dir)
            {
                case Direction.UP:
                    for(int i = 2; i < w; i++)
                    {
                        if (i < Program.C.board.GetYElectrodes() && d.Occupy[i].ePosY == head.ePosY + i)
                        {
                            straightCount++;
                        } else { break; }
                    }

                    startX = head.ePosX;
                    startY = head.ePosY + straightCount;

                    break;
                case Direction.DOWN:
                    for (int i = 2; i < w; i++)
                    {
                        if (i < Program.C.board.GetYElectrodes() && d.Occupy[i].ePosY == head.ePosY - i)
                        {
                            straightCount++;
                        }
                        else { break; }
                    }

                    startX = head.ePosX;
                    startY = head.ePosY - straightCount;

                    break;
                case Direction.RIGHT:
                    for (int i = 2; i < w; i++)
                    {
                        if (i < Program.C.board.GetXElectrodes() && d.Occupy[i].ePosX == head.ePosX - i)
                        {
                            straightCount++;
                        }
                        else { break; }
                    }

                    Console.WriteLine(straightCount);

                    startX = head.ePosX - straightCount +1;
                    startY = head.ePosY;

                    break;
                case Direction.LEFT:
                    for (int i = 2; i < w; i++)
                    {
                        if (i < Program.C.board.GetXElectrodes() && d.Occupy[i].ePosX == head.ePosX + i)
                        {
                            straightCount++;
                        }
                        else { break; }
                    }

                    startX = head.ePosX + straightCount;
                    startY = head.ePosY;

                    break;

            }


            List<Electrode> leftArea = new List<Electrode>();
            List<Electrode> rightArea = new List<Electrode>();

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    switch (dir)
                    {
                        case Direction.UP:
                            leftArea.Add(Program.C.board.Electrodes[startX - i, startY + j]);
                            rightArea.Add(Program.C.board.Electrodes[startX + i, startY + j]);
                            break;
                        case Direction.LEFT:
                            leftArea.Add(Program.C.board.Electrodes[startX - i, startY - j]);
                            rightArea.Add(Program.C.board.Electrodes[startX - i, startY + j]);
                            break;
                        case Direction.DOWN:
                            leftArea.Add(Program.C.board.Electrodes[startX + i, startY - j]);
                            rightArea.Add(Program.C.board.Electrodes[startX - i, startY - j]);
                            break;
                        case Direction.RIGHT:
                            leftArea.Add(Program.C.board.Electrodes[startX + i, startY - j]);
                            rightArea.Add(Program.C.board.Electrodes[startX + i, startY + j]);
                            break;
                    }

                }
            }
            Console.WriteLine(dir);

            Console.WriteLine("Head: " + head.Name);

            foreach (Electrode e in leftArea)
            {
                Console.WriteLine("(" + e.ePosX + ", " + e.ePosY + ")");
            }

            string side = "none";
            if (CheckLegalMove(d, leftArea))
            {
                side = "left";
            }else if (CheckLegalMove(d, rightArea)) {
                side = "right";
            }

            return (dir, w - straightCount, side);

            /*
            switch (dir)
            {
                case Direction.UP:

                    break;
                case Direction.LEFT:

                    break;
                case Direction.DOWN:

                    break;
                case Direction.RIGHT:

                    break;
            }
            */

        }

        internal static void AwaitWork(Droplet droplet)
        {
            throw new NotImplementedException();
        }



        // Fix snake - If snake is broken, remake it.
    }
}
