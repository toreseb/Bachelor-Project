﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Utility;
using Bachelor_Project.Electrode_Types.Actuator_Types;

namespace Bachelor_Project.Simulation.Agent_Actions.Tests
{
    [TestClass()]
    public class Mission_TasksTests
    {


        static string inputfiles = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Input Files\\";
        static string testBoardData = "TestBoardData.json";
        static string testBoardDataBig = "TestBoardDataBig.json";

        static string testBoardDataLocation = inputfiles + "\\" + testBoardData;
        static string testBoardDataBigLocation = inputfiles + "\\" + testBoardDataBig;

        static string specialBoardDataLocation = inputfiles + "\\" + "Special Boards";

        static Board board;

        [TestInitialize]
        public void Setup()
        {
            Commander C = new(null, testBoardDataLocation);
            Program.C = C;
            board = Program.C.board;
        }

        [TestMethod()]
        public void InputDropletTest()
        {
            board = Program.C.SetBoard(testBoardDataLocation);
            Droplet Wat1 = new("Water", "Wat1");
            board.Droplets.Add("Wat1", Wat1);
            Wat1.Thread.Start();

            Assert.AreEqual(0, Wat1.Occupy.Count);

            Task input1 = new(() => Droplet_Actions.InputDroplet(Wat1, board.Input["in0"], 12));
            Wat1.GiveWork(input1);
            input1.Wait();
            Assert.AreEqual(2, Wat1.Occupy.Count);
        }

        [TestMethod()]
        public void OutputDropletTest()
        {
            board = Program.C.SetBoard(testBoardDataLocation);
            Droplet Wat1 = new("Water", "Wat1");
            board.Droplets.Add("Wat1", Wat1);
            Wat1.Thread.Start();

            Task input1 = new(() => Mission_Tasks.InputDroplet(Wat1, board.Input["in0"], 12));
            Wat1.GiveWork(input1);
            input1.Wait();
            Assert.AreEqual(2, Wat1.Occupy.Count);
            Assert.AreEqual(false, Wat1.Removed);
            Task output1 = new(() => Mission_Tasks.OutputDroplet(Wat1, board.Output["out0"]));
            Wat1.GiveWork(output1);
            output1.Wait();
            Assert.AreEqual(0, Wat1.Occupy.Count);
            Assert.AreEqual(true, Wat1.Removed);

        }

        [TestMethod()]
        public void MixDropletsTest()
        {
            board = Program.C.SetBoard(testBoardDataLocation);
            Droplet Wat1 = new("Water", "Wat1");
            board.Droplets.Add("Wat1", Wat1);
            Wat1.Thread.Start();

            Task input1 = new(() => Mission_Tasks.InputDroplet(Wat1, board.Input["in0"], 12));
            Wat1.GiveWork(input1);
            input1.Wait();
            Assert.AreEqual(2, Wat1.Occupy.Count);

            Assert.AreEqual(0, Wat1.Occupy[0].GetContaminants().Count);
            Assert.AreEqual(0, Wat1.Occupy[1].GetContaminants().Count);

            Task mix1 = new(() => Mission_Tasks.MixDroplets(Wat1, "square"));
            Wat1.GiveWork(mix1);
            mix1.Wait();
            Assert.AreEqual(2, Wat1.Occupy.Count);

            Assert.AreEqual(1, Wat1.Occupy[0].GetContaminants().Count);
            Assert.AreEqual(1, Wat1.Occupy[1].GetContaminants().Count);
            Assert.AreEqual(Wat1.Substance_Name, Wat1.Occupy[0].GetContaminants()[0]);
            Assert.AreEqual(Wat1.Substance_Name, Wat1.Occupy[1].GetContaminants()[0]);

        }



        [TestMethod()]
        public void MergeDropletsTest()
        {
            // Create board and both droplets
            board = Program.C.SetBoard(specialBoardDataLocation + "//MultipleInputs.json");
            Droplet Wat1 = new("Water", "Wat1");
            Droplet Wat2 = new("Water", "Wat2");
            Droplet Wat3 = new("Water", "Wat3");
            Droplet Wat4 = new("Water", "Wat4");
            board.Droplets.Add("Wat1", Wat1);
            board.Droplets.Add("Wat2", Wat2);
            board.Droplets.Add("Wat3", Wat3);
            board.Droplets.Add("Wat4", Wat4);
            Wat1.Thread.Start();
            Wat2.Thread.Start();
            Wat3.Thread.Start();
            Wat4.Thread.Start();

            Task input1 = new(() => Mission_Tasks.InputDroplet(Wat1, board.Input["in0"], 11));
            Wat1.GiveWork(input1);
            Task input2 = new(() => Mission_Tasks.InputDroplet(Wat2, board.Input["in1"], 11));
            Wat2.GiveWork(input2);
            Task input3 = new(() => Mission_Tasks.InputDroplet(Wat3, board.Input["in2"], 11));
            Wat3.GiveWork(input3);

            input1.Wait();
            input2.Wait();
            input3.Wait();

            Assert.AreEqual(Wat1, board.Input["in0"].pointers[0].Occupant);
            Assert.AreEqual(Wat2, board.Input["in1"].pointers[0].Occupant);
            Assert.AreEqual(Wat3, board.Input["in2"].pointers[0].Occupant);

            Assert.AreEqual(0, Wat4.Occupy.Count);

            List<string> InputDroplets = [Wat1.Name, Wat2.Name, Wat3.Name];
            List<string> OutputDroplets = [Wat4.Name];
            Apparature CommandDestination = board.Actuators["heat0"];

            UsefullSemaphore sem1 = new(InputDroplets.Count);
            Task<Electrode> calcMerge = new(() => Droplet_Actions.MergeCalc(InputDroplets, board.Droplets[OutputDroplets[0]], sem1));

            UsefullSemaphore sem2 = new(InputDroplets.Count);
            foreach (var item in InputDroplets)
            {
                Task awaitWork = new(() => Mission_Tasks.AwaitWork(board.Droplets[item], calcMerge, sem1, sem2, InputDroplets));
                board.Droplets[item].GiveWork(awaitWork);
            }
            Task mergeDroplet = new(() => Mission_Tasks.MergeDroplets(InputDroplets, board.Droplets[OutputDroplets[0]], calcMerge, sem2, CommandDestination));


            board.Droplets[OutputDroplets[0]].GiveWork(mergeDroplet);
            mergeDroplet.Wait();
            Assert.AreEqual(3, Wat4.Occupy.Count);

            Task outputDroplet = new(() => Mission_Tasks.OutputDroplet(Wat4, board.Output["out0"]));
            board.Droplets[OutputDroplets[0]].GiveWork(outputDroplet);
            outputDroplet.Wait();
            Assert.AreEqual(0, Wat4.Occupy.Count);




        }

