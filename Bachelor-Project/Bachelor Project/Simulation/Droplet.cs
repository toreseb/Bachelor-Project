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
    /// <summary>
    /// The class of <see cref="Droplet"/>s which are the agents moving across the <see cref="Board"/>
    /// </summary>
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

        public Apparatus? nextDestination = null;
        public Electrode? nextElectrodeDestination = null;

        // Used for threading
        private CancellationTokenSource cancellationTokenSource;
        private ManualResetEventSlim workAvailableEvent = new ManualResetEventSlim(false);

        private Queue<Task> TaskQueue = [];

        private object TaskEnqueue = new object();

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
            Color = GetColor();

            cancellationTokenSource = new CancellationTokenSource();

            Thread = new Thread(new ThreadStart(StartAgent));
            Thread.Name = name;

            
            

        }

        /// <summary>
        /// Finds the closest <see cref="Electrode"/> in this <see cref="Droplet"/> to the given <see cref="Apparatus"/> <paramref name="a"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <returns>The found closest <see cref="Electrode"/></returns>
        private Electrode GetClosestPartToApparature(Apparatus a)
        {
            (int x, int y) = a.GetCenter();
            Electrode? closestElectrode = null;
            double minDistance = double.MaxValue;
            foreach (Electrode e in Occupy)
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
            }
            return closestElectrode;
        }

        /// <summary>
        /// Sets the location that this <see cref="Droplet"/> is trying to go next
        /// </summary>
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

        /// <summary>
        /// Finds the closest <see cref="Electrode"/> in the <see cref="Apparatus"/> <paramref name="a"/> to this <see cref="Droplet"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <returns>The found closest <see cref="Electrode"/></returns>
        /// <exception cref="InvalidDataException"></exception>
        public Electrode GetClosestFreePointer(Apparatus a)
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
                center = GetClosestPartToApparature(a);
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
                throw new InvalidDataException("Apparatus has no pointers");
            }
            return closestElectrode;
        }

        /// <summary>
        /// The function to start the <see cref="Droplet"/> as an agent.
        /// </summary>
        private async void StartAgent()
        {
           RunAgent(cancellationTokenSource.Token);
        }

        /// <summary>
        /// The function whith the <see cref="Droplet"/> runs on its <see cref="Thread"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
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
                        lock(TaskQueue)
                        {
                            ActiveTask = TaskQueue.Dequeue();
                        }
                        
                        Printer.PrintLine("Droplet " + Name + " is doing work");
                        
                        try
                        {
                            ActiveTask.RunSynchronously();
                            if (ActiveTask.IsFaulted)
                            {
                                throw ActiveTask.Exception;
                            }
                            ActiveTask = null;
                        }
                        catch (ThreadInterruptedException e)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                Printer.PrintLine("Droplet " + Name + " has been stopped");
                                ActiveTask = null;
                                return;
                            }
                            else
                            {
                                throw;
                            }

                        }
                        catch (AggregateException e)
                        {
                            if(e.InnerException is not NewWorkException)
                            {
                                throw;
                            }
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

        /// <summary>
        /// The method the <see cref="Commander"/> uses to assign the <see cref="Task"/> <paramref name="task"/> to this <see cref="Droplet"/>.
        /// </summary>
        /// <param name="task"></param>
        public void GiveWork(Task task)
        {
            lock (TaskQueue)
            {
                TaskQueue.Enqueue(task);
                // Signal that work is available
                workAvailableEvent.Set();

            }
            
        }

        /// <summary>
        /// Getter for <see cref="TaskQueue"/>
        /// </summary>
        /// <returns></returns>
        public List<Task> GetWork()
        {
            return TaskQueue.ToList();
        }

        /// <summary>
        /// Stops the <see cref="Thread"/> of this <see cref="Droplet"/>. Called when the <see cref="Droplet"/> needs to be removed from the <see cref="Board"/>.
        /// </summary>
        public void Stop()
        {
            cancellationTokenSource.Cancel();
            Thread.Interrupt();
            Removed = true;
        }

        /// <summary>
        /// Color is not entirely implemented.
        /// </summary>
        /// <returns></returns>
        private string GetColor()
        {
            return "0000FF"; // Needs to be changed to a color based on the substance name.
        }

        /// <summary>
        /// Calculates and sets the <see cref="Size"/> of this <see cref="Droplet"/> based off of <paramref name="Volume"/>. Also sets <see cref="Volume"/>
        /// </summary>
        /// <param name="Volume"></param>
        public void SetSizes(double Volume)
        {
            this.Volume = Volume;
            Size = ((int)Volume/12)+1;
        }

        /// <summary>
        /// Sets this <see cref="Droplets"/> <see cref="Contamintants"/>.
        /// </summary>
        /// <param name="list"></param>
        public void SetContam(List<string> list)
        {
            Contamintants = list;
        }
        
        /// <summary>
        /// ToString overwriter.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name + " " + Substance_Name + " OccupyCount = " + Occupy.Count;
        }

        /// <summary>
        /// Changes the <see cref="Substance_Name"/> of this <see cref="Droplet"/>.
        /// </summary>
        /// <param name="newType"></param>
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

        /// <summary>
        /// Changes the <see cref="Temperature"/> of this <see cref="Droplet"/>.
        /// </summary>
        /// <param name="actualTemperature"></param>
        public void ChangeTemp(int actualTemperature)
        {
            Temperature = actualTemperature;
        }

        /// <summary>
        /// Removes this <see cref="Droplet"/> from the <see cref="Board"/> by removing its <see cref="Occupy"/> and stoppong its <see cref="Thread"/>
        /// </summary>
        public void RemoveFromBoard()
        {
            lock (ModifiedAStar.PathLock) // Also needs the PathLock, so removepath doesn't create a deadlock
            {
                lock (this)
                {
                    Program.C.RemovePath(this);
                    List<Electrode> oldElectrode = new(Occupy);
                    foreach (var item in oldElectrode)
                    {
                        Droplet_Actions.MoveOffElectrode(this, item);

                    }
                    SetSizes(0);

                    SnekMode = false;
                    Printer.PrintLine("Droplet " + Name + " has been stopped");
                    Stop();
                }
            }
            
            
        }


        /// <summary>
        /// This <see cref="Droplet"/> gains all the electrodes of <paramref name="d"/>, and removes <paramref name="d"/> from the <see cref="Board"/>
        /// </summary>
        /// <param name="d"></param>
        public void TakeOver(Droplet d)
        {
            lock (ModifiedAStar.PathLock) // Also take pathLock, to make sure removeboard doesn't create deadlock
            {
                lock (d)
                {
                    SnekMode = false;
                    List<Electrode> elec = new(d.Occupy);
                    foreach (var item in elec)
                    {
                        Droplet_Actions.TakeOverElectrode(this, item);
                    }
                    if (Volume == 0)
                    {
                        SetSizes(d.Volume);
                    }
                    d.RemoveFromBoard();
                }
            }
            
            
        }

        /// <summary>
        /// Empties the <see cref="SnekList"/> and turns <see cref="SnekMode"/> <see langword="false"/> to go amorphous.
        /// </summary>
        public void GoAmorphous()
        {
            SnekMode = false;
            SnekList = [];
        }

        
    }
}
