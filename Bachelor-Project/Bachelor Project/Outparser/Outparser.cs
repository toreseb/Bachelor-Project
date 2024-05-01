

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
        static List<string> Seen = [];

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

                        cTask.RunSynchronously();
                        
                        
                        cTask = null;
                    }



                }

            }
        }

        private static void GiveOutput(Task<bool> task)
        {
            lock (OutputEnqueue)
            {
                // TODO: Implement task queue
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
            WatchSinceLastTick.Start();
            TickEvent.Set();
        }

        

        public static bool ChangeEl(Droplet? d, int ID, bool set)
        {
            if (d != null)
            {
                if (Seen.Contains(d.Name))
                {
                    Writer.WriteLine("    TICK;");
                    WatchSinceLastTick.Reset();
                    TickEvent.Set();
                    Seen.Clear();
                }
                Seen.Add(d.Name);
            }

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
            return true;
        }

        public static void WaitDroplet(Droplet d, int milliseconds)
        {
            GiveOutput(new(() => Wait(milliseconds)));
        }

        public static bool Wait(int milliseconds)
        {
            int elapsedTime = 0;
            bool set = true;
            while (elapsedTime < milliseconds)
            {
                set = TickEvent.Wait(Settings.TimeToWaitBeforeTicking);
                TickEvent.Reset();
                if (!set)
                {
                    GiveOutput(new(() => Tick()));
                    elapsedTime += Settings.TimeStepOnSingleTick;
                }
                else
                {
                    elapsedTime += Settings.TimeStep;
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