        [TestMethod()]
        public void TempDropletTest()
        {


            board = Program.C.SetBoard(testBoardDataBigLocation);
            Droplet Wat1 = new("Water", "Wat1");
            board.Droplets.Add("Wat1", Wat1);
            Wat1.Thread.Start();

            Task input1 = new(() => Mission_Tasks.InputDroplet(Wat1, board.Input["in0"], 12));
            Wat1.GiveWork(input1);
            input1.Wait();
            Assert.AreEqual(2, Wat1.Occupy.Count);

            Assert.AreEqual(0, Wat1.Occupy[0].GetContaminants().Count);
            Assert.AreEqual(0, Wat1.Occupy[1].GetContaminants().Count);

            int time = 1;
            Task temp1 = new(() => Mission_Tasks.TempDroplet(Wat1, (Heater)board.Actuators["heat1"], time, Wat1.Substance_Name));
            Wat1.GiveWork(temp1);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            temp1.Wait();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Assert.IsTrue(elapsedMs >= time * 1000);
        }

        [TestMethod()]
        public void SenseDropletTest()
        {
            board = Program.C.SetBoard(specialBoardDataLocation + "//WithSensors.json");
            Droplet Wat1 = new("Water", "Wat1");
            board.Droplets.Add("Wat1", Wat1);
            Wat1.Thread.Start();

            Task input1 = new(() => Mission_Tasks.InputDroplet(Wat1, board.Input["in0"], 12));
            Wat1.GiveWork(input1);
            input1.Wait();

            Assert.AreEqual(null, board.Sensors["sens0"].value);

            Task sense1 = new(() => Mission_Tasks.SenseDroplet(Wat1, board.Sensors["sens0"]));
            Wat1.GiveWork(sense1);
            sense1.Wait();

            Assert.AreNotEqual(null, board.Sensors["sens0"].value);
            Assert.AreEqual("0000FF", (string)board.Sensors["sens0"].value[0]);

            Wat1.Color = "FFFF00";

            Task sense2 = new(() => Mission_Tasks.SenseDroplet(Wat1, board.Sensors["sens0"]));
            Wat1.GiveWork(sense2);
            sense2.Wait();

            Assert.AreNotEqual(null, board.Sensors["sens0"].value);
            Assert.AreEqual("FFFF00", (string)board.Sensors["sens0"].value[0]);

        }
        /*
        [TestMethod()]
        public void SplitDropletTest()
        {
            Assert.Fail();


            // Create board and both droplets
            board = Program.C.SetBoard(specialBoardDataLocation + "//TestBoardDataBigWithMoreHeat.json");
            Droplet Wat1 = new("Water", "Wat1");
            Droplet Wat2 = new("Water", "Wat2");
            Droplet Wat3 = new("Water", "Wat3");
            Droplet Wat4 = new("Water", "Wat4");
            board.Droplets.Add("Wat1", Wat1);
            board.Droplets.Add("Wat2", Wat2);
            board.Droplets.Add("Wat3", Wat3);
            board.Droplets.Add("Wat4", Wat4);
            Wat1.Thread.Start();
            Wat2.Thread.Start();
            Wat3.Thread.Start();
            Wat4.Thread.Start();

            Task input = new(() => Mission_Tasks.InputDroplet(Wat1, board.Input["in0"], 60));
            Wat1.GiveWork(input);
            Task move1 = new(() => )

            Dictionary<string, int> ratios = new Dictionary<string, int>();
            ratios.Add("Wat2", 50);
            ratios.Add("Wat3", 50);
            ratios.Add("Wat4", 50);
            Dictionary<string, UsefullSemaphore> dropSem = new Dictionary<string, UsefullSemaphore>();
            dropSem.Add("Wat2", new UsefullSemaphore(0, 1));
            dropSem.Add("Wat3", new UsefullSemaphore(0, 1));
            dropSem.Add("Wat4", new UsefullSemaphore(0, 1));

            Task split = new(() => Mission_Tasks.SplitDroplet(Wat1, [Wat2, Wat3, Wat4], ratios, dropSem)); //needs dest


        }*/
    }
}