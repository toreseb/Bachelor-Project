﻿using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        public float Volume { get; set; }
        public List<Electrode> Occupy { get; set; } = [];
        public bool snekMode { get; set; } = true;
        public List<string> Contamintants { get; set; } = [];
        public int ContamLevel { get; set; } = 0;

        // Used for threading
        private CancellationTokenSource cancellationTokenSource;
        private ManualResetEventSlim workAvailableEvent = new ManualResetEventSlim(false);

        private Queue<Task> TaskQueue = [];


        public Droplet(string substance_name, string name = "") : base(-1, -1, 1, 1, name)
        {
            Temperature = 20;
            Substance_Name = substance_name;
            Color = GetColor(Substance_Name);
            
            

            cancellationTokenSource = new CancellationTokenSource();

        }

        public async void StartAgent()
        {
            await Task.Run(() => { RunAgent(cancellationTokenSource.Token); });
        }

        public async void RunAgent(CancellationToken cancellationToken)
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
                while(TaskQueue.Count > 0)
                {
                    Task cTask = TaskQueue.Dequeue();
                    Console.WriteLine("Droplet " + Name + " is doing work");
                    cTask.Start();
                    
                    cTask.Wait(cancellationToken);
                    Console.WriteLine("Droplet " + Name + " has done work");
                }

            }
        }

        public void GiveWork(Task task)
        {
            // TODO: Implement task queue
            TaskQueue.Enqueue(task);
            // Signal that work is available
            workAvailableEvent.Set();
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        private string GetColor(string substance_name)
        {
            return "0000FF"; // Needs to be changed to a color based on the substance name.
        }


        public void SetSizes(float Volume)
        {
            this.Volume = Volume;
            Size = ((int)Volume/12)+1;
        }

        internal void SetContam(List<string> list)
        {
            Contamintants = list;
        }
    }
}
