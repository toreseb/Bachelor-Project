using Bachelor_Project.Electrode_Types;
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

    /// <summary>
    /// This class contains the more basic movements and actions an <see cref="Droplet"/> agent can take.
    /// </summary>
    public static class Droplet_Actions
    {
        
        public static readonly object MoveLock = new object(); //Lock to ensure that only one droplet moves at the same time

        /// <summary>
        /// Inputs a <see cref="Droplet"/> <paramref name="d"/> onto the board with <paramref name="volume"/> at the specified <see cref="Input"/> <paramref name="i"/>.
        /// <para>If a <paramref name="destination"/> is specified it moves toward it, else it coils around the <see cref="Input"/> <paramref name="i"/>.</para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="i"></param>
        /// <param name="volume"></param>
        /// <param name="destination"></param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ThreadInterruptedException"></exception>
        public static void InputDroplet(Droplet d, Input i, int volume, Apparatus? destination = null)
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

        /// <summary>
        /// Mixes the <see cref="Droplet"/> <paramref name="d"/> by moving it around in a square. The <paramref name="pattern"/> is curently not implemented
        /// </summary>
        /// <param name="d"></param>
        /// <param name="pattern"></param>
        /// <returns><see cref="bool"/> determining if the mix succeded</returns>
        /// <exception cref="IllegalMoveException"></exception>
        public static bool MixDroplet(Droplet d, string pattern)
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
                    if (e.EPosX < 1) left = false;
                    if (!(e.EPosX < Program.C.board.GetXElectrodes() - 1)) right = false;
                    if (e.EPosY < 1) up = false;
                    if (!(e.EPosY < Program.C.board.GetYElectrodes() - 1)) down = false;
                }

                // Check for other droplets and contaminants in zone (+ boarder)
                // Needs to check for each possible direction
                List<Electrode> temp = new List<Electrode>(d.Occupy);

                if (Convert.ToInt32(up) + Convert.ToInt32(right) + Convert.ToInt32(down) + Convert.ToInt32(left) >= 2 && !((Convert.ToInt32(up) + Convert.ToInt32(down) == 0) || (Convert.ToInt32(right) + Convert.ToInt32(left) == 0)))
                {
                    foreach (Electrode e in d.Occupy)
                    {
                        if (up && Program.C.board.Electrodes[e.EPosX, e.EPosY - 1].Occupant != d)
                        {
                            temp.Add(Program.C.board.Electrodes[e.EPosX, e.EPosY - 1]);
                        }
                        if (right && Program.C.board.Electrodes[e.EPosX + 1, e.EPosY].Occupant != d)
                        {
                            temp.Add(Program.C.board.Electrodes[e.EPosX + 1, e.EPosY]);
                        }
                        if (down && !up && Program.C.board.Electrodes[e.EPosX, e.EPosY + 1].Occupant != d)
                        {
                            temp.Add(Program.C.board.Electrodes[e.EPosX, e.EPosY + 1]);
                        }
                        if (left && !right && Program.C.board.Electrodes[e.EPosX - 1, e.EPosY].Occupant != d)
                        {
                            temp.Add(Program.C.board.Electrodes[e.EPosX - 1, e.EPosY]);
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
                                bool moved = false;
                                int counter = 0;
                                while (!moved)
                                {
                                    moved = MoveDroplet(d, item);
                                    if (!moved)
                                    {
                                        counter++;
                                    }
                                    else
                                    {
                                        counter = 0;
                                    }
                                    
                                    if (counter == 50)
                                    {
                                        throw new IllegalMoveException("No space for mixing");
                                    }
                                }
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

        /// <summary>
        /// Moves a <see cref="Droplet"/> <paramref name="d"/> to a <see cref="Apparatus"/> <paramref name="dest"/>, and coils either on or in it, depending on <see cref="Apparatus.CoilInto"/>.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="dest"></param>
        /// <returns>Closest <see cref="Electrode"/> in <see cref="Apparatus"/> <paramref name="dest"/> to <see cref="Droplet"/> <paramref name="d"/></returns>
        public static Electrode MoveToApparature(Droplet d, Apparatus dest)
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

        /// <summary>
        /// Moves a <see cref="Droplet"/> <paramref name="d"/> to an <see cref="Electrode"/> <paramref name="destination"/>.
        /// <para> If <paramref name="mergeDroplets"/> is specified, it can merge along the way.</para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="destination"></param>
        /// <param name="mergeDroplets"></param>
        public static void MoveToDest(Droplet d, Electrode destination, List<string>? mergeDroplets = null)
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

        /// <summary>
        /// Moves a <see cref="Droplet"/> <paramref name="d"/> towards an <see cref="Apparatus"/> <paramref name="dest"/>.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="dest"></param>
        /// <returns>Closest <see cref="Electrode"/> in <see cref="Apparatus"/> <paramref name="dest"/> to <see cref="Droplet"/> <paramref name="d"/></returns>
        public static Electrode MoveTowardApparature(Droplet d, Apparatus dest)
        {
            Electrode closest = d.GetClosestFreePointer(dest);
            MoveTowardDest(d, closest);
            return closest;
        }

        /// <summary>
        /// Moves a <see cref="Droplet"/> <paramref name="d"/> towards an <see cref="Electrode"/> <paramref name="destination"/>.
        /// <para>If <paramref name="remove"/> is false it does not remove from the <see cref="Droplet"/> <paramref name="d"/>.</para>
        /// <para>If <paramref name="mergeDroplets"/> is specified it can merge during the movement.</para>
        /// <para>If <paramref name="splitDroplet"/> is specified, it can move through the border of the specified split <see cref="Droplet"/>.</para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="destination"></param>
        /// <param name="mergeDroplets"></param>
        /// <param name="splitDroplet"></param>
        /// <param name="remove"></param>
        /// <returns><see cref="bool"/> for if the <see cref="Droplet"/> moved, and the <see cref="Electrode"/> the <see cref="Droplet"/> <paramref name="d"/> moved off</returns>
        /// <exception cref="ThreadInterruptedException"></exception>
        /// <exception cref="NewWorkException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IllegalMoveException"></exception>
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
                UncoilSnek(d, destination, mergeDroplets);
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
                    Merge(d, occupant, d.SnekList.First.Value.ElectrodeStep(dir));
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

        /// <summary>
        /// Move <see cref="Droplet"/> <paramref name="d"/> as an amorphous <see cref="Droplet"/> in the direction <paramref name="dir"/>.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="dir"></param>
        /// <returns><see cref="bool"/> determining if the movement succeded.</returns>
        public static bool MoveDroplet(Droplet d, Direction dir)
        {
            bool legalMove = true;
            List<Electrode> temp = new List<Electrode>();

            (int xChange, int yChange) = DirectionUtils.GetXY(dir);

            // Make list with new placements of electrodes
            foreach (Electrode e in d.Occupy)
            {
                // Check if new posision is legal
                if (CheckLegalPosition(d,[(e.EPosX + xChange, e.EPosY + yChange)]))
                {
                    temp.Add(Program.C.board.Electrodes[e.EPosX + xChange, e.EPosY + yChange]);
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
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the <see cref="Droplet"/> <paramref name="d"/> moving onto the <see cref="Electrode"/>s in <paramref name="temp"/> would violate the borders of other <see cref="Droplet"/>s.
        /// <para>If <paramref name="mergeDroplets"/> is specified, it ignores the borders of the <see cref="Droplet"/>s in <paramref name="mergeDroplets"/></para>
        /// <para>If <paramref name="source"/> is specified, it ignores the borders of the <paramref name="source"/> <see cref="Droplet"/></para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="temp"></param>
        /// <param name="mergeDroplets"></param>
        /// <param name="source"></param>
        /// <returns><see cref="bool"/> for if the movement is legal and the occupant to tell which <see cref="Droplet"/>'s border it violates </returns>
        private static (bool legalmove, Droplet? occupant) CheckBorder(Droplet d, List<Electrode> temp, List<string>? mergeDroplets = null, string? source = null)
        {
            Droplet? occupant = null;
            mergeDroplets ??= [];
            // For snek, just put in head instead of all positions
            bool legalMove = true;
            foreach (Electrode e in temp)
            {
                // Check neighbors all the way around electrode for occupancy
                // If same droplet, fine. If blank, fine. If other droplet, not fine.

                int xCheck = e.EPosX;
                int yCheck = e.EPosY;
                for (int i = 1; i <= 8; i++)
                {
                    switch (i)
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
                        if (occupant != null && d != occupant && !mergeDroplets.Contains(occupant.Name))
                        {
                            if (source != null)
                            {
                                if (occupant.Name != source)
                                {
                                    legalMove = false;
                                    return (legalMove, occupant);
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
        /// Checks if the <see cref="Electrode"/> <paramref name="el"/> is in the border of an <see cref="Apparatus"/>.
        /// <para><paramref name="coilIntoApp"/> specifies if it is currently coiling into a <see cref="Apparatus"/></para>
        /// </summary>
        /// <param name="el"></param>
        /// <param name="alreadyOnApp"></param>
        /// <param name="coilIntoApp"></param>
        /// <returns><see cref="bool"/> determening if there are border violations</returns>
        private static bool CheckApparatureBorders(Electrode el, Apparatus? coilIntoApp)
        {
            if (el.Apparature != null && el.Apparature != coilIntoApp)
            {
                return false;
            }
            List<(Electrode, Direction?)> border = el.GetExtendedNeighbors();

            foreach ((Electrode cEl, Direction? _) in border)
            {
                if (cEl.Apparature != null && cEl.Apparature != coilIntoApp)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if the specific <paramref name="xPos"/> and <paramref name="yPos"/> is inside the bound of the <see cref="Board"/>.
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns><see cref="bool"/> determening if inside <see cref="Board"/></returns>
        public static bool CheckBoardEdge(int xPos, int yPos)
        {
            return !(xPos < 0 || xPos >= Program.C.board.GetXElectrodes() || yPos < 0 || yPos >= Program.C.board.GetYElectrodes());
        }

        /// <summary>
        /// Checking the placement of the <see cref="Droplet"/> <paramref name="d"/>, running both <see cref="CheckOtherDroplets"/> and <see cref="CheckContaminations"/>, giving them the relevant parameters.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="temp"></param>
        /// <param name="mergeDroplets"></param>
        /// <param name="splitDroplet"></param>
        /// <returns><see cref="bool"/> determining if the placement is allowed</returns>
        private static bool CheckPlacement(Droplet d, List<Electrode> temp, List<string>? mergeDroplets = null, string? splitDroplet = null)
        {
            if (!CheckOtherDroplets(d, temp, mergeDroplets, splitDroplet))
            {
                return false;
            }
            if (!CheckContaminations(d, temp))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the <see cref="Droplet"/> <paramref name="d"/> moved on the <see cref="Electrode"/>s <paramref name="temp"/> would place <paramref name="d"/> on top of existing <see cref="Droplet"/>s.
        /// <para>If <paramref name="mergeDroplets"/> is specified, <paramref name="d"/> is allowed to move through the <see cref="Droplet"/>s in <paramref name="mergeDroplets"/></para>
        /// <para>If <paramref name="splitDroplet"/> is specified, <paramref name="d"/> is allowed to move through the split <see cref="Droplet"/></para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="temp"></param>
        /// <param name="mergeDroplets"></param>
        /// <param name="splitDroplet"></param>
        /// <returns><see cref="bool"/> determining if the move is allowed</returns>
        public static bool CheckOtherDroplets(Droplet d, List<Electrode> temp, List<string>? mergeDroplets = null, string? splitDroplet = null)
        {
            foreach (Electrode e in temp)
            {
                if (!(e.Occupant == null || d == e.Occupant || (mergeDroplets != null && mergeDroplets.Contains(e.Occupant.Name)) || (splitDroplet != null && e.Occupant.Name == splitDroplet)))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if the <see cref="Droplet"/> <paramref name="d"/> moved on the <see cref="Electrode"/>s <paramref name="temp"/> would be allowed by contaminations.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="temp"></param>
        /// <returns><see cref="bool"/> determining if the movement is allowed</returns>
        public static bool CheckContaminations(Droplet d, List<Electrode> temp){
            foreach (Electrode e in temp)
            {
                // Check for contaminants
                lock (e.GetContaminants())
                {
                    foreach (string c in e.GetContaminants())
                    {
                        if (d.Contamintants.Contains(c))
                        {
                            return false;
                        }
                    }
                }

            }
            return true;
        }

        /// <summary>
        /// Checks if the <see cref="Droplet"/> <paramref name="d"/> moved on the <see cref="Electrode"/> <paramref name="temp"/> would be a legal move. It uses many if the other checks in <see cref="Droplet_Actions"/>.
        /// <para>Parameters are parsed to the functions which needs them</para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="temp"></param>
        /// <param name="mergeDroplets"></param>
        /// <param name="source"></param>
        /// <param name="splitPlacement"></param>
        /// <returns><see cref="bool"/> determining if the movement is allowed to happen</returns>
        public static (bool legalmove, Droplet? occupant) CheckLegalMove(Droplet d, List<Electrode> temp, List<string>? mergeDroplets = null, string? source = null, bool splitPlacement = false)
        {
            bool legalMove = true;
            (bool borderCheck, Droplet? occupant) = CheckBorder(d, temp, mergeDroplets, source);
            if (!(borderCheck && CheckPlacement(d, temp, mergeDroplets, splitPlacement ? null : source)))
            {
                legalMove = false;
            }

            return (legalMove, occupant);
        }

        /// <summary>
        /// Checks if the given <paramref name="pos"/> is inside the <see cref="Board"/>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="pos"></param>
        /// <returns><see cref="bool"/> determining if inside, and returning <see cref="CheckLegalMove(Droplet, List{Electrode}, List{string}?, string?, bool)"/> if it is.</returns>
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
            return CheckLegalMove(d, temp).legalmove;
        }

        /// <summary>
        /// Makes <see cref="Droplet"/> <paramref name="d"/> <see cref="Thread.Sleep(int)"/> while waiting on the movement being legal. There is a limit to the amount of time it can wait.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="temp"></param>
        /// <exception cref="Exception"></exception>
        public static void AwaitLegalMove(Droplet d, List<Electrode> temp)
        {
            int i = 0;
            while (!CheckLegalMove(d, temp).legalmove)
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


        /// <summary>
        /// Used for moving as a snake, to make sure that the <paramref name="newHead"/> of <paramref name="d"/> is not occupied.
        /// <para>If <paramref name="source"/> is specified, the move is allowed through the <paramref name="source"/> <see cref="Droplet"/></para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="newHead"></param>
        /// <param name="source"></param>
        /// <returns><see cref="bool"/> determing whether the <paramref name="newHead"/> is allowed.</returns>
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


        /// <summary>
        /// Moves the <see cref="Droplet"/> <paramref name="d"/> in the <see cref="Direction"/> <paramref name="dir"/> as a snake. 
        /// <para>If <paramref name="splitDroplet"/> is specified, it can move through the border of the split <see cref="Droplet"/></para>
        /// <para>If <paramref name="remove"/> is <see langword="false"/> the tail is not removed after moving</para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="dir"></param>
        /// <param name="splitDroplet"></param>
        /// <param name="remove"></param>
        /// <returns><see cref="bool"/> determining if the move succeeded and a <see cref="Electrode"/> for which it moved off. </returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ThreadInterruptedException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static (bool, Electrode? MovedOffElectrode) SnekMove(Droplet d, Direction dir, string? splitDroplet = null, bool remove = true)
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
                newHead.Add(Program.C.board.Electrodes[head.EPosX + x, head.EPosY + y]);
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

                    Printer.PrintLine("New head: " + newHead[0].EPosX + " " + newHead[0].EPosY);
                    Printer.PrintLine("Old head: " + head.EPosX + " " + head.EPosY);

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

        /// <summary>
        /// <see cref="Droplet"/> <paramref name="d"/> takes over the <see cref="Electrode"/> <paramref name="e"/>. This removes it from its prior <see cref="Droplet"/>.
        /// <para>If <paramref name="first"/> is <see langword="false"/> it is placed at the tail, else the head.</para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="first"></param>
        /// <exception cref="ThreadInterruptedException"></exception>
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

        /// <summary>
        /// Moves the <see cref="Droplet"/> <paramref name="d"/> onto the <see cref="Electrode"/> <paramref name="e"/>. How it is placed is determine by if the droplet is a snake.
        /// <para>If it is a snake it is placed in the SnekList. If <paramref name="first"/> is <see langword="false"/> it is placed at the end, else at the start </para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="first"></param>
        /// <exception cref="ThreadInterruptedException"></exception>
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

        /// <summary>
        /// Moves the <see cref="Droplet"/> <paramref name="d"/> off an <see cref="Electrode"/>.
        /// <para>If no <paramref name="e"/> is specified it select the tail of SnekList. Else it moves of the <see cref="Electrode"/> <paramref name="e"/></para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <returns><see cref="Electrode"/> representing the moved off <see cref="Electrode"/></returns>
        public static Electrode MoveOffElectrode(Droplet d, Electrode? e = null)
        {
            lock (MoveLock)
            {
                e ??= d.SnekList.Last();
                if (e == null)
                {
                    throw new IllegalMoveException("A droplet is trying to move off, without specifying the droplet or being a snake.");
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
            
        }


        /// <summary>
        /// Uncoils the <see cref="Droplet"/> <paramref name="d"/> into a snake <see cref="Droplet"/>. It moves towars the <see cref="Electrode"/> <paramref name="dest"/> while uncoiling.
        /// <para>If <paramref name="mergeDroplets"/> is specified it can merge with the <paramref name="mergeDroplets"/> after it finished uncoiling</para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="dest"></param>
        /// <param name="mergeDroplets"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ThreadInterruptedException"></exception>
        public static void UncoilSnek(Droplet d, Electrode dest, List<string>? mergeDroplets = null)
        {
            d.Waiting = false;

            // For Testing
            int preSize = d.Occupy.Count;


            // If snake already occupies destination, coil around dest.
            if (dest.Occupant != null && dest.Occupant.Equals(d))
            {
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
                
                (bool physMove, Electrode? _) = MoveTowardDest(d, dest, mergeDroplets, remove: false);
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
                    blobTree.RemoveLeaf();
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


        /// <summary>
        /// Coils the <see cref="Droplet"/> <paramref name="d"/> around the <see cref="Electrode"/> <paramref name="center"/>.
        /// <para>
        /// If <paramref name="center"/> is specified, it coils around it either if it has an <see cref="Apparatus"/> or is not in the border of an <see cref="Apparatus"/>. This is to ensure that it only is near a <see cref="Apparatus"/> if strictly necessary
        /// If no <paramref name="center"/> is specified, it finds a new <see cref="Electrode"/> furthes away from any <see cref="Apparatus"/> and coils around it.
        /// </para>
        /// <para>If <paramref name="input"/> is <see langword="true"/> and extra occupy will be coiled, used by input.</para>
        /// <para>If <paramref name="mergeDroplets"/> is specified it can coil next to the merge <see cref="Droplet"/>s.</para>
        /// <para><paramref name="ignoreBorders"/> and <paramref name="coiledAgain"/> are both used to coil again, <paramref name="ignoreBorders"/> makes the new coil able to coil inside <see cref="Apparatus"/> borders</para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="center"></param>
        /// <param name="app"></param>
        /// <param name="input"></param>
        /// <param name="mergeDroplets"></param>
        /// <param name="ignoreBorders"></param>
        /// <param name="coiledAgain"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ThreadInterruptedException"></exception>
        /// <exception cref="IllegalMoveException"></exception>
        public static void CoilSnek(Droplet d, Electrode? center = null, Apparatus? app = null, bool input = false, List<string>? mergeDroplets = null, bool ignoreBorders = false, bool coiledAgain = false)
        {
            d.MergeReady = false;

            Program.C.RemovePath(d);
            // For Testing
            int preSize = d.Occupy.Count;

            d.SnekMode = false;
            d.SnekList.Clear();

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
                Electrode? oldCenter = center;
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
                
                center = null;

                while(!(input ||app != null || (oldCenter != null && oldCenter.Apparature != null)) && currentElectrodes.Count > 0)
                {
                    Electrode cElectrode = currentElectrodes[0];

                    if (CheckApparatureBorders(cElectrode, app))
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
                center ??= d.Occupy[0];
                int a = 2;

            }
            

            

            
            
            int amount = input ? d.Occupy.Count : d.Occupy.Count -1; // -1 because the center is not in the list 0 if it inputs new value
            if (amount == 0 && input)
            {
                MoveOnElectrode(d, center);
            }
            
            
            
            List<Electrode> newBlob = [center];
            List<Electrode> activeBlob = [center];
            List<Electrode> seenElectrodes = [center];


            while(activeBlob.Count > 0 && amount > 0) // Find the electrodes to coil in.
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException("Thread has been interrupted");
                }
                Electrode current = activeBlob[0];

                List<(Electrode, Direction)> trueNeighbors = current.GetTrueNeighbors();
                List<Direction> foundNeighbors = [];
                foreach ((Electrode el, Direction dir) in trueNeighbors)
                {
                    if (!(ignoreBorders || input) && !CheckApparatureBorders(el, app))
                    {
                        continue;
                    }
                    
                    if (CheckLegalMove(d,[el]).legalmove && !seenElectrodes.Contains(el) && (app != null && ((app.CoilInto && el.Apparature == app)||(!app.CoilInto)) || (app == null && (el.Occupant == d || el.Apparature == null))))
                    {
                        activeBlob.Add(el);
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
                    List<Electrode> extendedNeighbors = current.GetExtendedNeighborsFromTrue(foundNeighbors);

                    foreach (Electrode el in extendedNeighbors)
                    {
                        if (!(ignoreBorders || input) && !CheckApparatureBorders(el, app))
                        {
                            continue;
                        }

                        if (!seenElectrodes.Contains(el) && CheckLegalMove(d, [el]).legalmove && (app != null && ((app.CoilInto && el.Apparature == app) || (!app.CoilInto)) || (app == null && (el.Occupant == d || el.Apparature == null))))
                        {
                            activeBlob.Add(el);
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
                


                activeBlob.Remove(current);
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

        /// <summary>
        /// Creates a <see cref="Tree"/> using a <see cref="Droplet"/> <paramref name="d"/>. The root is placed at the <see cref="Electrode"/> <paramref name="center"/>. <paramref name="newElectrodes"/> specifies which <see cref="Electrode"/>s the <see cref="Tree"/> cannot remove.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="newElectrodes"></param>
        /// <param name="center"></param>
        /// <returns>The <see cref="Tree"/> built.</returns>
        public static Tree BuildTree(Droplet d, List<Electrode> newElectrodes, Electrode center)
        {
            return new Tree(d, d.Occupy, newElectrodes, center);
        }

        /// <summary>
        /// Outpus the <see cref="Droplet"/> <paramref name="droplet"/> by moving it to an <see cref="Output"/> and removing the <see cref="Droplet"/>.
        /// </summary>
        /// <param name="droplet"></param>
        /// <param name="output"></param>
        internal static void OutputDroplet(Droplet droplet, Output output)
        {
            Tree snekTree = BuildTree(droplet, [], output.pointers[0]);
            snekTree.RemoveTree();
            droplet.RemoveFromBoard();
            Printer.PrintBoard();

        }

        /// <summary>
        /// Calculates the location where the merge should happen, using all the <see cref="Droplet"/>s from <paramref name="inputDroplets"/>. The location is the center between all merging <see cref="Droplet"/>s
        /// </summary>
        /// <param name="inputDroplets"></param>
        /// <param name="droplet"></param>
        /// <param name="done"></param>
        /// <returns>The <see cref="Electrode"/> of the location it found.</returns>
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
                x += randomEl.EPosX;
                y += randomEl.EPosY;
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

        /// <summary>
        /// Merges two <see cref="Droplet"/>s together when they touch at the <see cref="Electrode"/> <paramref name="center"/> while moving, and their mergeDroplets contain each other.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="mergeDroplet"></param>
        /// <param name="center"></param>
        /// <param name="mergeDroplets"></param>
        public static void Merge(Droplet d, Droplet mergeDroplet, Electrode center)
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
        /// Splits the active <see cref="Droplet"/> <paramref name="source"/> into multiple other <see cref="Droplet"/>s, one for each value in <paramref name="ratios"/>. <paramref name="dropSem"/> is used to time the MissionTask.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="ratios"></param>
        /// <param name="dropSem"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static void SplitDroplet(Droplet source, Dictionary<string, double> ratios, Dictionary<string, UsefulSemaphore> dropSem)
        {
            // For loop to split the droplets out one by one.
            // Makes a snake of appropriate size and cuts it off.
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
                d.SetNextElectrodeDestination();

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
                while (!CheckBorder(d, d.Occupy).legalmove)
                {
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
                        if (CheckBorder(d, d.Occupy).legalmove)
                        {
                            CoilSnek(d);
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
                Printer.PrintLine(dName + " done splitting");

            }
            source.RemoveFromBoard();
        }

        /// <summary>
        /// Coils the <see cref="Droplet"/> <paramref name="d"/> around the head of <paramref name="d"/>, while adding the remainder. 
        /// <para>If <paramref name="source"/> is specified so it can coil close to the <paramref name="source"/> <see cref="Droplet"/></para>
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

        /// <summary>
        /// Calculates a free area around the <paramref name="source"/> which can hold the <see cref="Droplet"/> <paramref name="d"/> of size <paramref name="size"/>.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <returns>The <see cref="Electrode"/> at the center of the found space.</returns>
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

        /// <summary>
        /// Checks if the <see cref="Electrode"/> <paramref name="e"/> has a large enough space for <see cref="Droplet"/> <paramref name="d"/> if size <paramref name="size"/>.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="size"></param>
        /// <returns><see cref="bool"/> determining if a space was found and a <see cref="Electrode"/> at the center of the space</returns>
        public static (bool, Electrode?) CheckIfLargeEnoughSpace(Droplet d, Electrode e, int size)
        {
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

        /// <summary>
        /// Checks if the distance between the <paramref name="dest"/> and <paramref name="start"/>, or <see cref="Droplet"/> <paramref name="d"/> if no <paramref name="start"/>, is less than <paramref name="allowedDist"/>.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="d"></param>
        /// <param name="allowedDist"></param>
        /// <param name="start"></param>
        /// <returns><see cref="bool"/> detemining if the distance is allowed</returns>
        public static bool CheckMinDistanceDrop(Electrode dest, Droplet d, int allowedDist, Electrode? start = null)
        {
            start ??= d.Occupy[0];
            if (Electrode.GetDistance(dest, start) < allowedDist)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Approximates the middle <see cref="Electrode"/> of the <see cref="List{Electrode}"/> <paramref name="space"/>.
        /// </summary>
        /// <param name="space"></param>
        /// <returns>The <see cref="Electrode"/> at the approximate middle</returns>
        public static Electrode? ApproximateMiddleOfSpace(List<Electrode> space)
        {
            int minX = int.MaxValue;
            int maxX = 0;
            int minY = int.MaxValue;
            int maxY = 0;

            foreach (Electrode e in space)
            {
                if (e.EPosX < minX) minX = e.EPosX;
                if (e.EPosY < minY) minY = e.EPosY;
                if (e.EPosX > maxX) maxX = e.EPosX;
                if (e.EPosY > maxY) maxY = e.EPosY;
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
        /// Sets the <see cref="Droplet"/> <paramref name="d"/> to wait for specified amount of <paramref name="milliseconds"/>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="milliseconds"></param>
        public static void WaitDroplet(Droplet d, int milliseconds)
        {
            Outparser.Outparser.WaitDroplet(d, milliseconds);
            

        }

        /// <summary>
        /// Finds the location in the <paramref name="destination"/> for travel the <see cref="Droplet"/> <paramref name="d"/> to travel to.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="destination"></param>
        public static void SetupDestinations(Droplet d, Apparatus destination)
        {
            d.nextDestination = destination;
            if (d.Occupy.Count > 0)
            {
                d.SetNextElectrodeDestination();
            }

        }

        /// <summary>
        /// Return <see langword="true"/> if there is a parity problem, false if <see langword="false"/> is fine
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

        /// <summary>
        /// Checks if the <see cref="Droplet"/> <paramref name="d"/> is currently coherent.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="mergeDroplets"></param>
        /// <returns><see cref="bool"/> determining if it is coherent</returns>
        /// <exception cref="ThreadInterruptedException"></exception>
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
