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

        private static readonly object MoveLock = new object(); //Lock to ensure that only one droplet moves at the exact same time

        public static bool InputDroplet(Droplet d, Input i, int volume, Apparature? destination = null)
        {
            if (d.Inputted)
            {
                Printer.Print("Droplet already inputted");
                throw new Exception("Droplet already inputted");
            }
            d.Inputted = true;
            d.Important = false;
            d.Waiting = false;
            Electrode destElectrode = null;

            d.SetSizes(volume);
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
                        Printer.PrintBoard();
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
            }catch (NullReferenceException ex)
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
                d.CurrentPath.RemoveAt(0);
            }
            else if (d.CurrentPath[0].Item2 != null)
            {
                bool changed = SnekMove(d, d.CurrentPath[0].Item2.Value);
                if (changed)
                {
                    d.TriedMoveCounter = 0;
                    d.CurrentPath.RemoveAt(0);
                }else
                {
                    d.TriedMoveCounter++;
                    moved = false;
                }
            }
            if (d.TriedMoveCounter > 10)
            {
                d.CurrentPath = ModifiedAStar.FindPath(d, destination);
            }
            Printer.PrintBoard();
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
                Printer.Print("Illegal Move");
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
            if (!CheckOtherDroplets(d, temp))
            {
                return false;
            }
            if (!CheckContaminations(d, temp))
            {
                return false;
            }

            return true;
        }
        public static bool CheckOtherDroplets(Droplet d, List<Electrode> temp) // Returns false if there is a contamination that is not compatible with the droplet
        {
            foreach (Electrode e in temp)
            {
                if (!(e.Occupant == null || droplets.Contains(e.Occupant)))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CheckContaminations(Droplet d, List<Electrode> temp) // Returns false if there is a contamination that is not compatible with the droplet
        {
            foreach (Electrode e in temp)
            {
                // Check for contaminants
                foreach (string c in e.GetContaminants())
                {
                    foreach (Droplet d in droplets)
                    {
                        return false;
                    }
                }
            }
            return true;
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
                Printer.Print(d.Name + " waiting for space");
                if (i > 50)
                {
                    Printer.Print(d.Name + " waited for too long");
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


        public static bool SnekMove(Droplet d, Direction dir)
        {
            return SnekMove(d,d.Occupy,dir);
        }

        // Non-protected snake move forward 1
        // Assumes that the list of occupied electrodes are in the form of a snake.
        public static bool SnekMove(Droplet d, List<Electrode> el, Direction dir) // Returns true if movement happened, false if it got stopped
        {
            Printer.Print("SnekMove Toward: " +dir);
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
                Printer.Print("Movement out of bounds");
                return false;
            }

            lock (MoveLock)
            {
                // Do a snekcheck
                // If move is legal, do the thing
                if (CheckLegalMove(d, newHead) && SnekCheck(newHead[0]))
                {

                    Printer.Print("New head: " + newHead[0].ePosX + " " + newHead[0].ePosY);
                    Printer.Print("Old head: " + head.ePosX + " " + head.ePosY);

                    MoveOnElectrode(d, newHead[0]);

                    MoveOffElectrode(d);
                    Printer.Print("Droplet moved");
                    return true;
                }
                else
                {
                    Printer.Print("Droplet not moved");
                    return false;
                }
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
                Printer.Print("SPECIAL BOARD:");
                Printer.PrintBoard();
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

                Printer.PrintBoard();

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

            Printer.PrintBoard();

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
            Printer.PrintBoard();
        }


        public static void MergeMove(Droplet result, List<Droplet> mergers, Electrode mergePoint) // TODO: Can corners be cut?
        {
            // This method assumes that the droplets to be merged are close and ready to move into each other
            // and is just here to bypass the restrictions of the regular move

            // Take volume of all merging droplets and make a space of size corresponding to this volume.
            // Then, have each droplet touching this area and make trees to turn off their pre-merge positions.

            double volume = 0;
            foreach (Droplet d in mergers)
            {
                volume += d.Volume;
            }

            result.SetSizes(volume);

            // Find space.
            List<Electrode> looking = [mergePoint];
            List<Electrode> space = new List<Electrode>();

            while (space.Count < result.Size)
            {
                if (looking.Count == 0)
                {
                    throw new Exception("Not enough space");
                }
                Electrode e = looking[0];
                looking.RemoveAt(0);

                // If the e we are currently looking at is valid, we can look further along it.
                if (CheckPlacement(mergers, [e])){
                    List<(Electrode, Direction)> neighbors = e.GetTrueNeighbors();
                    List<(Electrode, Direction?)> diagonals = e.GetExtendedNeighbors();

                    foreach ((Electrode el, Direction direction) in neighbors)
                    {
                        looking.Add(el);
                    }
                    foreach ((Electrode el, Direction? direction) in diagonals)
                    {
                        looking.Add(el);
                    }

                    space.Add(e);
                }
                // Continue with next electrode in looking
            }

            // Turn on electrodes in found space - also adds e to occupy in result.
            foreach (Electrode e in space)
            {
                MoveOnElectrode(result, e);
            }

            // Make trees of mergers and turn off the electrodes not in the space.
            foreach (Droplet d in mergers)
            {
                Tree tree = new Tree(d, d.Occupy, space, mergePoint);
                tree.RemoveTree();
            }
            // The droplets should now all be in the space.
        }

    }
}
