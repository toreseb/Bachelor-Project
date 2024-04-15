using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Utility;

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
            Task Output1 = new(() => Mission_Tasks.OutputDroplet(Wat1, board.Output["out0"]));
            Wat1.GiveWork(Output1);
            Output1.Wait();
            Assert.AreEqual(0, Wat1.Occupy.Count);
            Assert.AreEqual(true, Wat1.Removed);

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

            Task input1 = new(() => Droplet_Actions.InputDroplet(Wat1, board.Input["in0"], 11));
            Wat1.GiveWork(input1);
            Task input2 = new(() => Droplet_Actions.InputDroplet(Wat2, board.Input["in1"], 11));
            Wat2.GiveWork(input2);
            Task input3 = new(() => Droplet_Actions.InputDroplet(Wat3, board.Input["in2"], 11));
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

        
    }
}