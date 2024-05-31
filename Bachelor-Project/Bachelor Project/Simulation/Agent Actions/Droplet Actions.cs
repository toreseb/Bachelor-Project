﻿using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Antlr4.Runtime.Atn.SemanticContext;


namespace Bachelor_Project.Simulation.Agent_Actions
{
    // This class contains the more basic movements and actions an agent can take.
    public static class Droplet_Actions
    {

        public static readonly object MoveLock = new object(); //Lock to ensure that only one droplet moves at the exact same time

        public static void InputDroplet(Droplet d, Input i, int volume, Apparature? destination = null)
        {

            d.MergeReady = false;
            if (d.Inputted)
            {
                Printer.PrintLine("Droplet already inputted");
                throw new Exception("Droplet already inputted");
            }
            d.Inputted = true;
            d.Important = false;
            d.Waiting = false;
            Electrode destElectrode = null;

            d.SetSizes(volume);
            if (d.Volume < 6)
            {
                throw new ArgumentException("droplet too small");
            }
            int size = d.Size;
            
            lock (MoveLock)
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException();
                }
                AwaitLegalMove(d, i.pointers);
                if (destination != null)
                {
                    d.SnekMode = true;
                    MoveOnElectrode(d, i.pointers[0]);
                    size--;
                    destElectrode = d.GetClosestFreePointer(destination);
                }
            }
            

