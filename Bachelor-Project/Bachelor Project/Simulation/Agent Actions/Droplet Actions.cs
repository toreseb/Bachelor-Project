using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;


namespace Bachelor_Project.Simulation.Agent_Actions
{
    public static class Droplet_Actions
    {
        private static readonly int mixAmount = 5;
        
        public static void InputDroplet(Droplet d, Input i, int volume, Apparature? destination = null)
        {
            Electrode destElectrode = null;

            d.SetSizes(volume);
            int size = d.Size;
            AwaiLegalMove(d, i.pointers);
            if (destination != null)
            {
                d.SnekMode = true;
                MoveOnElectrode(d, i.pointers[0]) ;
                size--;
                destElectrode = d.GetClosestFreePointer(destination);
            }
            nonSnek:
            if (destination == null)
            {
                
                while (size > 0)
                {
                    d.SnekMode = false;
                    CoilSnek(d, i.pointers[0], input: true);
                    size--;
                }
            }
            else
            {
                while (size > 0)
                {
                    MoveTowardDest(d, destElectrode);
                    MoveOnElectrode(d, i.pointers[0], first: false);
                    size--;
                    if (d.CurrentPath.Count <= 4)
                    {
                        while (size > 0)
                        {
                            MoveOnElectrode(d, d.SnekList.First(), first: false);
                            size--;
                        }
                        CoilSnek(d, d.SnekList.First());
                        return;
                    }
                }
            }
            
        }

