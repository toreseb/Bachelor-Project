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
            d.MergeReady = false;
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
                    if (d.CurrentPath.Value.path.Count <= 4)
                    {
                        Electrode center = d.SnekList.First();
                        while (size > 0)
                        {
                            CoilSnek(d, center, input: true);
                            size--;
                        }
                        d.MergeReady = true;
                        return true;
                    }
                }
                if (size == 0)
                {
                    d.CurrentPath = ModifiedAStar.FindPath(d, destElectrode);
                }
                while (d.CurrentPath.Value.path.Count > 4)
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
                        d.MergeReady = true;
                        return false;
                    }

                    if (d.CurrentPath.Value.path.Count <= 4)
                    {
                        CoilSnek(d, d.SnekList.First());
                        return true;
                    }
                }
            }
            d.MergeReady = true;
            return true;


        }

        public static Electrode MoveToApparature(Droplet d, Apparature dest)
        {
            Electrode closest = d.GetClosestFreePointer(dest);

            try
            {
                MoveToDest(d, closest);
                CoilSnek(d, closest, into: dest);
            }
            catch (NewWorkException)
            {
                Printer.PrintBoard();
                d.Waiting = false;
                d.MergeReady = true;
                return closest;
            }
            return closest;
        }

        public static void MoveToDest(Droplet d, Electrode destination, List<string>? mergeDroplets = null) //TODO: Make sure that the droplet moves to the destination
        {
            if (d.CurrentPath == null || d.CurrentPath.Value.path.Count == 0 || d.CurrentPath.Value.path[^1].Item1 != destination)
            {
                d.CurrentPath = ModifiedAStar.FindPath(d, destination, mergeDroplets);
            }
            try
            {
                while (d.CurrentPath != null && d.CurrentPath.Value.path.Count > 0)
                {
                    MoveTowardDest(d, destination, mergeDroplets);
                }
            }catch (NullReferenceException ex)
            {
                throw ex;
            }
            Printer.PrintBoard();
        }

        public static Electrode MoveTowardApparature(Droplet d, Apparature dest)
        {
            Electrode closest = d.GetClosestFreePointer(dest);
            MoveTowardDest(d, closest);
            return closest;
        }

        public static (bool, Electrode? movedOffElectrode) MoveTowardDest(Droplet d, Electrode destination, List<string>? mergeDroplets = null, string? splitDroplet = null, bool remove = true) // returns true if droplet physcally moves, false if not
        {
            Thread.Sleep(0);
            if (d.Removed)
            {
                throw new ThreadInterruptedException();
            }

            // For testing
            int preSize = d.Occupy.Count;

            bool moved = true;
            bool removePath = true;
            Electrode? movedOff = null;
            bool changed = false;
            if (d.GetWork().Count > 0 && d.Waiting == true && d.Important == false)
            {
                d.SnekMode = false;
                d.SnekList = [];
                throw new NewWorkException();
            }

            if (!d.SnekMode && d.SnekList.Count == 0)
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException();
                }
                int preCoilSize = d.Occupy.Count;
                UncoilSnek(d, destination, mergeDroplets);
                if (!d.SnekMode)
                {
                    if (preSize != d.Occupy.Count && d.Occupy.Count != d.Size)
                    {
                        throw new ArgumentException("Anomaly in Occupy.Count");
                    }
                    return (true, null);
                }
            }

            if (d.CurrentPath == null || d.CurrentPath.Value.path.Count == 0 || d.TriedMoveCounter > 10)
            {
                if (d.TriedMoveCounter > 10)
                {
                    Console.WriteLine(d.Name + " needed to find a new path");
                    d.TriedMoveCounter = 0;

                }
                d.CurrentPath = ModifiedAStar.FindPath(d, destination, mergeDroplets);
            }
            lock (MoveLock)
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException();
                }
                if (d.CurrentPath.Value.path[0].Item2 == null)
                {
                    d.CurrentPath.Value.path.RemoveAt(0);
                    if (preSize != d.Occupy.Count && d.Occupy.Count != d.Size)
                    {
                        throw new ArgumentException("Anomaly in Occupy.Count");
                    }
                    return (false, null); // Hopefully this should just be null.
                }
                bool legalMove = true; Droplet? occupant = null;
                if (d.SnekMode)
                {
                    try
                    {
                        (legalMove, occupant) = CheckBorder(d, [d.SnekList.First.Value.ElectrodeStep(d.CurrentPath.Value.path[0].Item2.Value)]);
                    }
                    catch (Exception)
                    {

                        legalMove = false; occupant = null;
                    }

                }
                if (d.MergeReady == true || (occupant != null && occupant.MergeReady == true))
                {
                    int a = 2;
                }
                if (d.Occupy.Contains(d.CurrentPath.Value.path[0].Item1.ElectrodeStep(d.CurrentPath.Value.path[0].Item2.Value))) // if it goes through itself
                {
                    moved = false;
                    if (d.SnekList.Contains(d.CurrentPath.Value.path[0].Item1.ElectrodeStep(d.CurrentPath.Value.path[0].Item2.Value)))
                    {
                        d.SnekList.Remove(d.CurrentPath.Value.path[0].Item1.ElectrodeStep(d.CurrentPath.Value.path[0].Item2.Value));
                    }
                    d.SnekList.AddFirst(d.CurrentPath.Value.path[0].Item1.ElectrodeStep(d.CurrentPath.Value.path[0].Item2.Value));
                    int reduced = d.CurrentPath.Value.inside - 1;
                    d.CurrentPath = (d.CurrentPath.Value.path, reduced);
                }
                else if (!legalMove && occupant != null && mergeDroplets != null && mergeDroplets.Contains(occupant.Name) && d.MergeReady == true && occupant.MergeReady == true)
                {
                    Thread.Sleep(0);
                    if (d.Removed)
                    {
                        throw new ThreadInterruptedException();
                    }
                    Direction dir = d.CurrentPath.Value.path[0].Item2.Value;
                    if (d.SnekList.First.Value.ElectrodeStep(dir).ElectrodeStep(dir).Occupant != occupant)
                    {
                        MoveOnElectrode(d, d.SnekList.First.Value.ElectrodeStep(dir));
                    }
                    Merge(d, occupant, d.SnekList.First.Value.ElectrodeStep(dir), mergeDroplets); //TODO: make sure that if a droplet meets 2 droplets in it's borders, it somehow merge with either the first then the second, or both at once
                    Printer.PrintBoard();

                    return (false, null);

                }
                else
                {
                    Thread.Sleep(0);
                    (changed, movedOff) = SnekMove(d, d.CurrentPath.Value.path[0].Item2.Value, splitDroplet: splitDroplet, remove);
                    if (changed)
                    {
                        d.TriedMoveCounter = 0;
                    }
                    else
                    {
                        d.TriedMoveCounter++;
                        removePath = false;
                        moved = false;
                    }
                }

                if (removePath)
                {
                    d.CurrentPath.Value.path.RemoveAt(0);
                }
                if (((remove == true && preSize != d.Occupy.Count) || ((remove == false && changed == true) && preSize+1 != d.Occupy.Count)) && d.Occupy.Count != d.Size)
                {
                    throw new ArgumentException("Anomaly in Occupy.Count");
                }
                return (moved, movedOff);
            }
            
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


        private static (bool legalmove, Droplet? occupant) CheckBorder(Droplet d, List<Electrode> temp, string? source = null)
        {
            return CheckBorder([d], temp, source);
        }


        // Used to check if new droplet position upholds border
        private static (bool legalmove, Droplet? occupant) CheckBorder(List<Droplet> droplets, List<Electrode> temp, string? source = null)
        {
            Droplet? occupant = null;
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
                        occupant = Program.C.board.Electrodes[xCheck, yCheck].Occupant;
                        if (!(occupant == null || droplets.Contains(occupant)))
                        {
                            if (source != null)
                            {
                                if (!occupant.Equals(Program.C.board.Droplets[source]))
                                {
                                    legalMove = false;
                                }
                            }
                            else
                            {
                                legalMove = false;
                                goto destination;
                            }
                        }
                    }
                    
                }
            }
            destination:
            return (legalMove, occupant);
        }

        public static bool CheckEdge(int xPos, int yPos)
        {
            return !(xPos < 0 || xPos >= Program.C.board.GetXElectrodes() || yPos < 0 || yPos >= Program.C.board.GetYElectrodes());
        }

        private static bool CheckPlacement(Droplet d, List<Electrode> temp){
            return CheckPlacement([d], temp);
        }

        private static bool CheckPlacement(List<Droplet> droplets, List<Electrode> temp)
        {
            if (!CheckOtherDroplets(droplets, temp))
            {
                return false;
            }
            if (!CheckContaminations(droplets, temp))
            {
                return false;
            }

            return true;
        }

        public static bool CheckOtherDroplets(Droplet d, List<Electrode> temp){
            return CheckOtherDroplets([d], temp);
        }
        public static bool CheckOtherDroplets(List<Droplet> droplets, List<Electrode> temp) // Returns false if there is a contamination that is not compatible with the droplet
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

        public static bool CheckContaminations(Droplet d, List<Electrode> temp){
            return CheckContaminations([d], temp);
        }

        public static bool CheckContaminations(List<Droplet> droplets, List<Electrode> temp) // Returns false if there is a contamination that is not compatible with the droplet
        {
            foreach (Electrode e in temp)
            {
                // Check for contaminants
                foreach (string c in e.GetContaminants())
                {
                    foreach (Droplet d in droplets)
                    {
                        if (d.Contamintants.Contains(c)){
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static (bool legalmove, Droplet? occupant) CheckLegalMove(Droplet d, List<Electrode> temp, string? source = null)
        {
            return CheckLegalMove([d], temp, source);
        }

        public static (bool legalmove, Droplet? occupant) CheckLegalMove(List<Droplet> droplets, List<Electrode> temp, string? source = null)
        {
            bool legalMove = true;
            (bool borderCheck, Droplet? occupant) = CheckBorder(droplets, temp, source);
            if (!(borderCheck && CheckPlacement(droplets, temp))){
                legalMove = false;
            }

            return (legalMove, occupant);
        }

        public static bool CheckLegalPosition(Droplet d, List<(int, int)> pos, string? source = null)
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
            return CheckLegalMove(d, temp, source).legalmove;
        }


        public static void AwaitLegalMove(Droplet d, List<Electrode> temp, string? source = null)
        {
            int i = 0;
            while (!CheckLegalMove(d, temp, source).legalmove)
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


        public static (bool, Electrode? MovedOffElectrode) SnekMove(Droplet d, Direction dir, string? splitDroplet = null, bool remove = true)
        {
            return SnekMove(d,d.Occupy,dir, splitDroplet: splitDroplet, remove);
        }

        // Non-protected snake move forward 1
        // Assumes that the list of occupied electrodes are in the form of a snake.
        public static (bool, Electrode? MovedOffElectrode) SnekMove(Droplet d, List<Electrode> el, Direction dir, string? splitDroplet = null, bool remove = true) // Returns true if movement happened, false if it got stopped
        {
            Printer.Print(d.Name +" SnekMoves Toward: " +dir);
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
                return (false,null);
            }

            lock (MoveLock)
            {
                // Do a snekcheck
                // If move is legal, do the thing
                if (CheckLegalMove(d, newHead, splitDroplet).legalmove && SnekCheck(newHead[0]))
                {

                    Printer.Print("New head: " + newHead[0].ePosX + " " + newHead[0].ePosY);
                    Printer.Print("Old head: " + head.ePosX + " " + head.ePosY);

                    MoveOnElectrode(d, newHead[0]);

                    
                    Printer.Print("Droplet moved");
                    Electrode movedOffElectrode = null;
                    if (remove)
                    {
                        movedOffElectrode = MoveOffElectrode(d);
                    }
                    return (true,movedOffElectrode);
                }
                else
                {
                    Printer.Print("Droplet not moved");
                    return (false,null);
                }
            }
            
        }
        public static void TakeOverElectrode(Droplet d, Electrode e, bool first = true)
        {
            lock (MoveLock)
            {
                Outparser.Outparser.ElectrodeOn(e);
                if (d.SnekMode)
                {
                    if (e.Occupant != null && e.Occupant != d)
                    {
                        e.Occupant.SnekList.Remove(e);
                    }

                    if (first)
                    {
                        d.SnekList.AddFirst(e);
                    }
                    else
                    {
                        d.SnekList.AddLast(e);
                    }
                }
                if (e.Occupant != null)
                {
                    e.Occupant.Occupy.Remove(e);
                }
                else
                {
                    Outparser.Outparser.ElectrodeOn(e);
                }
                d.Occupy.Add(e);
                e.Occupant = d;
            }
        }

        public static void MoveOnElectrode(Droplet d, Electrode e, bool first = true)
        {
            lock (MoveLock)
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
            
        }

        public static Electrode MoveOffElectrode(Droplet d, Electrode? e = null)
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
            return e;
        }


        // Switch head and tail of snake
        public static void SnekReversal(Droplet d)
        {
            LinkedList<Electrode> copyList = new LinkedList<Electrode>();

            LinkedListNode<Electrode> start = d.SnekList.Last;
            while (start != null)
            {

                copyList.AddLast(start.Value);

                start = start.Previous;
            }

            d.SnekList = copyList;

        }



        // Uncoil snake - takes droplet and destination
        // Choose head at spot close to destination. Run through rest of body, check for each part if moving it would disconnect body,
        // if yes, go to next part, if no, turn on next electrode for head and turn off electrode for part.
        public static void UncoilSnek(Droplet d, Electrode dest, List<string>? mergeDroplets = null)
        {
            d.Waiting = false;

            // For Testing
            int preSize = d.Occupy.Count;
            
            // If snake already occupies destination, coil around dest.
            if (dest.Occupant != null && dest.Occupant.Equals(d))
            {
                d.CurrentPath = null;
                CoilSnek(d, dest);
                if (preSize != d.Occupy.Count && d.Occupy.Count != d.Size)
                {
                    throw new ArgumentException("Anomaly in Occupy.Count");
                }
                return;
            }

            // Make a temp snake to snek move towards the destination that grows with the shrinkage of the droplet.
            d.SnekMode = true;
            Electrode start = dest.GetClosestElectrodeInList(d.Occupy);
            d.SnekList.AddFirst(start);
            d.CurrentPath = ModifiedAStar.FindPath(d, dest);
            int priorCounter = 0;
            int totalExtraAdded = 0;
            while (d.CurrentPath.Value.path[0].Item2 != null && d.CurrentPath.Value.inside > 0) //Move the head inside the blob
            {
                if (d.Occupy.Contains(d.CurrentPath.Value.path[0].Item1.ElectrodeStep(d.CurrentPath.Value.path[0].Item2.Value)))
                {
                    priorCounter++;
                }
                
                (bool physMove, Electrode movedOffElectrode) = MoveTowardDest(d, dest, mergeDroplets);
                if (physMove)
                {
                    MoveOnElectrode(d, movedOffElectrode, first: false) ;
                    totalExtraAdded++;
                }
                Printer.Print("SPECIAL BOARD:");
                Printer.PrintBoard();
            }
            int extraAdded = totalExtraAdded;
            // Make tree out of blob in order to know what can safely be removed.
            Tree blobTree = BuildTree(d, d.SnekList.ToList(), d.SnekList.First());

            // Make single moves all the way towards the destination.
            do
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException();
                }
                
                lock (MoveLock)
                {
                    if (extraAdded > 0)
                    {
                        MoveOffElectrode(d);
                        extraAdded--;
                    }
                    // Save last electrode so we can turn it on again.
                    // The tree will turn off the correct electrode.
                    bool moved;
                    bool needToRemove = true;
                    if (blobTree.Nodes.Count > (1 + priorCounter + totalExtraAdded)){
                        needToRemove = false;
                    }

                    moved = MoveTowardDest(d, dest, mergeDroplets, remove: needToRemove).Item1;


                    
                    // If there are still nodes in the tree, it means that the snake is still uncoiling and the electrode that is turned off
                    // needs to be controlled by the tree. Otherwise, we are no longer uncoiling and we can just move.
                    // "> 1" because the last should not be counted.
                    if (!needToRemove && moved)
                    {
                        d.MergeReady = true;
                        // Turn off the right electrode.
                        blobTree.RemoveLeaf();
                        if (blobTree.Nodes.Count <= (1 + priorCounter + totalExtraAdded))
                        {
                            d.MergeReady = true;
                        }
                    }
                    else if (d.Waiting == false)
                    {
                        d.MergeReady = true;
                        d.Waiting = true;
                    }

                    if (d.Occupy.Contains(dest) && !d.Important)
                    {
                        int preCoilSize = d.Occupy.Count;
                        CoilSnek(d, dest);
                        return;
                    }
                }

                Printer.PrintBoard();
            } while (d.CurrentPath != null && d.CurrentPath.Value.path.Count != 0);

            // Once at dest, whether the snake is fully uncoiled or not, coil the snake again.
            // This avoids long tails being in the way.
            if (d.Occupy.Contains(dest) && !d.Important)
            {
                CoilSnek(d, dest);
            }
            d.MergeReady = true;
            if (preSize != d.Occupy.Count && d.Occupy.Count != d.Size)
            {
                throw new ArgumentException("Anomaly in Occupy.Count");
            }
        }

        // Coil snake
        // Could try doing it without thinking of it as a snake, just a bunch of small droplets moving to be a big one.
        public static void CoilSnek(Droplet d, Electrode? center = null, Apparature? into = null, bool input = false)
        {
            d.MergeReady = false;

            // For Testing
            int preSize = d.Occupy.Count;

            if (center == null)
            {
                center = d.SnekList.First();
            }
            d.SnekMode = false;
            d.SnekList.Clear();
            
            int amount = input ? d.Occupy.Count : d.Occupy.Count -1; // -1 because the center is not in the list 0 if it inputs new value
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
                if (d.Removed)
                {
                    throw new ThreadInterruptedException("Thread has been interrupted");
                }
                if (amount <= 0)
                {
                    goto done;
                }
                Electrode current = activeBlob1[0];
                List<(Electrode, Direction?)> neighbors = current.GetExtendedNeighbors();
                foreach (var item in neighbors)
                {
                    if (CheckLegalMove(d,[item.Item1]).legalmove && !seenElectrodes.Contains(item.Item1) && (item.Item1.Apparature == into || (into == null && item.Item1.Occupant == d)))
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
            if (d.Removed)
            {
                throw new ThreadInterruptedException("Thread has been interrupted");
            }
            if (amount > 0)
            {
                Printer.Print("Not enough space to coil");
                Printer.PrintBoard();
                Thread.Sleep(1000);
                throw new IllegalMoveException("Not enough space to coil");
            }
            Tree snekTree = BuildTree(d, newBlob, center);
            
            lock (MoveLock)
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException("Thread has been interrupted");
                }
                foreach (var item in newBlob)
                {


                    if (!d.Occupy.Contains(item))
                    {
                        MoveOnElectrode(d, item);
                    }


                }
                
                
            }
            snekTree.RemoveTree(into);

            Printer.PrintBoard();
            d.MergeReady = true;

            if ((input == false && preSize != d.Occupy.Count && d.Occupy.Count != d.Size) || (input == true && preSize != d.Occupy.Count-1))
            {
                throw new ArgumentException("Anomaly in Occupy.Count");
            }

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

        public static Electrode MergeCalc(List<string> inputDroplets, Droplet droplet, UsefullSemaphore done) //Release 1 to done when done
        {

            done.TryRelease(inputDroplets.Count);
            return droplet.nextElectrodeDestination;
        }

        public static void Merge(Droplet d, Droplet mergeDroplet, Electrode center, List<string> mergeDroplets)
        {

            d.SetSizes(d.Volume+ mergeDroplet.Volume);

            int sizeDif = (d.Occupy.Count + mergeDroplet.Occupy.Count+1) - (d.Size);
            d.TakeOver(mergeDroplet);
            MoveOnElectrode(d, center);
            Tree mergeTree = BuildTree(d, [], center);
            while (sizeDif > 0)
            {
                mergeTree.RemoveLeaf();
                sizeDif--;
            }
            
            d.SnekList = [];
            d.SnekMode = false;
            if (d.nextElectrodeDestination != null)
            {
                d.CurrentPath = ModifiedAStar.FindPath(d, d.nextElectrodeDestination, mergeDroplets);
            }
            else
            {
                d.CurrentPath = null;
            }
            Printer.Print(d.Name + " and " + mergeDroplet.Name + " has been merged");

        }

        public static void MergeMove(Droplet result, List<Droplet> mergers, Electrode mergePoint) // TODO: Can corners be cut?
            // TODO: Maybe CheckLegalMove instead
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
                if (CheckLegalMove(mergers, [e]).legalmove){
                    space.Add(e);

                    List<(Electrode, Direction?)> neighbors = e.GetExtendedNeighbors();

                    foreach ((Electrode el, Direction? direction) in neighbors)
                    {
                        looking.Add(el);
                    }
                }
                // Continue with next electrode in looking
            }

            // Turn on electrodes in found space - also adds e to occupy in result.
            foreach (Electrode e in space)
            {
                MoveOnElectrode(result, e);

                foreach (Droplet d in mergers)
                {
                    if (d.Occupy.Contains(e))
                    {
                        d.Occupy.Remove(e);
                    }
                }
            }

            Printer.PrintBoard();

            // Make trees of mergers and turn off the electrodes not in the space.
            foreach (Droplet d in mergers)
            {
                Tree tree = new Tree(d, d.Occupy, space, mergePoint);
                tree.RemoveTree();

                while(d.Occupy.Count > 0)
                {
                    MoveOffElectrode(d, d.Occupy[0]);
                }
                // TODO: Remove d from board

                Printer.PrintBoard();
            }
            // The droplets should now all be in the space.
        }

        
        public static void splitDroplet(Droplet source, Dictionary<string, double> ratios, Dictionary<string, UsefullSemaphore> dropSem)
        {
            // For loop to split the droplets out one by one.
            // Makes a snake of appropriate size a la uncoil and cuts it off.
            // After cutoff, the 'new' droplet is given the task of moving.

            Printer.Print("I am splitting!");

            source.Waiting = false;

            foreach ((string dName, double ratio) in ratios)
            {
                Droplet d = Program.C.board.Droplets[dName];

                // Find electrode in source closest to where splitter needs to go. nextElectrodeDestination is not set, so we do it here.
                d.SetNextElectrodeDestination(); // Frick.. To set this, I need the starting point, but I need this to find the starting point.
                // I am unsure how to fix this.

                Printer.Print("I have set electrode destination for " + d.Name); // We are never getting to this

                Electrode dest = d.nextElectrodeDestination;
                Electrode start = dest.GetClosestElectrodeInList(source.Occupy);

                Printer.Print("I have found the start point for " + d.Name);

                // Make tree from the closest electrode to dest.
                Tree sourceTree = BuildTree(source, [], start);

                d.SnekMode = true;
                d.SnekList = [];

                // Find path split droplet needs to follow.
                TakeOverElectrode(d, start);


                d.CurrentPath = ModifiedAStar.FindPath(d, dest, splitDroplet: source.Name);

                Printer.PrintBoard();

                Thread.Sleep(100);

                // Find number of electrodes to occupy
                d.SetSizes(source.Volume * ratio / 100);

                // Move out the amount of spaces this splitter needs
                int i = 1;
                while (i < d.Size)
                {
                    if (d.CurrentPath.Value.path.Count > Constants.DestBuff)
                    {
                        // Try to make move toward dest
                        bool moved = MoveTowardDest(d, dest, splitDroplet: source.Name).Item1;

                        if (moved)
                        {
                            // Turn on the electrode MoveTowardDest turned off.
                            MoveOnElectrode(d, start, first: false);
                            // Turn off the right electrode.
                            sourceTree.RemoveLeaf();

                            i++;
                        }
                        else if (d.Waiting == false)
                        {
                            d.Waiting = true;
                        }
                    }
                    else
                    {
                        int remainder = d.Size - i;

                        // Coil at dest and remove from source
                        CoilWithoutRemoval(d, remainder); 

                        // Remove the corresponding electrodes from the source tree
                        for (int j = 0; j < remainder; j++)
                        {
                            sourceTree.RemoveLeaf();
                        }

                        // Normal coil to suck up tail
                        CoilSnek(d, dest);

                        i += remainder;
                    }
                    

                    // TODO: What if destination is not far enough to have the snake completely split from the source?
                }

                // Move all the way away
                // I can use CheckBoarder for it and just give it the current position of the droplet that was just split off.
                while (!CheckBorder(d, d.Occupy).Item1)
                {
                    if (d.CurrentPath.Value.path.Count > Constants.DestBuff)
                    {
                        bool moved = MoveTowardDest(d, dest, splitDroplet: source.Name).Item1;

                        if (!moved && d.Waiting == false)
                        {
                            d.Waiting = true;
                        }
                    }
                    else
                    {
                        CoilSnek(d, into: d.nextDestination);
                    }
                }


                d.Waiting = true;
                d.Important = false;

                // Give d the task of moving.
                dropSem[d.Name].TryReleaseOne();

                Printer.PrintBoard();
                Printer.Print(dName + " DONE!!");
            }

            source.RemoveFromBoard();
        }
        
        /// <summary>
        /// Coils around head without removing from tail.
        /// Used for input and split.
        /// </summary>
        /// <param name="d"></param>
        public static void CoilWithoutRemoval(Droplet d, int remainder)
        {
            List<Electrode> workingList = [d.Occupy[0]];
            // Coil without removing
            for (int i = 0; i < remainder; i++)
            {
                Electrode e = workingList[0];
                workingList.Remove(e);

                List<(Electrode, Direction?)> neighbors = e.GetExtendedNeighbors();

                foreach ((Electrode n, Direction? dir) in neighbors)
                {
                    if (n.Occupant == null)
                    {
                        MoveOnElectrode(d,n, first: false);
                        workingList.Add(n);
                    }
                }
            }
        }
    }
}
