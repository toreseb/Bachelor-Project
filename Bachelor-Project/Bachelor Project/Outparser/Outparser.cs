

using Bachelor_Project.Simulation;
using Bachelor_Project.Utility;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace Bachelor_Project.Outparser
{
    static public class Outparser
    {

        public static Queue<Task<bool>> OutputQueue = [];
        public static Task<bool>? cTask = null;
        static bool LastTick = false;
        static List<string> Seen = [];
        static Dictionary<string,(int elapsedTime, int totalTime)> Waiters = [];

        static Stopwatch WatchSinceLastTick = new();
        static StreamWriter Writer;

        private static CancellationTokenSource cancellationTokenSource;
        private static ManualResetEventSlim OutputAvailableEvent = new ManualResetEventSlim(false);

        private static ManualResetEventSlim TickEvent = new ManualResetEventSlim(false);

        private static readonly object OutputEnqueue = new object();
        static Outparser()
        {
            if (Settings.Outputting)
            {
                string docPath = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Outparser\\Output\\";
                string fileName = Settings.ProtocolName+"_Output.basm";
                Writer = new StreamWriter(Path.Combine(docPath, fileName));
                cancellationTokenSource = new CancellationTokenSource();
                StartAgent();
            }
        }

        public static async void StartAgent()
        {
            await Task.Run(() => { RunAgent(cancellationTokenSource.Token); });
        }

        public static void RunAgent(CancellationToken cancellationToken)
        {
            SetupOutput();

            // Run while cancellation has not been requested
            while (!cancellationToken.IsCancellationRequested)
            {
                // Needs to wait for task to be put in a task queue by Commander, then execute task and wait again

                // Wait for work to become available
                OutputAvailableEvent.Wait(cancellationToken);

                // Reset signal
                OutputAvailableEvent.Reset();

                // Do thing
                while (OutputQueue.Count > 0)
                {
                    lock (OutputEnqueue)
                    {
                        cTask = OutputQueue.Dequeue();
                    }
                    cTask.RunSynchronously();
                        
                        
                    cTask = null;
                }

            }
        }

        private static void GiveOutput(Task<bool> task) // Pos is from the right, so last. If pos is null it inserts at left
        {
            lock (OutputEnqueue)
            {
                OutputQueue.Enqueue(task);
                // Signal that work is available
                OutputAvailableEvent.Set();
            }

        }

        public static void ElectrodeOn(Electrode e, Droplet? d = null)
        {
            Printer.PrintLine("setel " + e.DriverID + " " + e.ElectrodeID + " \\r");
            lock (OutputEnqueue)
            {
                e.Status = 1;
                GiveOutput(new(() => ChangeEl(d, e.ElectrodeID, true)));
                OutputAvailableEvent.Set();
            }
            
        }

        public static void ElectrodeOff(Electrode e, Droplet? d = null) 
        {
            Printer.PrintLine("clrel " + e.DriverID + " " + e.ElectrodeID + " \\r");
            lock (OutputEnqueue)
            {
                e.Status = 0;
                GiveOutput(new(() => ChangeEl(d, e.ElectrodeID, false)));
                OutputAvailableEvent.Set();
            }
            
        }

        public static void SetupOutput()
        {
            Writer.WriteLine(".text");
            Writer.WriteLine("main:");
            Writer.Flush();
            TickEvent.Set();
        }

        

        public static bool ChangeEl(Droplet? d, int ID, bool set)
        {
            if (d != null)
            {
                if (Seen.Contains(d.Name))
                {
                    Writer.WriteLine("    TICK;");
                    LastTick = true;
                    foreach ((string droplet, (int elapsedTime, int totalTime)) in Waiters)
                    {
                        Waiters[droplet] = (elapsedTime + Settings.TimeStep,totalTime);
                    }
                    Seen.Clear();
                }
                
                Seen.Add(d.Name);
            }
            LastTick = false;
            string total = "    ";
            switch(set)
            {
                case true:
                    total += "SETELI ";
                    break;
                case false:
                    total += "CLRELI ";
                    break;
            }
            total += ID + ";" ;
            Writer.WriteLine(total);
            return true;
        }

        public static bool Tick()
        {
            Writer.WriteLine("    TICK;");
            LastTick = true;
            return true;
        }

        public static void WaitDroplet(Droplet d, int milliseconds)
        {
            if (Settings.ConnectedToHardware)
            {
                Thread.Sleep(milliseconds);
            }
            else
            {
                GiveOutput(new(() => Wait(d, milliseconds)));
            }
            
        }

        /// <summary>
        /// This is only called if the software is not connected to the hardware
        /// </summary>
        /// <param name="d"></param>
        /// <param name="milliseconds"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static bool Wait(Droplet d, int milliseconds, int start = 0)
        {
            int elapsedTime = start;
            lock (OutputEnqueue)
            {
                if (/*OutputQueue.Count == 0*/ true) // TODO: Make it so not everything wait when a droplet waits. VERY DIFFICULT
                {

                    if (!LastTick) // If the last one wasn't a tick, it needs 2 ticks to get a lone tick for the extra time.
                    {
                        Tick();
                        elapsedTime += Settings.TimeStep;
                    }
                    while (elapsedTime < milliseconds)
                    {
                        elapsedTime += Settings.TimeStepOnSingleTick;
                        Tick();

                    }

                }
            }
            
            return true;
        }
        

        public static void Dispose()
        {
            if (Writer != null)
            {
                Writer.WriteLine("    TICK;");
                Writer.Flush();
                Writer.Dispose();
            }

        }


    }
}
