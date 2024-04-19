using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Simulation.Agent_Actions;
using Bachelor_Project.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    public class Droplet: TileEntity
    {
        // In creation of a Droplet, the following is needed:
        // Droplet <name> = new Droplet();
        // await (?) <name>.StartAgent();


        public string Substance_Name { get; set; }
        public string Color { get; set; }
        public float Temperature { get; set; }

        public int Size { get; set; }
        public double Volume { get; set; }
        public List<Electrode> Occupy { get; set; } = [];
        public LinkedList<Electrode> SnekList { get; set; } = [];
        public bool SnekMode = false;
        public List<string> Contamintants { get; set; } = []; //Which substance types the droplet is contaminated by, ergo which contaminations it can't pass over.
        public int ContamLevel { get; set; } = 0;
        public (List<(Electrode, Direction?)> path, int inside)? CurrentPath = null; //List of electrode and directions is the path with the moving direction, int is the amount of movement the path takes inside the droplet
        public (int?, Direction?) SquareInfo { get; set; } // Square used for constant width towards edges.

        public bool Waiting = true; // If a droplet is waiting, it's movement isn't important, so it can be changed. This is for actions
        public bool Important = true; // If a droplet is important, it's movement is important, so it can't be changed. This is for tasks
        public bool MergeReady = false; // Determines if a droplet is in the state where it is possible to merge.

        public Apparature? nextDestination = null;
        public Electrode? nextElectrodeDestination = null;

        // Used for threading
        private CancellationTokenSource cancellationTokenSource;
        private ManualResetEventSlim workAvailableEvent = new ManualResetEventSlim(false);

        private Queue<Task> TaskQueue = [];

        public Task? ActiveTask;

        public bool Inputted = false;

        public bool Removed = false;

        public int TriedMoveCounter = 0;
        public int TriedResetCounter = 0;

        public Thread Thread;


        public Droplet(string substance_name, string name = "") : base(-1, -1, 1, 1, name)
        {
            Temperature = 20;
            Substance_Name = substance_name;
            Color = GetColor(Substance_Name);

            cancellationTokenSource = new CancellationTokenSource();

            Thread = new Thread(new ThreadStart(StartAgent));

            
            

        }

        public static Electrode GetClosestPartToApparature(Droplet d, Apparature a)
        {
            (int x, int y) = a.GetCenter();
            Electrode? closestElectrode = null;
            double minDistance = double.MaxValue;
            foreach (Electrode e in d.Occupy) // BROKEN - There is a problem when splitting as there is nothing in Occupy
            {
                double distance = Electrode.GetDistance(e, new Electrode(x, y));
                if (distance < minDistance)
                {
                    closestElectrode = e;
                    minDistance = distance;
                }
            }

            if (closestElectrode == null)
            {
                return a.pointers[0];
                //throw new Exception("Droplet has no electrodes");
            }
            return closestElectrode;
        }

        public void SetNextElectrodeDestination()
        {
            if (nextDestination != null)
            {
                nextElectrodeDestination = GetClosestFreePointer(nextDestination);
            }
            else if (nextElectrodeDestination == null)
            {
                nextElectrodeDestination = Occupy[0];
                // TODO: think of making this better
                //throw new NullReferenceException("Can't find the closest electrode destination, when next destination is null");
            }
            
        }

        public Electrode GetClosestFreePointer(Apparature a)
        {
            Electrode? closestElectrode = null;
            double minDistance = double.MaxValue;
            Electrode center;
            if (SnekMode)
            {
                center = SnekList.First();
            }
            else
            {
                center = GetClosestPartToApparature(this, a); // DEBUG: Droplet 1 is stopped here??
            }
            foreach (Electrode electrode in a.pointers)
            {
                if (!Droplet_Actions.CheckContaminations(this, [electrode]))
                {
                    continue;
                }
                double distance = Electrode.GetDistance(electrode, center);
                if (distance < minDistance)
                {
                    closestElectrode = electrode;
                    minDistance = distance;
                }
            }
            if (closestElectrode == null)
            {
                throw new Exception("No free pointers");
            }
            return closestElectrode;
        }


        private async void StartAgent()
        {
           RunAgent(cancellationTokenSource.Token);
        }

        public async void RunAgent(CancellationToken cancellationToken)
        {
            try
            {
                // Run while cancellation has not been requested
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Needs to wait for task to be put in a task queue by Commander, then execute task and wait again

                    // Wait for work to become available
                    workAvailableEvent.Wait(cancellationToken);

                    // Reset signal
                    workAvailableEvent.Reset();

                    // Do thing
                    while (TaskQueue.Count > 0)
                    {
                        ActiveTask = TaskQueue.Dequeue();
                        Printer.PrintLine("Droplet " + Name + " is doing work");
                        ActiveTask.Start();
                        if (Name == "drop4")
                        {
                            int a = 2;
                        }
                        try
                        {

                            ActiveTask.Wait(cancellationToken);
                            ActiveTask = null;
                        }
                        catch (Exception e)
                        {
                            Printer.PrintLine("Droplet " + Name + " has been stopped");
                            ActiveTask = null;
                            return;
                        }

                        Printer.PrintLine("Droplet " + Name + " has done work");
                        ActiveTask = null;
                    }

                }
            }
            catch (ThreadInterruptedException)
            {

            }
            catch (OperationCanceledException)
            {

            }
            
        }

        public void GiveWork(Task task)
        {
            // TODO: Implement task queue
            TaskQueue.Enqueue(task);
            // Signal that work is available
            workAvailableEvent.Set();
        }

        public List<Task> GetWork()
        {
            return TaskQueue.ToList();
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        private string GetColor(string substance_name)
        {
            return "0000FF"; // Needs to be changed to a color based on the substance name.
        }


        public void SetSizes(double Volume)
        {
            this.Volume = Volume;
            Size = ((int)Volume/12)+1;
        }

        internal void SetContam(List<string> list)
        {
            Contamintants = list;
        }
        public override string ToString()
        {
            return Name + " " + Substance_Name + " OccupyCount = " + Occupy.Count;
        }

        public void ChangeType(string newType)
        {
            Substance_Name = newType;
            if (Program.C.data != null && Program.C.data.Value.contaminated.ContainsKey(newType))
            {
                SetContam(Program.C.data.Value.contaminated[newType]);
            }
            else
            {
                SetContam([]);
            }
            
        }

        internal void RemoveFromBoard()
        {
            // TODO: Remove it entirely from the board
            List<Electrode> oldElectrode = new(Occupy);
            foreach (var item in oldElectrode)
            {
                Droplet_Actions.MoveOffElectrode(this, item);

            }
            SnekMode = false;
            Removed = true;
            Printer.PrintLine("Droplet " + Name + " has been stopped");
            Stop();
            Thread.Interrupt();
        }


        /// <summary>
        /// This droplet gains all the electrodes of d, and Removes d
        /// </summary>
        /// <param name="d"></param>
        public void TakeOver(Droplet d)
        {
            SnekMode = false;
            List<Electrode> elec = new(d.Occupy);
            foreach (var item in elec)
            {
                Droplet_Actions.TakeOverElectrode(this, item);
            }
            d.RemoveFromBoard();
        }
    }
}
