using Microsoft.VisualStudio.TestTools.UnitTesting;
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
                Task awaitWork = new(() => Mission_Tasks.AwaitMergeWork(board.Droplets[item], calcMerge, sem1, sem2, InputDroplets));
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
            Task temp1 = new(() => Mission_Tasks.TempDroplet(Wat1, (Heater)board.Actuators["heat1"], time, "Hotwater"));
            Wat1.GiveWork(temp1);
            temp1.Wait();

            Assert.AreEqual("Hotwater", Wat1.Substance_Name);
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
        
        [TestMethod()]
        public void SplitDropletTest()
        {
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

            int time = 1;

            Task input = new(() => Mission_Tasks.InputDroplet(Wat1, board.Input["in0"], 60));
            Wat1.GiveWork(input);

            input.Wait();

            Apparature dest = Wat2.nextDestination;

            Dictionary<string, int> dropRat = new Dictionary<string, int>();
            dropRat.Add(Wat2.Name, 50);
            dropRat.Add(Wat3.Name, 50);
            dropRat.Add(Wat4.Name, 50);

            List<string> outDrops = [Wat2.Name, Wat3.Name, Wat4.Name];

            Dictionary<string, double> ratios = Calc.Ratio(dropRat, outDrops);
            Dictionary<string, UsefullSemaphore> dropSem = new Dictionary<string, UsefullSemaphore>();
            dropSem.Add("Wat2", new UsefullSemaphore(0, 1));
            dropSem.Add("Wat3", new UsefullSemaphore(0, 1));
            dropSem.Add("Wat4", new UsefullSemaphore(0, 1));

            Wat2.nextDestination = board.Actuators["heat1"];
            Wat3.nextDestination = board.Actuators["heat2"];
            Wat4.nextDestination = board.Output["out0"];

            Task split = new(() => Mission_Tasks.SplitDroplet(Wat1, ratios, dropSem, dest));
            Wat1.GiveWork(split);

            Task split2 = new(() => Mission_Tasks.AwaitSplitWork(Wat2, Wat2.nextDestination, dropSem[Wat2.Name]));
            Task split3 = new(() => Mission_Tasks.AwaitSplitWork(Wat3, Wat3.nextDestination, dropSem[Wat3.Name]));
            Task split4 = new(() => Mission_Tasks.AwaitSplitWork(Wat4, Wat4.nextDestination, dropSem[Wat4.Name]));
            Wat2.GiveWork(split2);
            Wat3.GiveWork(split3);
            Wat4.GiveWork(split4);

            split.Wait();

            Printer.PrintBoard();

            // Check that sizes are right
            Assert.IsTrue(Wat1.Removed);
            Assert.AreEqual(2, Wat2.Size);
            Assert.AreEqual(2, Wat3.Size);
            Assert.AreEqual(2, Wat4.Size);
            Assert.IsTrue(19.5 < Wat2.Volume && Wat2.Volume < 20.5);
            Assert.IsTrue(19.5 < Wat3.Volume && Wat3.Volume < 20.5);
            Assert.IsTrue(19.5 < Wat4.Volume && Wat4.Volume < 20.5);
        }
    }
}