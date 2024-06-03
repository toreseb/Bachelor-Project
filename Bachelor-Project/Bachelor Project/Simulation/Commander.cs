using Antlr4.Runtime;
using Bachelor_Project.Outparser;
using Bachelor_Project.Parsing;
using Bachelor_Project.Simulation.Agent_Actions;
using Bachelor_Project.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Bachelor_Project.Simulation
{
    public class Commander
    {


        public (List<Command> commands, Dictionary<string, string> dropletpairs, Dictionary<string, List<string>> contaminated, Dictionary<string, List<string>> contaminates)? data;
        public Board board;
        public Dictionary<string, ((Point start, Point end)? path, UsefulSemaphore sem)> dropletPaths;
        private List<Command> currentCommands = [];

        public Commander((List<Command>, Dictionary<string, string>, Dictionary<string, List<string>>, Dictionary<string, List<string>>)? data, string boarddata)
        {
            Reset();
            this.data = data;
            string json = File.ReadAllText(boarddata);
            board = Board.ImportBoardData(json);
            dropletPaths = [];
        }

        /// <summary>
        /// Creates all the <see cref="Droplet"/>s that will be on the <see cref="Board"/>, and links the <see cref="Command"/> tree together, and finds the starting <see cref="Command"/>
        /// </summary>
        public void Setup()
        {
            Printer.PrintBoard();
            if (!data.HasValue)
            {
                return;
            }
            (List<Command> commands, Dictionary<string, string> dropletpairs, Dictionary<string, List<string>> contaminated, Dictionary<string, List<string>> contaminates) = data.Value;
            foreach (var dropletpair in dropletpairs)
            {
                Droplet nDrop = new Droplet(dropletpair.Value, dropletpair.Key);
                if (contaminated.ContainsKey(dropletpair.Value))
                {
                    nDrop.SetContam(contaminated[dropletpair.Value]);
                }
                else
                {
                    nDrop.SetContam([]);
                }
                if (!contaminates.ContainsKey(dropletpair.Value))
                {
                    contaminates.Add(dropletpair.Value, []);
                }
                if (!contaminated.ContainsKey(dropletpair.Value))
                {
                    contaminated.Add(dropletpair.Value, []);
                }
                nDrop.ContamLevel = contaminates[dropletpair.Value].Count;
                Printer.PrintLine(nDrop.Substance_Name +" is contaminated by");
                board.Droplets.Add(dropletpair.Key, nDrop);
                nDrop.Thread.Start();
            }

            board.Droplets = board.Droplets.OrderBy(x => x.Value.ContamLevel).ToDictionary();
            board.Droplets.Values.ToList().ForEach(x => Printer.PrintLine(x.ContamLevel));

            foreach (var item in commands)
            {
                item.SetDest();
            }

            foreach (var item1 in commands)
            {
                
                
                foreach (var item2 in commands)
                {
                    if (item1 != item2 && !item1.InputCommands.Contains(item2) && !item2.InputCommands.Contains(item1))
                    {
                        foreach (var item in item1.OutputDroplets)
                        {
                            
                            if (item2.InputDroplets.Contains(item) && !item1.OutputCommands.Contains(item2))
                            {
                                item2.InputCommands.Add(item1);
                                item1.OutputCommands.Add(item2);
                            }
                        }
                    }
                }
            }
            foreach (var item in commands)
            {
                Printer.PrintLine(item.Type + " needs commands: " + item.InputCommands.Count + " and allows: " + item.OutputCommands.Count + " and has nextdest of " + item.CommandDestination);
            }

            commands.FindAll(x => x.InputCommands.Count == 0).ForEach(x => currentCommands.Add(x));
        }

        /// <summary>
        /// Overwrites the current <see cref="Board"/> as a new one, used in tests to reset the environment.
        /// </summary>
        /// <param name="boarddata"></param>
        /// <returns> The created <see cref="Board"/></returns>
        public Board SetBoard(string boarddata)
        {
            Printer.Reset();
            string json = File.ReadAllText(boarddata);
            board = Board.ImportBoardData(json);
            return board;
        }

        /// <summary>
        /// Begins assigning all the <see cref="Mission_Tasks"/> using the <see cref="Command"/>s and begins checking of the program is finished.
        /// </summary>
        public void Start()
        {

            while (currentCommands.Count > 0)
            {
                
                currentCommands.OrderBy(x => x.InputDroplets.Select(y => board.Droplets[y]).Min());

                Command cCommand = currentCommands[0];
                cCommand.CommandDestination ??= cCommand.FindDest();
                Printer.PrintLine(cCommand.ToString() + cCommand.CommandDestination);
                cCommand.ExecuteCommand();
                cCommand.OutputCommands.ForEach(x => 
                    {
                        x.InputCommands.Remove(cCommand);
                        if (x.InputCommands.Count == 0) {
                            currentCommands.Insert(0, x);
                        }
                    });
                currentCommands.Remove(cCommand);
                
                // board.PrintBoardState();
            }

            




            //board.PrintBoardState();
            bool finished = true;
            do
            {
                Thread.Sleep(1000);
                finished = Printer.CurrentlyDone();
                foreach (Droplet item in board.Droplets.Values)
                {
                    if (!finished)
                    {
                        break;
                    }
                    if (item.GetWork().Count > 0 || !(item.ActiveTask == null || item.ActiveTask.IsCompleted))
                    {
                        finished = false;
                        break;
                    }
                }
                if (!((!Settings.Outputting || Outparser.Outparser.OutputQueue.Count == 0) && Outparser.Outparser.cTask == null))
                {
                    finished = false;
                }
            } while (!finished);
            foreach (var item in board.Droplets.Values)
            {
                item.Stop();
            }
            Printer.PrintBoard();
            while (!Printer.CurrentlyDone())
            {
                Thread.Sleep(10);
            }
            Outparser.Outparser.Dispose();
            
        }


        /// <summary>
        /// Checks if the given path that the <see cref="Droplet"/> <paramref name="d"/> is taking intersects any current paths. If it does wait until they finish, then add path to <see cref="dropletPaths"/> and start calculating the path between to point.
        /// <para>If <paramref name="mergeDroplets"/> is specified, <paramref name="d"/> can ignore <see cref="Droplet"/>s that it is merging with.</para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="startPosX"></param>
        /// <param name="startPosY"></param>
        /// <param name="endPosX"></param>
        /// <param name="endPosY"></param>
        /// <param name="mergeDroplets"></param>
        /// <returns><see langword="true"/> if path has been reset after a line intersect, else <see langword="false"/></returns>
        public bool SetPath(Droplet d, int startPosX, int startPosY, int endPosX, int endPosY, List<string>? mergeDroplets = null)
        {
            bool newPath = false;
            Dictionary<string, ((Point start, Point end)? path, UsefulSemaphore sem)> oldPaths = new(dropletPaths);

            lock (d)
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException();
                }
                if (dropletPaths.TryGetValue(d.Name, out ((Point start, Point end)? path, UsefulSemaphore sem) value1))
                {
                    dropletPaths[d.Name] = ((new Point(startPosX, startPosY), new Point(endPosX, endPosY)), value1.sem);
                }
                else
                {
                    dropletPaths.Add(d.Name, ((new Point(startPosX, startPosY), new Point(endPosX, endPosY)), new UsefulSemaphore(1, 1)));
                }
            }
            


            foreach ((var key, var value) in oldPaths)
            {
                if (value.path == null || key == d.Name) continue;
                if (mergeDroplets != null && mergeDroplets.Contains(key)) continue;
                if (LineIntersection.IsIntersecting(dropletPaths[d.Name].path.Value.start, dropletPaths[d.Name].path.Value.end, value.path.Value.start, value.path.Value.end))
                {
                    var oldValue = dropletPaths[d.Name];
                    dropletPaths[d.Name] = (null, dropletPaths[d.Name].sem);
                    Printer.PrintLine(d.Name + " waits on " + key);
                    Monitor.Exit(ModifiedAStar.PathLock);
                    value.sem.CheckOne();
                    Monitor.Enter(ModifiedAStar.PathLock);
                    dropletPaths[d.Name] = oldValue;
                }
            }

            
            dropletPaths[d.Name].sem.TryReleaseOne();
            dropletPaths[d.Name].sem.WaitOne();

            return newPath;

        }

        /// <summary>
        /// Calls the <see cref="SetPath(Droplet, int, int, int, int, List{string}?)"/> function using the positions of the given electrodes.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="mergeDroplets"></param>
        /// <returns><see langword="true"/> if path has been reset after a line intersect, else <see langword="false"/></returns>
        public bool SetPath(Droplet d, Electrode start, Electrode end, List<string>? mergeDroplets = null)
        {
            return SetPath(d, start.EPosX, start.EPosY, end.EPosX, end.EPosY, mergeDroplets);
        }

        /// <summary>
        /// Removes the path of the <see cref="Droplet"/> <paramref name="d"/> in <see cref="dropletPaths"/>.
        /// </summary>
        /// <param name="d"></param>
        public void RemovePath(Droplet d)
        {

            lock (ModifiedAStar.PathLock)
            {
                
                Printer.PrintLine(d.Name + " no longer waits" );
                d.CurrentPath = null;
                if (dropletPaths.TryGetValue(d.Name, out ((Point start, Point end)? path, UsefulSemaphore sem) value))
                {
                    dropletPaths[d.Name] = (null, value.sem);
                }
                else
                {
                    dropletPaths.Add(d.Name, (null, new UsefulSemaphore(1, 1)));
                }
                dropletPaths[d.Name].sem.TryReleaseOne();
            }
            

        }

        /// <summary>
        /// Resets the <see cref="Commander"/>, used by tests to reset the environment.
        /// </summary>
        public void Reset()
        {
            Printer.Reset();
            Parsing.Parsing.Reset();
            Command.Reset();
        }

    }
}
