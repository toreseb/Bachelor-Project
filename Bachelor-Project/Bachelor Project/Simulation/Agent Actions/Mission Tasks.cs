using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Electrode_Types.Actuator_Types;
using Bachelor_Project.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation.Agent_Actions
{
    // This class contains the more complicated missions the agents will have.
    /// <summary>
    /// This class contains the <see cref="Mission_Tasks"/>, which uses <see cref="Droplet_Actions"/> to perform the <see cref="Command"/>s given by the <see cref="Commander"/> 
    /// </summary>
    public static class Mission_Tasks
    {
        /// <summary>
        /// Input the <see cref="Droplet"/> <paramref name="d"/> onto the <see cref="Board"/> at the <see cref="Input"/> <paramref name="i"/> with a <paramref name="volume"/>.
        /// <para>If <paramref name="destination"/> is specified, it moves towards the location, else it coils around <paramref name="i"/></para>
        /// <para><paramref name="InputSem"/> is used to time the inputs of <see cref="Droplet"/>s</para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="i"></param>
        /// <param name="volume"></param>
        /// <param name="InputSem"></param>
        /// <param name="destination"></param>
        /// <returns><see langword="true"/> when the task is finished</returns>
        public static bool InputDroplet(Droplet d, Input i, int volume, UsefulSemaphore? InputSem = null, Apparatus? destination = null)
        {
            Printer.PrintLine(d.Name + " has NextDestiantion of: " + d.nextDestination);
            Printer.PrintLine(d.Name + " : INPUTTING");
            Droplet_Actions.InputDroplet(d, i, volume, destination);

            InputSem?.TryReleaseOne();
            
            if (destination != null && d.GetWork().Count == 0)
            {
                Electrode destElectrode = d.GetClosestFreePointer(destination);
                d.CurrentPath = ModifiedAStar.FindPath(d, destElectrode);
                while (d.CurrentPath.Value.path.Count > Constants.DestBuff)
                {
                    d.Waiting = true;
                    try
                    {
                        Droplet_Actions.MoveTowardDest(d, destElectrode);
                    }
                    catch (NewWorkException)
                    {
                        Printer.PrintBoard();
                        d.Waiting = false;
                        d.MergeReady = true;
                        return false;
                    }

                    if (d.CurrentPath.Value.path.Count <= Constants.DestBuff)
                    {
                        Droplet_Actions.CoilSnek(d, d.SnekList.First());
                        Program.C.RemovePath(d);
                        return true;
                    }
                }
            }
            
            return true;

        }

        /// <summary>
        /// Outputs the <see cref="Droplet"/> <paramref name="droplet"/> at the <see cref="Output"/> <paramref name="output"/>.
        /// </summary>
        /// <param name="droplet"></param>
        /// <param name="output"></param>
        /// <returns><see langword="true"/> when the task is finished</returns>
        public static bool OutputDroplet(Droplet droplet, Output output)
        {
            Printer.PrintLine(droplet.Name + " : OUTPUTTING");
            Printer.PrintBoard();
            droplet.Important = true;
            Droplet_Actions.MoveToApparatus(droplet, output);
            Droplet_Actions.OutputDroplet(droplet, output);

            return true;
        }

        /// <summary>
        /// Mixes the <see cref="Droplet"/> <paramref name="d"/>.
        /// <para>If <paramref name="newType"/> is specified then <paramref name="d"/> changes its type to <paramref name="newType"/></para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="pattern"></param>
        /// <param name="newType"></param>
        /// <returns><see langword="true"/> when the task is finished</returns>
        public static bool MixDroplet(Droplet d, string pattern, string? newType = null)
        {
            Droplet_Actions.MixDroplet(d, pattern);
            if (newType != null)
            {
                d.ChangeType(newType);
            }
            return true;

        }

        /// <summary>
        /// Is called from the <see cref="Droplet"/> <paramref name="d"/> who results from the merging. The other <paramref name="inputDroplets"/> call the <see cref="AwaitMergeWork(Droplet, Task{Electrode}, UsefulSemaphore, UsefulSemaphore, UsefulSemaphore, List{string})"/>.
        /// <para> <paramref name="calcMerge"/> is used to calculate the location of the merge, while the other <see cref="Droplet"/>s can access it</para>
        /// <para> The semaphores are used to time the many different <see cref="Droplet"/>s.</para>
        /// <para> <paramref name="cmdDestination"/> is where the resulting <see cref="Droplet"/> <paramref name="d"/> wants to go after the merge.</para>
        /// </summary>
        /// <param name="inputDroplets"></param>
        /// <param name="d"></param>
        /// <param name="calcMerge"></param>
        /// <param name="everybodyReady"></param>
        /// <param name="mergesFinished"></param>
        /// <param name="cmdDestination"></param>
        /// <returns><see langword="true"/> when the task is finished</returns>
        public static bool MergeDroplets(List<string> inputDroplets, Droplet d, Task calcMerge, UsefulSemaphore everybodyReady, UsefulSemaphore mergesFinished, Apparatus cmdDestination)
        {
            Droplet_Actions.SetupDestinations(d, cmdDestination);
            Printer.PrintLine(d.Name + " : MERGING");
            foreach (var item1 in inputDroplets)
            {
                foreach (var item2 in inputDroplets)
                {
                    Droplet id1 = Program.C.board.Droplets[item1];
                    Droplet id2 = Program.C.board.Droplets[item2];

                    if(id1 != id2)
                    {
                        lock (id1.Contamintants)
                        {
                            id1.Contamintants.Remove(id2.Substance_Name);
                        }
                        
                    }

                }

            }
            everybodyReady.Check(inputDroplets.Count);
            calcMerge.Start();
            mergesFinished.Wait(inputDroplets.Count);
            foreach (var inputDroplet in inputDroplets)
            {
                Droplet other = Program.C.board.Droplets[inputDroplet];
                if (!other.Removed)
                {
                    d.TakeOver(other);
                }
            }
            Printer.PrintBoard();
            return true;
        }

        /// <summary>
        /// Splits the calculating <see cref="Droplet"/> <paramref name="d"/> into the resulting <see cref="Droplet"/>s, one for each value in <paramref name="percentages"/>. The other <see cref="Droplet"/>s call <see cref="AwaitSplitWork(Droplet, Apparatus, UsefulSemaphore)"/>.
        /// <para> The semaphore, <paramref name="dropSem"/>, is used for timing the many threads together.</para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="percentages"></param>
        /// <param name="dropSem"></param>
        /// <returns><see langword="true"/> when the task is finished</returns>
        public static bool SplitDroplet(Droplet d, Dictionary<string, double> percentages, Dictionary<string, UsefulSemaphore> dropSem)
        {
            //Droplet_Actions.SetupDestinations(d, cmdDestination);
            d.Important = true;

            // Run Droplet_Actions.splitDroplet
            Droplet_Actions.SplitDroplet(d, percentages, dropSem);

            return true;
        }

        /// <summary>
        /// Waits on the merging <see cref="Droplet"/> allows <see cref="Droplet"/> <paramref name="d"/> to start merging.
        /// <para><paramref name="AwaitWork"/> is a <see cref="Task"/> that calculates the <see cref="Electrode"/> that the merge happpens.</para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="AwaitWork"></param>
        /// <param name="imReady"></param>
        /// <param name="locCalculated"></param>
        /// <param name="selfDone"></param>
        /// <param name="mergeDoplets"></param>
        /// <returns><see langword="true"/> when the task is finished</returns>
        public static bool AwaitMergeWork(Droplet d, Task<Electrode> AwaitWork, UsefulSemaphore imReady, UsefulSemaphore locCalculated, UsefulSemaphore selfDone, List<string> mergeDoplets) // check if beforedone is done, and then release on selfDone when done
        {
            d.Important = true;
            d.SnekList = [];
            d.SnekMode = false;
            imReady.TryReleaseOne();
            Program.C.RemovePath(d);
            imReady.Check(mergeDoplets.Count);
            locCalculated.WaitOne();
            Electrode location = AwaitWork.Result;
            try
            {
                d.CurrentPath = null;
                d.nextElectrodeDestination = location;
                Droplet_Actions.MoveToDest(d, location, mergeDoplets);
            }
            catch (Exception)
            {
                if (d.Removed)
                {
                    selfDone.TryReleaseOne();
                    return true;
                }
                throw;
            }
            selfDone.TryReleaseOne();
            return true;

        }

        /// <summary>
        /// <see cref="Droplet"/> <paramref name="droplet"/> waits on <paramref name="beginSem"/>. When <see cref="SplitDroplet(Droplet, Dictionary{string, double}, Dictionary{string, UsefulSemaphore})"/> has split off <paramref name="droplet"/> it can continue.
        /// </summary>
        /// <param name="droplet"></param>
        /// <param name="cmdDestination"></param>
        /// <param name="beginSem"></param>
        /// <returns><see langword="true"/> when the task is finished</returns>
        public static bool AwaitSplitWork(Droplet droplet, Apparatus cmdDestination, UsefulSemaphore beginSem)
        {
            // Set destinations and release one semaphore
            Droplet_Actions.SetupDestinations(droplet, cmdDestination);
            beginSem.TryReleaseOne();

            // Wait for SplitDroplet to release 2 semaphore
            beginSem.Wait(2);
            return true;
        }

        /// <summary>
        /// Heats the <see cref="Droplet"/> <paramref name="d"/> up by moving it to the <see cref="Heater"/> <paramref name="heater"/> and wating for <paramref name="time"/> seconds. Then the <see cref="Droplet.Temperature"/> is changed.
        /// <para>If <paramref name="newType"/> is specified, the <see cref="Droplet"/> <paramref name="d"/> changes its <see cref="Droplet.Substance_Name"/> into <paramref name="newType"/></para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="heater"></param>
        /// <param name="time"></param>
        /// <param name="newType"></param>
        /// <returns><see langword="true"/> when the task is finished</returns>
        /// <exception cref="ArgumentException"></exception>
        public static bool TempDroplet(Droplet d, Heater heater, int time, string? newType = null)
        {
            Droplet_Actions.SetupDestinations(d, heater);
            d.Important = true;
            Droplet_Actions.MoveToApparatus(d, heater);
            if (time <= 0)
            {
                throw new ArgumentException("Time must be greater than 0");
            }
            Droplet_Actions.WaitDroplet(d, time*1000);
            if (newType != null && d.Substance_Name != newType)
            {
                d.ChangeTemp(heater.ActualTemperature);
                d.ChangeType(newType);
            }
            return true;
        }

        /// <summary>
        /// Senses the relevant <see cref="Sensor.Sense()"/> of the <see cref="Droplet"/> <paramref name="d"/>.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="sensor"></param>
        /// <returns><see langword="true"/> when the task is finished</returns>
        public static bool SenseDroplet(Droplet d, Sensor sensor)
        {
            Droplet_Actions.SetupDestinations(d, sensor);
            Printer.PrintLine(d.Name + " : SENSING");
            d.Important = true;
            Electrode closest = Droplet_Actions.MoveToApparatus(d, sensor);
            sensor.Sense();
            return true;
        }

        /// <summary>
        /// Makes the <see cref="Droplet"/> <paramref name="d"/> wait for <paramref name="time"/> milliseconds.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool WaitDroplet(Droplet d, int time)
        {
            d.Important = true;
            Printer.PrintLine(d.Name + " : WAITING");
            Droplet_Actions.WaitDroplet(d,time);
            Printer.PrintLine(d.Name + " : DONE WAITING");
            return true;
        }





    }
}
