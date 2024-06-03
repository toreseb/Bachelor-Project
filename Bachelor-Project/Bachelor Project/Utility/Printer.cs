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
    /// <summary>
    /// The <see cref="Printer"/> is an agent that organises and prints for the rest of the program.
    /// </summary>
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

        /// <summary>
        /// The function which starts the <see cref="Printer"/> agent.
        /// </summary>
        public static async void StartAgent()
        {
            await Task.Run(() => { RunAgent(cancellationTokenSource.Token); });
        }

        /// <summary>
        /// The function the <see cref="Printer"/> agent runs.
        /// </summary>
        /// <param name="cancellationToken"></param>
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
                        if (PrintQueue.Count == 0)
                        {
                            continue;
                        }
                        cTask = PrintQueue.Dequeue();
                    }
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

        /// <summary>
        /// Gives print <see cref="Task"/>s to the <see cref="Printer"/>.
        /// </summary>
        /// <param name="task"></param>
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

        /// <summary>
        /// Gives the <see cref="Printer"/> a standard Write <see cref="Task"/>.
        /// </summary>
        /// <param name="message"></param>
        public static void Print(object? message)
        {
            GivePrint(new Task<bool>(() => PrintCommand(message)));
        }

        /// <summary>
        /// Gives the <see cref="Printer"/> a standard WriteLine <see cref="Task"/>.
        /// </summary>
        /// <param name="message"></param>
        public static void PrintLine(object? message = null)
        {
            GivePrint(new Task<bool>(() => PrintCommand(message, true)));
        }
        
        /// <summary>
        /// Gives the <see cref="Printer"/> a custom printboard <see cref="Task"/>.
        /// </summary>
        public static void PrintBoard()
        {
            GivePrint(new Task<bool>(Program.C.board.PrintBoardState));
        }

        /// <summary>
        /// Prints the statements.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="newLine"></param>
        /// <returns><see langword="true"/> when the <see cref="Task"/> is completed.</returns>
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

        
        /// <summary>
        /// Checks if the <see cref="Printer"/> is currently done, so the <see cref="PrintQueue"/> is empty and <see cref="cTask"/> is either null or completed.
        /// </summary>
        /// <returns></returns>
        public static bool CurrentlyDone()
        {
            return (PrintQueue.Count == 0 && cTask == null) || !Settings.Printing;


        }

        /// <summary>
        /// Reset the <see cref="PrintQueue"/> of the <see cref="Printer"/>.
        /// </summary>
        internal static void Reset()
        {
            lock (PrintEnqueue)
            {
                PrintQueue.Clear();
            }
        }
    }
}