            if (destination == null)
            {
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

                    (bool moved, Electrode? _) = MoveTowardDest(d, destElectrode,remove: false);
                    if (moved)
                    {
                        size--;
                    }
                    
                    if (d.CurrentPath.Value.path.Count <= Constants.DestBuff)
                    {
                        CoilWithoutRemoval(d, size);
                        CoilSnek(d, i.pointers[0]);

                        d.MergeReady = true;
                        return;
                    }
                }
                
            }
            Program.C.RemovePath(d);
            d.MergeReady = true;
        }

        public static bool MixDroplet(Droplet d, string pattern) //TODO: Remake to make sure that droplet interference makes it try a different direction, not give up
        {
            Printer.PrintLine(d.Name + " : MIXING");
            d.Important = true;
            int retryCounter = 0;
            Program.C.RemovePath(d);
            while (true)
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
                    if (CheckLegalMove(d, temp).legalmove)
                    {
                        for (int i = 0; i < Constants.MixAmount; i++)
                        {
                            foreach (var item in directions)
                            {
                                MoveDroplet(d, item);
                                Printer.PrintBoard();
                            }
                        }

                        return true;
                    }
                    else
                    {
                        if (retryCounter > 10)
                        {
                            throw new IllegalMoveException("No space for mixing");
                        }
                        Thread.Sleep(100);
                        retryCounter++;
                    }

                }
                else
                {
                    if (retryCounter > 10)
                    {
                        throw new IllegalMoveException("No space for mixing");
                    }
                    Thread.Sleep(100);
                    retryCounter++;
                }

            }


        }

        public static Electrode MoveToApparature(Droplet d, Apparature dest)
        {
            Electrode closest = d.GetClosestFreePointer(dest);

            

            try
            {
                MoveToDest(d, closest);
                CoilSnek(d, closest, app: dest);
            }
            catch (NewWorkException)
            {
                Printer.PrintBoard();
                d.GoAmorphous();
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
                Program.C.RemovePath(d);
            }catch (NullReferenceException)
            {
                throw;
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
                if (CheckParity(d, preSize, mergeDroplets))
                {
                    throw new ArgumentException("Anomaly in Occupy.Count");
                }
                int preCoilSize = d.Occupy.Count;
                UncoilSnek(d, destination, mergeDroplets);
                int postCoilSize = d.Occupy.Count;
                if (!d.SnekMode)
                {
                    if (CheckParity(d, preSize, mergeDroplets))
                    {
                        throw new ArgumentException("Anomaly in Occupy.Count");
                    }
                    if (CheckDropletHeldTogetherParity(d))
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
                    Printer.PrintLine(d.Name + " needed to find a new path");
                    d.TriedMoveCounter = 0;
                    d.TriedResetCounter++;
                    if (d.TriedResetCounter > 10)
                    {
                        throw new ArgumentException("Reset too many times");
                    }
                    
                    Thread.Sleep(50);

                }
                
                Program.C.RemovePath(d);
                d.CurrentPath = ModifiedAStar.FindPath(d, destination, mergeDroplets);
                Printer.PrintBoard();
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
                    if (CheckDropletHeldTogetherParity(d))
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
                Electrode el = d.CurrentPath.Value.path[0].Item1.ElectrodeStep(d.CurrentPath.Value.path[0].Item2.Value);
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
                    CoilSnek(d, mergeDroplets: mergeDroplets);
                    Printer.PrintBoard();

                    if (CheckParity(d, d.Occupy.Count, mergeDroplets))
                    {
                        throw new IllegalMoveException("Broken parity");
                    }


                    return (false, null);

                }
                else
                {
                    if (CheckDropletHeldTogetherParity(d))
                    {
                        throw new ArgumentException("Anomaly in Occupy.Count");
                    }
                    Thread.Sleep(0);
                    (changed, movedOff) = SnekMove(d, d.CurrentPath.Value.path[0].Item2.Value, splitDroplet: splitDroplet, remove);
                    if (changed)
                    {
                        d.TriedMoveCounter = 0;
                        d.TriedResetCounter = 0;
                    }
                    else
                    {
                        d.TriedMoveCounter++;
                        removePath = false;
                        moved = false;
                    }
                    if (CheckDropletHeldTogetherParity(d))
                    {
                        throw new ArgumentException("Anomaly in Occupy.Count");
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
                if (CheckDropletHeldTogetherParity(d))
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

            (int xChange, int yChange) = DirectionUtils.GetXY(dir);

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
                List<Electrode> OldOccupy = new(d.Occupy);
                // Turn off all old electrodes second (which are not also new)
                foreach (Electrode e in OldOccupy)
                {
                    if (!temp.Contains(e))
                    {
                        MoveOffElectrode(d, e);
                    }
                }
            }
            else
            {
                //throw new IllegalMoveException();
                Printer.PrintLine("Illegal Move");
            }
        }


        private static (bool legalmove, Droplet? occupant) CheckBorder(Droplet d, List<Electrode> temp, List<string>? mergeDroplets = null, string? source = null)
        {
            return CheckBorder([d], temp, mergeDroplets: mergeDroplets, source: source);
        }


        // Used to check if new droplet position upholds borders
        private static (bool legalmove, Droplet? occupant) CheckBorder(List<Droplet> droplets, List<Electrode> temp, List<string>? mergeDroplets = null, string? source = null)
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
                    if (CheckBoardEdge(xCheck, yCheck))
                    {
                        occupant = Program.C.board.Electrodes[xCheck, yCheck].Occupant;
                        if (occupant != null && !droplets.Contains(occupant))
                        {
                            if (source != null || mergeDroplets != null)
                            {
                                if (occupant.Name != source || (mergeDroplets != null && !mergeDroplets.Contains(occupant.Name)))
                                {
                                    legalMove = false;
                                }
                            }
                            else
                            {
                                legalMove = false;
                                return (legalMove, occupant);
                            }
                        }
                    }
                    
                }
            }
            return (legalMove, occupant);
        }
        /// <summary>
        /// Checks if the elctrode is in the board of an apparature.
        /// </summary>
        /// <param name="el"></param>
        /// <param name="alreadyOnApp"></param>
        /// <param name="coilIntoApp"></param>
        /// <returns></returns>
        private static bool CheckApparatureBorders(Electrode el, List<Apparature?> alreadyOnApp, Apparature? coilIntoApp)
        {
            if (el.Apparature != null && el.Apparature != coilIntoApp)
            {
                if (!alreadyOnApp.Contains(el.Apparature))
                {
                    return false;
                }
            }
            List<(Electrode, Direction?)> border = el.GetExtendedNeighbors();

            foreach ((Electrode cEl, Direction? _) in border)
            {
                if (cEl.Apparature != null && cEl.Apparature != coilIntoApp)
                {
                    if (!alreadyOnApp.Contains(cEl.Apparature))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool CheckBoardEdge(int xPos, int yPos)
        {
            return !(xPos < 0 || xPos >= Program.C.board.GetXElectrodes() || yPos < 0 || yPos >= Program.C.board.GetYElectrodes());
        }

        private static bool CheckPlacement(Droplet d, List<Electrode> temp, List<string>? mergeDroplets = null)
        {
            return CheckPlacement([d], temp, mergeDroplets: mergeDroplets);
        }

        private static bool CheckPlacement(List<Droplet> droplets, List<Electrode> temp, List<string>? mergeDroplets = null, string? splitDroplet = null)
        {
            if (!CheckOtherDroplets(droplets, temp, mergeDroplets, splitDroplet))
            {
                return false;
            }
            if (!CheckContaminations(droplets, temp))
            {
                return false;
            }

            return true;
        }

        public static bool CheckOtherDroplets(Droplet d, List<Electrode> temp, List<string>? mergeDroplets = null)
        {
            return CheckOtherDroplets([d], temp, mergeDroplets: mergeDroplets);
        }
        public static bool CheckOtherDroplets(List<Droplet> droplets, List<Electrode> temp, List<string>? mergeDroplets = null, string? splitDroplet = null) // Returns false if there is a contamination that is not compatible with the droplet
        {
            foreach (Electrode e in temp)
            {
                if (!(e.Occupant == null || droplets.Contains(e.Occupant) || (mergeDroplets != null && mergeDroplets.Contains(e.Occupant.Name)) || (splitDroplet != null && e.Occupant.Name == splitDroplet)))
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
                lock (e.GetContaminants())
                {
                    foreach (string c in e.GetContaminants())
                    {
                        foreach (Droplet d in droplets)
                        {
                            if (d.Contamintants.Contains(c))
                            {
                                return false;
                            }
                        }
                    }
                }
                
            }
            return true;
        }

        public static (bool legalmove, Droplet? occupant) CheckLegalMove(Droplet d, List<Electrode> temp, List<string>? mergeDroplets = null, string? source = null, bool splitPlacement = false)
        {
            if (splitPlacement)
            {
                return CheckLegalMove([d], temp, mergeDroplets: mergeDroplets, source: source, splitPlacement: splitPlacement);
            }
            return CheckLegalMove([d], temp, mergeDroplets: mergeDroplets, source: source) ;
        }

        public static (bool legalmove, Droplet? occupant) CheckLegalMove(List<Droplet> droplets, List<Electrode> temp, List<string>? mergeDroplets = null, string? source = null, bool splitPlacement = false)
        {
            bool legalMove = true;
            (bool borderCheck, Droplet? occupant) = CheckBorder(droplets, temp, mergeDroplets, source);
            if (!(borderCheck && CheckPlacement(droplets, temp, mergeDroplets, splitPlacement ? null: source))){
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
            return CheckLegalMove(d, temp, source: source).legalmove;
        }

        
        

        public static void AwaitLegalMove(Droplet d, List<Electrode> temp, string? source = null)
        {
            int i = 0;
            while (!CheckLegalMove(d, temp, source: source).legalmove)
            {
                Printer.PrintLine(d.Name + " waiting for space");
                if (i > 50)
                {
                    Printer.PrintLine(d.Name + " waited for too long");
                    throw new Exception("Droplet waited for too long");
                }
                Monitor.Exit(MoveLock);
                Thread.Sleep(100);
                Monitor.Enter(MoveLock);
                i++;
            }
        }



        private static bool SnekCheck(Droplet d, Electrode newHead, string? source = null)
        {
            if (newHead.Occupant == null || newHead.Occupant.Name == source)
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
            Printer.PrintLine(d.Name +" SnekMoves Toward: " +dir);
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

            (int x, int y) = DirectionUtils.GetXY(dir);
            

            try
            {
                newHead.Add(Program.C.board.Electrodes[head.ePosX + x, head.ePosY + y]);
            }
            catch (Exception)
            {
                Printer.PrintLine("Movement out of bounds");
                return (false,null);
            }
            if (newHead[0].Occupant == d)
            {
                d.GoAmorphous();
                return (false, null);
            }
            lock (MoveLock)
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException();
                }
                // Do a snekcheck
                // If move is legal, do the thing
                if (CheckLegalMove(d, newHead, source: splitDroplet).legalmove && SnekCheck(d, newHead[0], source: splitDroplet))
                {

                    Printer.PrintLine("New head: " + newHead[0].ePosX + " " + newHead[0].ePosY);
                    Printer.PrintLine("Old head: " + head.ePosX + " " + head.ePosY);

                    if (splitDroplet != null && Program.C.board.Droplets[splitDroplet].Occupy.Contains(newHead[0]))
                    {
                        TakeOverElectrode(d, newHead[0]);
                    }
                    else
                    {
                        MoveOnElectrode(d, newHead[0]);
                    }
                    

                    Printer.PrintLine("Droplet moved");
                    Electrode movedOffElectrode = null;
                    if (remove)
                    {
                        movedOffElectrode = MoveOffElectrode(d);
                    }
                    if (CheckDropletHeldTogetherParity(d))
                    {
                        throw new ArgumentException("Anomaly in Occupy.Count");
                    }
                    return (true,movedOffElectrode);
                }
                else
                {
                    
                    Printer.PrintLine("Droplet not moved");
                    return (false,null);
                }
            }
            
        }
        public static void TakeOverElectrode(Droplet d, Electrode e, bool first = true)
        {
            lock (MoveLock)
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException();
                }
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
                    Outparser.Outparser.ElectrodeOn(e, d: d);
                }
                d.Occupy.Add(e);
                e.Occupant = d;
            }
        }

        public static void MoveOnElectrode(Droplet d, Electrode e, bool first = true)
        {
            lock (MoveLock)
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException();
                }
                Outparser.Outparser.ElectrodeOn(e, d: d);
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

            Outparser.Outparser.ElectrodeOff(e, d: d);
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
                Program.C.RemovePath(d);
                CoilSnek(d, dest);
                if (CheckParity(d,preSize, mergeDroplets))
                {
                    throw new ArgumentException("Anomaly in Occupy.Count");
                }
                return;
            }

            if (CheckDropletHeldTogetherParity(d))
            {
                throw new ArgumentException("Anomaly in Occupy.Count");
            }

            // Make a temp snake to snek move towards the destination that grows with the shrinkage of the droplet.
            d.SnekMode = true;
            Electrode start = dest.GetClosestElectrodeInList(d.Occupy);
            d.SnekList.AddFirst(start);
            d.CurrentPath = ModifiedAStar.FindPath(d, dest, mergeDroplets: mergeDroplets);
            int priorCounter = 0;
            int totalExtraAdded = 0;

            LinkedList<Electrode> oldList = new(d.SnekList);
            if (CheckDropletHeldTogetherParity(d))
            {
                throw new ArgumentException("Anomaly in Occupy.Count");
            }
            while (d.CurrentPath.Value.path[0].Item2 != null && d.CurrentPath.Value.inside > 0) //Move the head inside the blob
            {
                if (d.Occupy.Contains(d.CurrentPath.Value.path[0].Item1.ElectrodeStep(d.CurrentPath.Value.path[0].Item2.Value)))
                {
                    priorCounter++;
                }
                
                (bool physMove, Electrode? movedOffElectrode) = MoveTowardDest(d, dest, mergeDroplets, remove: false);
                if (physMove)
                {
                    totalExtraAdded++;
                }
                Printer.PrintLine("SPECIAL BOARD:");
                Printer.PrintBoard();
            }
            if (CheckDropletHeldTogetherParity(d))
            {
                throw new ArgumentException("Anomaly in Occupy.Count");
            }
            int extraAdded = totalExtraAdded;
            LinkedList<Electrode> oldList2 = new(d.SnekList);
            int needToTreeRemove = (d.Occupy.Count - d.SnekList.Count) - totalExtraAdded;
            // Make tree out of blob in order to know what can safely be removed.
            Tree blobTree = BuildTree(d, d.SnekList.ToList(), d.SnekList.First());

            // Make single moves all the way towards the destination.
            do
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException();
                }
                if (extraAdded > 0)
                {
                    blobTree.RemoveLeaf();
                    if (CheckDropletHeldTogetherParity(d))
                    {
                        throw new ArgumentException("Anomaly in Occupy.Count");
                    }
                    extraAdded--;
                }
                // Save last electrode so we can turn it on again.
                // The tree will turn off the correct electrode.
                bool moved;
                bool needToRemove = true;
                if (needToTreeRemove > 0)
                {
                    needToRemove = false;
                    needToTreeRemove--;
                }
                if (CheckDropletHeldTogetherParity(d, mergeDroplets))
                {
                    throw new ArgumentException("Anomaly in Occupy.Count");
                }
                moved = MoveTowardDest(d, dest, mergeDroplets, remove: needToRemove).Item1;

                if (CheckDropletHeldTogetherParity(d, mergeDroplets))
                {
                    throw new ArgumentException("Anomaly in Occupy.Count");
                }

                // If there are still nodes in the tree, it means that the snake is still uncoiling and the electrode that is turned off
                // needs to be controlled by the tree. Otherwise, we are no longer uncoiling and we can just move.
                // "> 1" because the last should not be counted.
                if (!needToRemove && moved)
                {
                    d.MergeReady = true;
                    // Turn off the right electrode.
                    Electrode testSee = blobTree.RemoveLeaf();
                    if (CheckDropletHeldTogetherParity(d, mergeDroplets))
                    {
                        throw new ArgumentException("Anomaly in Occupy.Count");
                    }
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
                    Program.C.RemovePath(d);
                    int preCoilSize = d.Occupy.Count;
                    CoilSnek(d, dest);
                    if (CheckParity(d, preSize, mergeDroplets))
                    {
                        throw new ArgumentException("Anomaly in Occupy.Count");
                    }
                    return;
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
            if (CheckParity(d, preSize, mergeDroplets))
            {
                throw new ArgumentException("Anomaly in Occupy.Count");
            }
        }

        // Coil snake
        // Could try doing it without thinking of it as a snake, just a bunch of small droplets moving to be a big one.
        public static void CoilSnek(Droplet d, Electrode? center = null, Apparature? app = null, bool input = false, List<string>? mergeDroplets = null, bool ignoreBorders = false, bool coiledAgain = false)
        {
            d.MergeReady = false;

            Program.C.RemovePath(d);
            // For Testing
            int preSize = d.Occupy.Count;
            int preDSize = d.Size;
            List<Electrode> testlist = new(d.Occupy);

            bool coilAgain = false; // If the center found is still in the border, coil again and the center is outside the border

            if ((!(input || coiledAgain) && CheckParity(d, preSize, mergeDroplets)) || CheckDropletHeldTogetherParity(d, mergeDroplets))
            {
                throw new ArgumentException("Anomaly in Occupy.Count");
            }
            lock (d)
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException("Thread has been interrupted");
                }

                if (d.SnekList.Count > 0)
                {
                    center ??= d.SnekList.First();
                }
                else if (d.Occupy.Count > 0)
                {
                    center ??= d.Occupy[0];
                }
                List<Electrode> currentElectrodes = [];
                List<Electrode> seenElectrodes1 = [];
                Electrode? cBorderEl = null;
                if (center != null)
                {
                    currentElectrodes = [center];
                    seenElectrodes1 = [center];
                }
                Electrode oldCenter = center;
                center = null;

                while(!(input ||app != null || oldCenter.Apparature != null) && currentElectrodes.Count > 0)
                {
                    Electrode cElectrode = currentElectrodes[0];

                    if (CheckApparatureBorders(cElectrode, [], app))
                    {
                        center = cElectrode;
                        break;
                    }else if (cElectrode.Apparature == null)
                    {
                        cBorderEl = cElectrode;
                    }

                    List<(Electrode, Direction)> neighbors = cElectrode.GetTrueNeighbors();

                    foreach ((Electrode nEl, Direction _) in neighbors)
                    {
                        if (nEl.Occupant == d && !seenElectrodes1.Contains(nEl))
                        {
                            currentElectrodes.Add(nEl);
                            seenElectrodes1.Add(nEl);
                        }
                    }
                    currentElectrodes.Remove(cElectrode);
                }
                if (center == null && cBorderEl != null)
                {
                    center = cBorderEl;
                    coilAgain = true;
                }
                else center ??= oldCenter;
                int a = 2;

            }
            

            

            d.SnekMode = false;
            d.SnekList.Clear();
            
            int amount = input ? d.Occupy.Count : d.Occupy.Count -1; // -1 because the center is not in the list 0 if it inputs new value
            if (amount == 0 && input)
            {
                MoveOnElectrode(d, center);
            }


            int totalAmount = amount;
            
            
            
            List<Electrode> newBlob = [center];
            List<Electrode> activeBlob1 = [center];
            List<Electrode> seenElectrodes = [center];

            List<Apparature> alreadyOnApp = [];

            while(activeBlob1.Count > 0 && amount > 0)
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException("Thread has been interrupted");
                }
                Electrode current = activeBlob1[0];
                if (current.Apparature != null)
                {
                    alreadyOnApp.Add(current.Apparature);
                }
                List<(Electrode, Direction)> trueNeighbors = current.GetTrueNeighbors(); ; // This is causing some issues TODO: Make sure this always works
                List<Direction> foundNeighbors = [];
                foreach ((Electrode el, Direction dir) in trueNeighbors)
                {
                    if (!(ignoreBorders || input) && !CheckApparatureBorders(el, alreadyOnApp, app))
                    {
                        continue;
                    }
                    
                    if (CheckLegalMove(d,[el]).legalmove && !seenElectrodes.Contains(el) && (app != null && ((app.CoilInto && el.Apparature == app)||(!app.CoilInto)) || (app == null && (el.Occupant == d || el.Apparature == null))))
                    {
                        activeBlob1.Add(el);
                        seenElectrodes.Add(el);
                        newBlob.Add(el);
                        foundNeighbors.Add(dir);
                        amount--;
                        if (amount <= 0)
                        {
                            break;
                        }
                    }
                }
                if (amount > 0)
                {
                    List<Electrode> extendedNeighbors = current.GetExtendedNeighborsFromTrue(foundNeighbors, seenElectrodes); ; // This is causing some issues TODO: Make sure this always works

                    foreach (Electrode el in extendedNeighbors)
                    {
                        if (!(ignoreBorders || input) && !CheckApparatureBorders(el, alreadyOnApp, app))
                        {
                            continue;
                        }

                        if (!seenElectrodes.Contains(el) && CheckLegalMove(d, [el]).legalmove && (app != null && ((app.CoilInto && el.Apparature == app) || (!app.CoilInto)) || (app == null && (el.Occupant == d || el.Apparature == null))))
                        {
                            activeBlob1.Add(el);
                            seenElectrodes.Add(el);
                            newBlob.Add(el);
                            amount--;
                            if (amount <= 0)
                            {
                                break;
                            }
                        }
                    }
                }
                


                activeBlob1.Remove(current);
            }

            if (d.Removed)
            {
                throw new ThreadInterruptedException("Thread has been interrupted");
            }
            if (amount > 0)
            {
                Printer.PrintLine("Not enough space to coil");
                if (!ignoreBorders)
                {
                    CoilSnek(d, input: input, mergeDroplets: mergeDroplets, ignoreBorders: true, coiledAgain: true);
                    return;
                }
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
                snekTree.RemoveTree(app != null && app.CoilInto);
            }

            if (coilAgain)
            {
                if (!coiledAgain)
                {
                    CoilSnek(d, mergeDroplets: mergeDroplets, coiledAgain: true);
                }
                
            }

            Printer.PrintBoard();
            d.MergeReady = true;

            if ((!(input || coiledAgain) && CheckParity(d,preSize, mergeDroplets)) || (input == true && preSize != d.Occupy.Count-1))
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
            try
            {
                Tree snekTree = BuildTree(droplet, [], output.pointers[0]);
                snekTree.RemoveTree();
                droplet.RemoveFromBoard();
                Printer.PrintBoard();
            }
            catch (Exception e)
            {
                int a = 2;
            }
            
        }

        public static Electrode MergeCalc(List<string> inputDroplets, Droplet droplet, UsefulSemaphore done) //Release 1 to done when done
        {

            

            Electrode trueDestination = droplet.nextDestination.pointers[0];

            double x = 0, y = 0;
            int total = 0;
            foreach (var item in inputDroplets)
            {
                Droplet cD = Program.C.board.Droplets[item];
                if (cD.Occupy.Count == 0)
                {
                    done.TryRelease(inputDroplets.Count);
                    return trueDestination;
                }
                Electrode randomEl = cD.Occupy[0]; // A random electrode, don't think more thought is necessary
                x += randomEl.ePosX;
                y += randomEl.ePosY;
                total++;
            }

            x = x / total;
            y = y / total;

            x = (int) x;
            y = (int) y;

            Electrode averageEl = Program.C.board.Electrodes[(int)x, (int)y];



            done.TryRelease(inputDroplets.Count);
            return averageEl; //TODO: update to find a better spot to merge
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
            Program.C.RemovePath(d);
            Printer.PrintLine(d.Name + " and " + mergeDroplet.Name + " has been merged into " + d.Name);

        }

        /// <summary>
        /// This is discontinued, and no longer is used.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="mergers"></param>
        /// <param name="mergePoint"></param>
        /// <exception cref="Exception"></exception>

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

        
        public static void SplitDroplet(Droplet source, Dictionary<string, double> ratios, Dictionary<string, UsefulSemaphore> dropSem)
        {
            // For loop to split the droplets out one by one.
            // Makes a snake of appropriate size a la uncoil and cuts it off.
            // After cutoff, the 'new' droplet is given the task of moving.

            Printer.PrintLine(source.Name +" is splitting!");

            source.Waiting = false;

            bool fixStart = false;
            Program.C.RemovePath(source);

            foreach ((string dName, double ratio) in ratios)
            {
                Droplet d = Program.C.board.Droplets[dName];
                d.Waiting = false;
                fixStart = true;

                // Wait for AwaitSplitWork to SetupDestinations for droplet
                dropSem[dName].WaitOne();

                // Find electrode in source closest to where splitter needs to go. nextElectrodeDestination is not set, so we do it here.
                d.SetNextElectrodeDestination(source.Name);

                Electrode dest = d.nextElectrodeDestination;
                Electrode start = dest.GetClosestElectrodeInList(source.Occupy);

                int destBuffer = Constants.DestBuff;

                // If destination is in or too close to the source, set a destination to split to - d is tasked with going back later.
                // MinDist = sqrt(d.size)
                if (!CheckMinDistanceDrop(d.nextElectrodeDestination, d, Constants.SplitBuff, start: start))
                {
                    dest = FindFreeSpaceForSplit(d, source, d.Size);
                    destBuffer = 1;
                }

                d.CurrentPath = ModifiedAStar.FindPath(d, dest, splitDroplet: source.Name, start: start);

                while (d.CurrentPath.Value.path[1].Item1.Occupant == source)
                {
                    start = d.CurrentPath.Value.path[1].Item1;
                    d.CurrentPath.Value.path.RemoveAt(0);
                }

                // Make tree from the closest electrode to dest.
                Tree sourceTree = BuildTree(source, [], start);

                d.SnekMode = true;
                d.SnekList = [];

                // Find path split droplet needs to follow.
                Printer.PrintLine(start.Name);
                TakeOverElectrode(d, start);

                Printer.PrintBoard();

                // Find number of electrodes to occupy
                if (!Settings.ConnectedToHardware)
                {
                    d.SetSizes(source.Volume * ratio / 100);
                    if (d.Volume < 6)
                    {
                        throw new ArgumentException("Droplet too small");
                    }
                }
                else
                {
                    throw new Exception("Connected to hardware");
                }


                // Move out the amount of spaces this splitter needs
                int i = 1;
                while (i < d.Size)
                {
                    if (d.CurrentPath.Value.path.Count > destBuffer)
                    {

                        // Save source occupy
                        List<Electrode> sourceEl = new(source.Occupy);

                        // Try to make move toward dest
                        bool moved = MoveTowardDest(d, dest, splitDroplet: source.Name, remove: false).Item1;

                        if (moved)
                        {
                            // Turn off the right electrode if not moving inside source
                            if (!(d.Occupy.Intersect(sourceEl).Count() > 0))
                            {
                                sourceTree.RemoveLeaf();
                            }

                            i++;
                        }
                        else if (d.Waiting == false)
                        {
                            d.Waiting = true;
                        }

                    }
                    else
                    {
                        bool moved = false;
                        // Try to move one further away, to ensure that the CoilWithoutRemoval and TakeOverElectrode doesn't break the droplet
                        moved = MoveTowardDest(d, dest, splitDroplet: source.Name, remove: false).Item1;
                        if (moved)
                        {
                            i += 1;
                        }
                        int remainder = d.Size - i;



                        Electrode head = d.SnekList.First.Value;
                        // Coil at dest and remove from source
                        CoilWithoutRemoval(d, remainder, source);
                        // Remove the corresponding electrodes from the source tree
                        for (int j = 0; j < remainder; j++)
                        {
                            sourceTree.RemoveLeaf();
                        }

                        // Move away and replenish start TODO: Maybe put a moved on here
                        List<Electrode> oldList = d.Occupy;
                        if (ratios.Last().Key != d.Name) // If the source needs to split more, it needs start back
                        {
                            TakeOverElectrode(source, start);
                        }
                        
                        // Normal coil to suck up tail
                        CoilSnek(d, d.Occupy.Contains(dest) ? dest : head, input: true); // Input: true adds one more to the droplet
                        sourceTree.RemoveLeaf();
                        fixStart = false;

                        i += remainder;
                    }
                    if (CheckDropletHeldTogetherParity(d))
                    {
                        throw new ArgumentException("Anomaly in Occupy.Count");
                    }
                    Printer.PrintBoard();
                }

                // Move all the way away
                // I can use CheckBoarder for it and just give it the current position of the droplet that was just split off.
                bool hitBorders = false;
                while (!CheckBorder(d, d.Occupy).Item1)
                {
                    hitBorders = true;
                    if (d.CurrentPath == null || d.CurrentPath.Value.path.Count > destBuffer)
                    {
                        bool moved = MoveTowardDest(d, dest, splitDroplet: source.Name).Item1;

                        if (CheckDropletHeldTogetherParity(d))
                        {
                            throw new ArgumentException("Anomaly in Occupy.Count");
                        }
                        // Give start back to source to keep it connected.
                        if (fixStart && moved)
                        {
                            fixStart = false;
                            MoveOnElectrode(source, start);
                            sourceTree.RemoveLeaf();
                        }

                        if (!moved && d.Waiting == false)
                        {
                            d.Waiting = true;
                        }
                    }
                    else
                    {
                        CoilSnek(d);
                    }

                }
                

                d.Waiting = true;
                d.Important = false;

                Program.C.RemovePath(d);

                // Give d the task of moving.
                dropSem[d.Name].TryRelease(2);

                Printer.PrintBoard();
                Printer.PrintLine(dName + " DONE!!");

            }
            source.RemoveFromBoard();
        }

        /// <summary>
        /// Coils around head without removing from tail.
        /// Used for input and split.
        /// </summary>
        /// <param name="d"></param>
        public static void CoilWithoutRemoval(Droplet d, int remainder, Droplet? source = null)
        {
            //List<Electrode> workingList = [d.Occupy[0]];
            List<Electrode> workingList;
            lock (MoveLock)
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException();
                }
                workingList = [d.SnekList.First.Value];
            }
           
            // Coil without removing
            int i = 0;
            while (i < remainder)
            {
                if (workingList.Count == 0)
                {
                    throw new IllegalMoveException("Not enough space to coil without removal");
                }
                Electrode e = workingList[0];
                workingList.Remove(e);

                List<(Electrode, Direction?)> neighbors = e.GetExtendedNeighbors(d, source, splitPlacement: true);

                foreach ((Electrode n, Direction? dir) in neighbors)
                {
                    if (n.Apparature == null) // If apparature at each true neighbor, an unconnected corner may happen :(
                    {
                        MoveOnElectrode(d,n, first: false);
                        workingList.Add(n);
                        i++;

                        if (i >= remainder)
                        {
                            return;
                        }
                    }
                }
            }
            if (CheckDropletHeldTogetherParity(d))
            {
                throw new ArgumentException("Anomaly in Occupy.Count");
            }
            Program.C.RemovePath(d);
        }

        public static Electrode? FindFreeSpaceForSplit(Droplet d, Droplet source, int size)
        {
            bool foundSpace = false;
            List<Electrode> activeEl = new (source.Occupy);
            List<Electrode> seenElectrodes = [];

            // Search for space until a large enough one is found.
            while (!foundSpace)
            {
                // Use extended neighbors until a free space is found.
                List<(Electrode, Direction?)> neighbors = activeEl[0].GetExtendedNeighbors();
                seenElectrodes.Add(activeEl[0]);
                activeEl.RemoveAt(0);

                foreach ((Electrode e, Direction? dir) in neighbors)
                {
                    if (seenElectrodes.Contains(e))
                    {
                        continue;
                    }
                    // If e is not a space that d can occupy, add it to the list
                    if (!CheckLegalMove(d, [e]).legalmove || e.Apparature != null)
                    {
                        activeEl.Add(e);
                    }

                    // If e IS a space that d can occupy, check if the space is large enough.
                    if (!activeEl.Contains(e))
                    {
                        (bool, Electrode?) spaceCheck = CheckIfLargeEnoughSpace(d, e, size);
                        if (spaceCheck.Item1)
                        {
                            return spaceCheck.Item2;
                        }
                        else
                        {
                            activeEl.Add(e);
                        }
                    }
                }
            }

            return null;
        }

        public static (bool, Electrode?) CheckIfLargeEnoughSpace(Droplet d, Electrode e, int size)
        {
            // REMINDER dropletSize kvadratrod væk fra source mindst. To be safe the furthest one away?
            List<Electrode> checkingList = [e];
            List<Electrode> clearList = [e];
            List<(Electrode, Direction?)> neighbors = [];
            
            while(clearList.Count < size)
            {
                if (checkingList.Count == 0)
                {
                    return (false, null);
                }

                neighbors = checkingList[0].GetExtendedNeighbors();
                checkingList.RemoveAt(0);

                foreach ((Electrode n, Direction? dir) in neighbors)
                {
                    if (CheckLegalMove(d, [n]).legalmove && n.Apparature == null)
                    {
                        clearList.Add(n);
                        checkingList.Add(n);
                    }

                }
            }

            // Find middle of space
            Electrode? middle = ApproximateMiddleOfSpace(clearList);

            return (true, middle);
        }

        public static bool CheckMinDistanceDrop(Electrode dest, Droplet d, int allowedDist, Electrode? start = null)
        {
            if (start == null)
            {
                start = d.Occupy[0];
            }
            if (Electrode.GetDistance(dest, start) < allowedDist)
            {
                return false;
            }
            return true;
        }

        public static Electrode? ApproximateMiddleOfSpace(List<Electrode> space)
        {
            int minX = int.MaxValue;
            int maxX = 0;
            int minY = int.MaxValue;
            int maxY = 0;

            foreach (Electrode e in space)
            {
                if (e.ePosX < minX) minX = e.ePosX;
                if (e.ePosY < minY) minY = e.ePosY;
                if (e.ePosX > maxX) maxX = e.ePosX;
                if (e.ePosY > maxY) maxY = e.ePosY;
            }

            // Find electrode in middle of area
            int midX = (minX + maxX) / 2;
            int midY = (minY + maxY) / 2;

            List<(Electrode, Direction?)> neighbors = [];
            List<Electrode> checking = [];

            // If Electrode[midX, midY] is in space, return it.
            // Otherwise, look at neighbors until one is in space.
            if (space.Contains(Program.C.board.Electrodes[midX, midY]))
            {
                return Program.C.board.Electrodes[midX, midY];
            }
            else
            {
                checking.Add(Program.C.board.Electrodes[midX, midY]);
                while (checking.Count > 0)
                {
                    neighbors = checking[0].GetExtendedNeighbors();
                    checking.RemoveAt(0);

                    foreach ((Electrode n, Direction? dir) in neighbors)
                    {
                        if (space.Contains(n))
                        {
                            return n;
                        }
                        checking.Add(n);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sets a droplet to wait for specified amount of milliseconds
        /// </summary>
        /// <param name="d"></param>
        /// <param name="milliseconds"></param>
        public static void WaitDroplet(Droplet d, int milliseconds)
        {
            Outparser.Outparser.WaitDroplet(d, milliseconds);
            

        }

        /// <summary>
        /// Finds the location in the apparature for travel
        /// </summary>
        /// <param name="d"></param>
        /// <param name="destination"></param>
        public static void SetupDestinations(Droplet d, Apparature destination)
        {
            d.nextDestination = destination;
            if (d.Occupy.Count > 0)
            {
                d.SetNextElectrodeDestination();
            }

        }

        /// <summary>
        /// Return true if there is a parity problem, false if parity is fine
        /// </summary>
        /// <param name="d"></param>
        /// <param name="preSize"></param>
        /// <returns></returns>
        private static bool CheckParity(Droplet d, int preSize, List<string>? mergeDroplets)
        {
            lock (d)
            {
                if (d.Occupy.Count != d.Occupy.Distinct().ToList().Count) // If duplicates in occupy, bad
                {
                    return true;
                }

                if (CheckDropletHeldTogetherParity(d, mergeDroplets))
                {
                    return true;
                }

                if (mergeDroplets == null) // If a droplet is trying to merge, the resulting size will be different
                {
                    if (d.Removed && d.Occupy.Count == 0 && d.Size == 0) // If dropelt is removed and empty, good
                    {
                        return false;
                    }
                    if (d.Occupy.Count != d.Size) // If sizes are different, bad
                    {
                        return true;
                    }
                    if (preSize != d.Occupy.Count) //If sizes changed, bad
                    {
                        return true;
                    }
                }

                return false;
            }
            
        }

        private static bool CheckDropletHeldTogetherParity(Droplet d, List<string>? mergeDroplets = null)
        {
            lock (d)
            {

                if (d.Removed)
                {
                    throw new ThreadInterruptedException();
                }
                Tree? dropletTree = null;
                if (d.Occupy.Count != 0)// For letting coil inputting happen
                {
                    dropletTree = BuildTree(d, [], d.Occupy[0]);
                }

                if (mergeDroplets == null && d.Occupy.Count != 0 && dropletTree != null && !dropletTree.CheckTree()) // If droplet isnt held together, bad
                {
                    return true;
                }

                return false;
            }
            
        }


    }
}
