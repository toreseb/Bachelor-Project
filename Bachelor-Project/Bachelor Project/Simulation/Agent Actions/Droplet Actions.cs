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
    // This class contains the more basic movements and actions an agent can take.
    public static class Droplet_Actions
    {        

        public static bool InputDroplet(Droplet d, Input i, int volume, Apparature? destination = null)
        {
            if (d.Inputted)
            {
                Console.WriteLine("Droplet already inputted");
                throw new Exception("Droplet already inputted");
            }
            d.Inputted = true;
            d.Important = false;
            d.Waiting = false;
            Electrode destElectrode = null;

            d.SetSizes(volume);
            Console.WriteLine("error");
            int size = d.Size;
            AwaitLegalMove(d, i.pointers);

            if (destination != null)
            {
                d.SnekMode = true;
                MoveOnElectrode(d, i.pointers[0]);
                size--;
                destElectrode = d.GetClosestFreePointer(destination);
            }

            if (destination == null)
            {
                d.SnekMode = false;
                while (size > 0)
                {
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
                        return true;
                    }
                }

                while (d.CurrentPath.Count > 4)
                {
                    d.Waiting = true;
                    try
                    {
                        MoveTowardDest(d, destElectrode);
                    }
                    catch (NewWorkException)
                    {
                        Program.C.board.PrintBoardState();
                        d.Waiting = false;
                        return false;
                    }

                    if (d.CurrentPath.Count <= 4)
                    {
                        CoilSnek(d, d.SnekList.First());
                        return true;
                    }
                }
            }
            return true;


        }

        public static Electrode MoveToApparature(Droplet d, Apparature dest)
        {
            Electrode closest = d.GetClosestFreePointer(dest);
            MoveToDest(d, closest);
            return closest;
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

        public static Electrode MoveTowardApparature(Droplet d, Apparature dest)
        {
            Electrode closest = d.GetClosestFreePointer(dest);
            MoveTowardDest(d, closest);
            return closest;
        }

        public static bool MoveTowardDest(Droplet d, Electrode destination) // returns true if droplet physcally moves, false if the head changes location in the droplet
        {
            bool moved = true;
            if (d.GetWork().Count > 0 && d.Waiting == true && d.Important == false)
            {
                throw new NewWorkException();
            }
            d.CurrentPath ??= ModifiedAStar.FindPath(d, destination);
            if (!d.SnekMode)
            {
                UncoilSnek(d, destination);
            }

            if (d.CurrentPath.Count == 0)
            {
                d.CurrentPath = ModifiedAStar.FindPath(d, destination);
            }
            if (d.CurrentPath[0].Item2 != null && d.Occupy.Contains(d.CurrentPath[0].Item1.ElectrodeStep(d.CurrentPath[0].Item2.Value)))
            {
                moved = false;
                d.SnekList.AddFirst(d.CurrentPath[0].Item1.ElectrodeStep(d.CurrentPath[0].Item2.Value));

            }
            else if (d.CurrentPath[0].Item2 != null)
            {
                SnekMove(d, d.CurrentPath[0].Item2.Value);
            }
            d.CurrentPath.RemoveAt(0);
            return moved;
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


        public static void AwaitLegalMove(Droplet d, List<Electrode> temp)
        {
            int i = 0;
            while (!CheckLegalMove(d, temp))
            {
                Console.WriteLine(d.Name + " waiting for space");
                if (i > 50)
                {
                    Console.WriteLine(d.Name + " waited for too long");
                    throw new Exception("Droplet waited for too long");
                }
                Thread.Sleep(100);
                i++;
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
            Console.WriteLine("SnekMove Toward: " +dir);
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



        // Uncoil snake - takes droplet and destination
        // Choose head at spot close to destination. Run through rest of body, check for each part if moving it would disconnect body,
        // if yes, go to next part, if no, turn on next electrode for head and turn off electrode for part.
        public static void UncoilSnek(Droplet d, Electrode dest)
        {
            d.Waiting = false;
            // If snake already occupies destination, do nothing.
            if (dest.Occupant != null && dest.Occupant.Equals(d))
            {
                return;
            }

            // Make a temp snake to snek move towards the destination that grows with the shrinkage of the droplet.
            d.SnekMode = true;
            Electrode start = dest.GetClosestElectrodeInList(d.Occupy);
            d.SnekList.AddFirst(start);
            d.CurrentPath = ModifiedAStar.FindPath(d, dest);
            int priorCounter = 0;
            while (d.CurrentPath[0].Item2 != null && d.Occupy.Contains(d.CurrentPath[0].Item1.ElectrodeStep(d.CurrentPath[0].Item2.Value))) //Move the head inside the blob
            {
                priorCounter++;
                MoveTowardDest(d, dest);
                Console.WriteLine("SPECIAL BOARD:");
                Program.C.board.PrintBoardState();
            }
            // Make tree out of blob in order to know what can safely be removed.
            Tree blobTree = BuildTree(d, d.SnekList.ToList(), d.SnekList.First());

            // Make single moves all the way towards the destination.
            do
            {
                // Save last electrode so we can turn it on again.
                // The tree will turn off the correct electrode.
                bool moved = MoveTowardDest(d, dest);

                // If there are still nodes in the tree, it means that the snake is still uncoiling and the electrode that is turned off
                // needs to be controlled by the tree. Otherwise, we are no longer uncoiling and we can just move.
                // "> 1" because the last should not be counted.
                if (blobTree.Nodes.Count > (1 + priorCounter) && moved)
                {
                    MoveOnElectrode(d, start, first: false);
                    // Turn off the right electrode.
                    blobTree.RemoveLeaf();
                }
                else if(d.Waiting == false)
                {
                    d.Waiting = true;
                }

                Program.C.board.PrintBoardState();

            } while (d.CurrentPath != null && d.CurrentPath.Count != 0);

            // Once at dest, whether the snake is fully uncoiled or not, coil the snake again.
            // This avoids long tails being in the way.
            if (!d.Important)
            {
                CoilSnek(d, dest);
            }
            
        }

        // Coil snake
        // Could try doing it without thinking of it as a snake, just a bunch of small droplets moving to be a big one.
        public static void CoilSnek(Droplet d, Electrode? center = null, Apparature? into = null, bool input = false)
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
                if (amount <= 0)
                {
                    goto done;
                }
                Electrode current = activeBlob1[0];
                List<(Electrode, Direction?)> neighbors = current.GetExtendedNeighbors();
                foreach (var item in neighbors)
                {
                    if (CheckLegalMove(d,[item.Item1]) && !seenElectrodes.Contains(item.Item1) && item.Item1.Apparature == into)
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
            if (amount > 0)
            {
                throw new Exception("Not enough space to coil");
            }
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

        internal static void Output(Droplet droplet, Output output)
        {
            Tree snekTree = BuildTree(droplet, [], output.pointers[0]);
            snekTree.RemoveTree();
            MoveOffElectrode(droplet, output.pointers[0]);
            Program.C.board.PrintBoardState();
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
