using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bachelor_Project.Utility
{
    public static class Printer
    {
        private static CancellationTokenSource cancellationTokenSource;
        private static ManualResetEventSlim PrintAvailableEvent = new ManualResetEventSlim(false);

        private static Queue<Task<bool>> PrintQueue = [];
        public static Task<bool>? cTask;
        private static readonly object PrintEnqueue = new object();

        static Printer()
        {
            if (Settings.Printing)
            {
                cTask = null;
                cancellationTokenSource = new CancellationTokenSource();
                StartAgent();
            }
        }

        public static async void StartAgent()
        {
            await Task.Run(() => { RunAgent(cancellationTokenSource.Token); });
        }

        private static async void RunAgent(CancellationToken cancellationToken)
        {
            
            // Run while cancellation has not been requested
            while (!cancellationToken.IsCancellationRequested)
            {
                // Needs to wait for task to be put in a task queue by Commander, then execute task and wait again

                // Wait for work to become available
                PrintAvailableEvent.Wait(cancellationToken);

                // Reset signal
                PrintAvailableEvent.Reset();

                // Do thing
                while (PrintQueue.Count > 0)
                {
                    lock (PrintEnqueue)
                    {
                        cTask = PrintQueue.Dequeue();
                        if (cTask.Status != TaskStatus.RanToCompletion)
                        {
                            cTask.RunSynchronously();
                        }

                        try
                        {
                            cTask.Wait(cancellationToken);
                            cTask = null;
                        }
                        catch (Exception)
                        {
                            cTask = null;
                            return;
                        }
                    }
                    

                    
                }

            }
        }

        private static void GivePrint(Task<bool> task)
        {
            lock (PrintEnqueue)
            {
                // TODO: Implement task queue
                PrintQueue.Enqueue(task);
                // Signal that work is available
                PrintAvailableEvent.Set();
            }
            
        }

        public static void Print(object? message)
        {
            GivePrint(new Task<bool>(() => PrintCommand(message)));
        }

        public static void PrintLine(object? message = null)
        {
            GivePrint(new Task<bool>(() => PrintCommand(message, true)));
        }

        private static bool PrintCommand(object? message, bool newLine = false)
        {
            if (message == null && newLine == true)
            {
                Console.WriteLine();
            }
            else
            {
                if (newLine == true)
                {
                    Console.WriteLine(message.ToString());
                }
                else
                {
                    Console.Write(message.ToString());
                }
                
            }
            return true;
        }

        public static void PrintBoard()
        {
            GivePrint(new Task<bool>(Program.C.board.PrintBoardState));
        }

        public static bool CurrentlyDone()
        {
            return PrintQueue.Count == 0 && cTask == null;


        }
    }
}
