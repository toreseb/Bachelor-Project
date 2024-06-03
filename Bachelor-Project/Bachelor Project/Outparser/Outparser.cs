

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
        /// <summary>
        /// Starts the <see cref="Outparser"/> agent
        /// </summary>
        public static async void StartAgent()
        {
            await Task.Run(() => { RunAgent(cancellationTokenSource.Token); });
        }

        /// <summary>
        /// The functions which the <see cref="Outparser"/> loops.
        /// </summary>
        /// <param name="cancellationToken"></param>
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

        /// <summary>
        /// A function to assign outputs to the .basm file.
        /// </summary>
        /// <param name="task"></param>
        private static void GiveOutput(Task<bool> task)
        {
            lock (OutputEnqueue)
            {
                OutputQueue.Enqueue(task);
                // Signal that work is available
                OutputAvailableEvent.Set();
            }

        }

        /// <summary>
        /// Turns on the stated <see cref="Electrode"/>, and sends a task to the <see cref="Outparser"/> to write the bio-assembly
        /// </summary>
        /// <param name="e"></param>
        /// <param name="d"></param>
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

        /// <summary>
        /// Turns off the stated <see cref="Electrode"/>, and sends a task to the <see cref="Outparser"/> to write the bio-assembly
        /// </summary>
        /// <param name="e"></param>
        /// <param name="d"></param>
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

        /// <summary>
        /// Initial bio-assembly in the top of the file, always necessary
        /// </summary>
        /// <param name="e"></param>
        /// <param name="d"></param>
        public static void SetupOutput()
        {
            Writer.WriteLine(".text");
            Writer.WriteLine("main:");
            Writer.Flush();
            TickEvent.Set();
        }


        /// <summary>
        /// The function which writes the correct bio-assembly instructions.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="ID"></param>
        /// <param name="set"></param>
        /// <returns>Returns <see langword="true"/> when the task is finished</returns>
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

        /// <summary>
        /// Creates a tick in the bio-assembly.
        /// </summary>
        /// <returns>Returns <see langword="true"/> when the task is completed</returns>
        public static bool Tick()
        {
            Writer.WriteLine("    TICK;");
            LastTick = true;
            return true;
        }

        /// <summary>
        /// The fucntion used by <see cref="Droplet"/>s to <see langword="wait"/>, function changes depend on <see cref="Settings.ConnectedToHardware"/>.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="milliseconds"></param>
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
        /// This is only called if <see cref="Settings.ConnectedToHardware"/> is <see langword="false"/>.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="milliseconds"></param>
        /// <param name="start"></param>
        /// <returns>Return <see langword="true"/> when the task is finished</returns>
        public static bool Wait(Droplet d, int milliseconds, int start = 0)
        {
            int elapsedTime = start;
            lock (OutputEnqueue)
            {
                if (true)
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
        
        /// <summary>
        /// Dispose of the current <see cref="Outparser"/>, used by the <see cref="Commander"/> when exiting the <see cref="Program"/>.
        /// </summary>
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
