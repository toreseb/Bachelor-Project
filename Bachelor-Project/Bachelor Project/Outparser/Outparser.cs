

using Bachelor_Project.Simulation;
using Bachelor_Project.Utility;
using static System.Net.Mime.MediaTypeNames;

namespace Bachelor_Project.Outparser
{
    static public class Outparser
    {

        public static Queue<(Droplet? d,string text)> OutputQueue = [];
        public static string? cOutput = null; 
        static List<string> Seen = [];

        static StreamWriter Writer;

        private static CancellationTokenSource cancellationTokenSource;
        private static ManualResetEventSlim OutputAvailableEvent = new ManualResetEventSlim(false);

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
                        (Droplet? d, cOutput) = OutputQueue.Dequeue();
                        if(d != null)
                        {
                            if (Seen.Contains(d.Name))
                            {
                                Writer.WriteLine("    TICK;");
                                Seen.Clear();
                                Seen.Add(d.Name);
                            }
                            else
                            {
                                Seen.Add(d.Name);
                            }
                        }
                        Writer.WriteLine(cOutput);
                        cOutput = null;
                    }



                }

            }
        }

        public static void ElectrodeOn(Electrode e, Droplet? d = null)
        {
            Printer.PrintLine("setel " + e.DriverID + " " + e.ElectrodeID + " \\r");
            lock (OutputEnqueue)
            {
                e.Status = 1;
                OutputQueue.Enqueue((d,ChangeEl(e.ElectrodeID, true)));
                OutputAvailableEvent.Set();
            }
            
        }

        public static void ElectrodeOff(Electrode e, Droplet? d = null) 
        {
            Printer.PrintLine("clrel " + e.DriverID + " " + e.ElectrodeID + " \\r");
            lock (OutputEnqueue)
            {
                e.Status = 0;
                OutputQueue.Enqueue((d,ChangeEl(e.ElectrodeID, false)));
                OutputAvailableEvent.Set();
            }
            
        }

        public static void SetupOutput()
        {
            Writer.WriteLine(".text");
            Writer.WriteLine("main:");
            Writer.Flush();
        }

        

        public static string ChangeEl(int ID, bool set)
        {
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
            return total;
        }

        public static void Dispose()
        {
            Writer.WriteLine("    TICK;");
            Writer.Flush();
            Writer.Dispose();
        }


    }
}
