using Bachelor_Project.Outparser;
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

namespace Bachelor_Project.Simulation
{
    public class Commander
    {


        public (List<Command> commands, Dictionary<string, string> dropletpairs, Dictionary<string, List<string>> contaminated, Dictionary<string, List<string>> contaminates)? data;
        public Board board;
        public Dictionary<string, ((Point start, Point end)? path, UsefullSemaphore sem)> dropletPaths;
        private List<Command> currentCommands = [];

        public Commander((List<Command>, Dictionary<string, string>, Dictionary<string, List<string>>, Dictionary<string, List<string>>)? data, string boarddata)
        {
            this.data = data;
            string json = File.ReadAllText(boarddata);
            board = Board.ImportBoardData(json);
            dropletPaths = [];
        }

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
            commands = commands.OrderBy(x => x.InputCommands.Count).ToList();

            commands.FindAll(x => x.InputCommands.Count == 0).ForEach(x => currentCommands.Add(x));
        }

        public Board SetBoard(string boarddata)
        {
            string json = File.ReadAllText(boarddata);
            board = Board.ImportBoardData(json);
            return board;
        }

        public void Start()
        {

            while (currentCommands.Count > 0)
            {
                
                currentCommands.OrderBy(x => x.InputDroplets.Select(y => board.Droplets[y]).Min());

                Command cCommand = currentCommands[0];
                cCommand.CommandDestination ??= cCommand.FindDest();
                Printer.PrintLine(cCommand.ToString() + cCommand.CommandDestination);
                cCommand.ExecuteCommand();
                cCommand.OutputCommands.ForEach(x => x.InputCommands.Remove(cCommand));
                cCommand.OutputCommands.ForEach(x => 
                    {
                        if (x.InputCommands.Count == 0) {
                            currentCommands.Add(x);
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
            Outparser.Outparser.Dispose();
            
        }
        /// <summary>
        /// Return true if path has been reset after a line intersect, else false
        /// </summary>
        /// <param name="d"></param>
        /// <param name="startPosX"></param>
        /// <param name="startPosY"></param>
        /// <param name="endPosX"></param>
        /// <param name="endPosY"></param>
        /// <param name="mergeDroplets"></param>
        /// <returns></returns>
        public bool SetPath(Droplet d, int startPosX, int startPosY, int endPosX, int endPosY, List<string>? mergeDroplets = null)
        {
            bool newPath = false;
            Dictionary<string, ((Point start, Point end)? path, UsefullSemaphore sem)> oldPaths = new(dropletPaths);
            foreach ((var key, var value) in oldPaths)
            {
                if (value.path == null || !dropletPaths.ContainsKey(d.Name) || key == d.Name || dropletPaths[d.Name].path == null) continue;
                if (mergeDroplets != null && mergeDroplets.Contains(key)) continue;
                if (LineIntersection.IsIntersecting(dropletPaths[d.Name].path.Value.start, dropletPaths[d.Name].path.Value.end, value.path.Value.start, value.path.Value.end))
                {
                    var oldValue = dropletPaths[d.Name];
                    dropletPaths[d.Name] = (null, dropletPaths[d.Name].sem);
                    Monitor.Exit(ModifiedAStar.PathLock);
                    value.sem.Check();
                    Monitor.Enter(ModifiedAStar.PathLock);
                    Board b = Program.C.board;
                    if (!(d.CurrentPath == null || d.CurrentPath.Value.path.Count == 0))
                    {
                        newPath = true;
                        d.CurrentPath = ModifiedAStar.FindPath(d, d.CurrentPath.Value.path.Last().Item1, mergeDroplets: mergeDroplets, start: d.CurrentPath.Value.path[0].Item1);
                    }
                    dropletPaths[d.Name] = oldValue;
                }
            }

            if (dropletPaths.Keys.Contains(d.Name)){
                dropletPaths[d.Name] = ((new Point(startPosX, startPosY), new Point(endPosX, endPosY)), dropletPaths[d.Name].sem);
            }
            else
            {
                dropletPaths.Add(d.Name, ((new Point(startPosX, startPosY), new Point(endPosX, endPosY)), new UsefullSemaphore(1,1)));
            }
            dropletPaths[d.Name].sem.TryReleaseOne();
            dropletPaths[d.Name].sem.WaitOne();

            return newPath;

        }

        public bool SetPath(Droplet d, Electrode start, Electrode end, List<string>? mergeDroplets = null)
        {
            return SetPath(d, start.ePosX, start.ePosY, end.ePosX, end.ePosY, mergeDroplets);
        }

        public void RemovePath(Droplet d)
        {
            lock (ModifiedAStar.PathLock)
            {
                d.CurrentPath = null;
                if (dropletPaths.Keys.Contains(d.Name))
                {
                    dropletPaths[d.Name] = (null, dropletPaths[d.Name].sem);
                }
                else
                {
                    dropletPaths.Add(d.Name, (null, new UsefullSemaphore(1, 1)));
                }
                dropletPaths[d.Name].sem.TryReleaseOne();
            }
            

        }

    }
}