        public static void MoveToDest(Droplet d, Electrode destination) //TODO: Make sure that the droplet moves to the destination
        {
            d.CurrentPath ??= ModifiedAStar.FindPath(d, destination);
            if (d.CurrentPath.Count == 0)
            {
                d.CurrentPath = ModifiedAStar.FindPath(d, destination);
            }
            try
            {
                while (d.CurrentPath.Count > 0)
                {
                    MoveTowardDest(d, destination);
                }
            }catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void MoveTowardDest(Droplet d, Electrode destination)
        {
            d.CurrentPath ??= ModifiedAStar.FindPath(d, destination);
            if (d.CurrentPath.Count == 0)
            {
                d.CurrentPath = ModifiedAStar.FindPath(d, destination);
            }
            if (d.CurrentPath[0].Item2 != null)
            {
                SnekMove(d, d.CurrentPath[0].Item2.Value);
            }
            d.CurrentPath.RemoveAt(0);
        }

        public static void MoveDroplet(Droplet d, Direction dir)
        {
            bool legalMove = true;
            List<Electrode> temp = new List<Electrode>();

            int xChange = 0;
            int yChange = 0;
            switch (dir)
            {
                case Direction.UP:
                    yChange = -1;

                    break;
                case Direction.LEFT:
                    xChange = -1;

                    break;
                case Direction.DOWN:
                    yChange = 1;

                    break;
                case Direction.RIGHT:
                    xChange = 1;

                    break;
            }

            // Make list with new placements of electrodes
            foreach (Electrode e in d.Occupy)
            {
                // Check if new posision is legal
                if (CheckLegalPosition(d,[(e.ePosX + xChange, e.ePosY + yChange)]))
                {
                    temp.Add(Program.C.board.Electrodes[e.ePosX + xChange, e.ePosY + yChange]);
                }
                else
                {
                    legalMove = false;
                    break;
                }
            }


            if (legalMove)
            {
                // Turn on all new electrodes first
                foreach (Electrode e in temp)
                {
                    if (e.Status == 0)
                    {
                        MoveOnElectrode(d, e);
                    }
                }
                List<Electrode> turnOff = [];
                // Turn off all old electrodes second (which are not also new)
                foreach (Electrode e in d.Occupy)
                {
                    bool contains = false;
                    if (temp.Contains(e))
                    {
                        contains = true;
                        continue;
                    }
                    if (!contains) {
                        turnOff.Add(e);
                    }
                }
                foreach (Electrode e in turnOff)
                {
                    MoveOffElectrode(d, e);
                }
                
                d.Occupy = temp;
            }
            else
            {
                //throw new IllegalMoveException();
                Console.WriteLine("Illegal Move");
            }
        }


        // Droplets needing mixing are assumed to have been merged into one drop.
        // Does not take contaminants into account yet.
        public static void Mix(Droplet d) //TODO: Remake to make sure that droplet interference makes it try a different direction, not give up
        {
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

            if(Convert.ToInt32(up)+ Convert.ToInt32(right)+ Convert.ToInt32(down)+ Convert.ToInt32(left) >= 2 && !((Convert.ToInt32(up) + Convert.ToInt32(down) == 0)||(Convert.ToInt32(right) + Convert.ToInt32(left) == 0)))
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
                }else
                {
                    directions.Add(Direction.LEFT);
                }
                if (!up)
                {
                    directions.Add(Direction.UP);
                }else
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
                if (CheckLegalMove(d, temp))
                {
                    for (int i = 0; i < mixAmount; i++)
                    {
                        foreach (var item in directions)
                        {
                            MoveDroplet(d, item);
                            Program.C.board.PrintBoardState();
                        }
                    }

                    return;
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

        // Used to check if new droplet position upholds border
        private static bool CheckBorder(Droplet d, List<Electrode> temp)
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

                    if (CheckEdge(xCheck, yCheck))
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

        public static bool CheckEdge(int xPos, int yPos)
        {
            return !(xPos < 0 || xPos >= Program.C.board.GetXElectrodes() || yPos < 0 || yPos >= Program.C.board.GetYElectrodes());
        }


        private static bool CheckPlacement(Droplet d, List<Electrode> temp)
        {
            bool legalMove = true;
            foreach (Electrode e in temp) {
                // Check for other occupants
                if (!(e.Occupant == null || e.Occupant.Equals(d)))
                {
                    legalMove = false;
                    break;
                }

                // Check for contaminants
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

        public static bool CheckLegalPosition(Droplet d, List<(int, int)> pos)
        {
            List<Electrode> temp = [];

            // Check if within board
            foreach (var (x,y) in pos)
            {
                if (x < Program.C.board.GetXElectrodes() && x >= 0 && y < Program.C.board.GetYElectrodes() && y >= 0)
                {
                    temp.Add(Program.C.board.Electrodes[x, y]);
                }
                else
                {
                    return false;
                }
            }

            // Check for other droplets and contamination
            return CheckLegalMove(d, temp);
        }


        private static void AwaiLegalMove(Droplet d, List<Electrode> temp)
        {
            while (!CheckLegalMove(d, temp))
            {
                Console.WriteLine(d.Name + " waiting for space");
                Thread.Sleep(100);
            }
        }



        private static bool SnekCheck(Electrode newHead)
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


        public static void SnekMove(Droplet d, Direction dir)
        {
            SnekMove(d,d.Occupy,dir);
        }

        // Non-protected snake move forward 1
        // Assumes that the list of occupied electrodes are in the form of a snake.
        public static void SnekMove(Droplet d, List<Electrode> el, Direction dir)
        {
            List<Electrode> newOcc = new List<Electrode>();
            List<Electrode> newHead = new List<Electrode>(); // Needs to be a list containing one electrode for a snekcheck.
            Electrode head;
            if (d.SnekMode)
            {
                head = d.SnekList.First();
            }
            else
            {
                throw new Exception("No head found");
            }

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
            if (CheckLegalMove(d, newHead) && SnekCheck(newHead[0]))
            {
                Console.WriteLine("New head: " + newHead[0].ePosX + " " + newHead[0].ePosY);
                Console.WriteLine("Old head: " + head.ePosX + " " + head.ePosY);

                MoveOnElectrode(d, newHead[0]);
                
                MoveOffElectrode(d);
                Console.WriteLine("Droplet moved");
            }
            else
            {
                Console.WriteLine("Droplet not moved");
            }
        }

        public static void MoveOnElectrode(Droplet d, Electrode e, bool first = true)
        {
            Outparser.Outparser.ElectrodeOn(e);
            if (d.SnekMode)
            {
                if (first)
                {
                    d.SnekList.AddFirst(e);
                }
                else
                {
                    d.SnekList.AddLast(e);
                }

            }
            d.Occupy.Add(e);
            e.Occupant = d;
        }

        public static void MoveOffElectrode(Droplet d, Electrode? e = null)
        {
            if (e == null)
            {
                e = d.SnekList.Last();
            }

            Outparser.Outparser.ElectrodeOff(e);
            if (!e.GetContaminants().Contains(d.Substance_Name))
            {
                e.Contaminate(d.Substance_Name);
            }
            if (d.SnekMode)
            {
                d.SnekList.Remove(e);
            }
            d.Occupy.Remove(e);
            e.Occupant = null;
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

        internal static void AwaitWork(Droplet droplet)
        {
            //throw new NotImplementedException();
        }


        // Uncoil snake - takes droplet and destination
        // Choose head at spot close to destination. Run through rest of body, check for each part if moving it would disconnect body,
        // if yes, go to next part, if no, turn on next electrode for head and turn off electrode for part.
        public static void UncoilSnek(Droplet d, Electrode dest)
        {
            // If snake already occupies destination, do nothing.
            if (dest.Occupant.Equals(d))
            {
                return;
            }

            // Make a temp snake to snek move towards the destination that grows with the shrinkage of the droplet.
            d.SnekMode = true;
            Droplet tempSnek = new Droplet(d.Substance_Name, "temp" + d.Name);

            // Make tree out of blob in order to know what can safely be removed.
            Tree blobTree = BuildTree(d, [], dest);

            tempSnek.Occupy.Add(blobTree.closestElectrode);
            d.SnekList.AddFirst(blobTree.closestElectrode);

            // Make single moves all the way towards the destination.
            do
            {
                // Save last electrode so we can turn it on again.
                // The tree will turn off the correct electrode.
                Electrode temp = tempSnek.Occupy[^1];
                MoveTowardDest(d, dest);

                // If there are still nodes in the tree, it means that the snake is still uncoiling and the electrode that is turned off
                // needs to be controlled by the tree. Otherwise, we are no longer uncoiling and we can just move.
                // "> 1" because the last should not be counted.
                if (blobTree.Nodes.Count > 1)
                {
                    // Turn on the electroede that was not supposed to be turned off.
                    MoveOnElectrode(d, temp, first: false);
                    // Turn off the right electrode.
                    blobTree.RemoveLeaf();
                }

                Program.C.board.PrintBoardState();

            } while (d.CurrentPath != null && d.CurrentPath.Count != 0);

            // Once at dest, whether the snake is fully uncoiled or not, coil the snake again.
            // This avoids long tails being in the way.
            CoilSnek(d, dest);
        }

        // Coil snake
        // Could try doing it without thinking of it as a snake, just a bunch of small droplets moving to be a big one.
        public static void CoilSnek(Droplet d, Electrode? center = null, bool input = false)
        {
            if (center == null)
            {
                center = d.SnekList.First();
            }
            d.SnekMode = false;
            d.SnekList.Clear();
            
            int amount = input ? d.Occupy.Count: d.Occupy.Count -1; // -1 because the center is not in the list 0 if it inputs new value
            if (amount == 0 && input)
            {
                MoveOnElectrode(d, center);
            }
            
            
            
            List<Electrode> newBlob = [center];
            List<Electrode> activeBlob1 = [center];
            List<Electrode> activeBlob2 = [];
            List<Electrode> seenElectrodes = [center];
            int i = 1;
            while(activeBlob1.Count > 0)
            {
                Electrode current = activeBlob1[0];
                List<(Electrode, Direction)> neighbors = current.getNeighbors();
                foreach (var item in neighbors)
                {
                    if (CheckLegalMove(d,[item.Item1]) && !seenElectrodes.Contains(item.Item1) && item.Item1.Apparature == null)
                    {
                        activeBlob2.Add(item.Item1);
                        seenElectrodes.Add(item.Item1);
                        newBlob.Add(item.Item1);
                        amount--;
                        if (amount <= 0)
                        {
                            goto done;
                        }
                    }
                }
                activeBlob1.Remove(current);
                if (activeBlob1.Count == 0)
                {
                    activeBlob1 = activeBlob2;
                    activeBlob2 = [];
                }
            }
        done:
            
            Tree snekTree = BuildTree(d, newBlob, center);

            foreach (var item in newBlob)
            {
                if (!d.Occupy.Contains(item))
                {
                    MoveOnElectrode(d, item);
                }
            }
            snekTree.RemoveTree();

            Program.C.board.PrintBoardState();

        }

        public static Tree BuildTree(Droplet d, List<Electrode> newElectrodes, Electrode center)
        {
            return new Tree(d, d.Occupy, newElectrodes, center);
        }

        
        /*
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
            

        }*/

    }
}
