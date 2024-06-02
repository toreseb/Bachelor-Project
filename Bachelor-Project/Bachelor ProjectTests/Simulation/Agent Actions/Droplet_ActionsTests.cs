using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Utility;
using System.Security.Cryptography;
using Bachelor_Project.Electrode_Types.Actuator_Types;
using Bachelor_Project.Parsing;
using Bachelor_Project.Electrode_Types;


namespace Bachelor_Project.Simulation.Agent_Actions.Tests
{
    [TestClass()]
    public class Droplet_ActionsTests
    {
        static string inputfiles = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Input Files\\";
        static string testBoardData = "TestBoardData.json";
        static string testBoardDataBig = "TestBoardDataBig.json";
        static string testBoardDataBigWithMoreHeat = "TestBoardDataBigWithMoreHeat.json";
        static string testBoardDataBigWithSensors = "WithSensors.json";

        static string testBoardDataLocation = inputfiles + "\\" + testBoardData;
        static string testBoardDataBigLocation = inputfiles + "\\" + testBoardDataBig;
        static string testBoardDataBigWithMoreHeatLocation = inputfiles + "\\Special Boards\\" + testBoardDataBigWithMoreHeat;
        static string testBoardDataBigWithSensorsLocation = inputfiles + "\\Special Boards\\" + testBoardDataBigWithSensors;

        static Board board;

        [TestInitialize]
        public void Setup()
        {
            Commander C = new(null, testBoardDataLocation);
            Program.C = C;
            board = Program.C.board;
        }



        [TestMethod()]
        public void InputDropletTest_SizeOne()
        {
            // Input a droplet of size 1 electrode (0-11 ml)
            board = Program.C.SetBoard(testBoardDataLocation);

            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 11);

            Assert.AreEqual(1, board.Droplets.Count);
            Assert.AreEqual(1, board.Droplets["Wat1"].Occupy.Count);

            int amountOn = 0;
            foreach (Electrode e in board.Electrodes)
            {
                if (e.Occupant != null) { amountOn++; }
            }

            Assert.AreEqual(1, amountOn);

            Assert.AreEqual(1, board.Input["in0"].pointers[0].Status);

        }

        [TestMethod()]
        public void InputDropletTest_SizeTwo()
        {
            // Input a droplet of size larger than 1 electrode (2: 12-23 ml)
            board = Program.C.SetBoard(testBoardDataLocation);

            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 23);

            Assert.AreEqual(1, board.Droplets.Count);
            Assert.AreEqual(2, board.Droplets["Wat1"].Occupy.Count);

            int amountOn = 0;
            foreach (Electrode e in board.Electrodes)
            {
                if (e.Occupant != null) { amountOn++; }
            }

            Assert.AreEqual(2, amountOn);
        }

        [TestMethod()]
        public void InputDropletTest_LargeWithDest()
        {
            Droplet d = new Droplet("Water", "Wat1");
            board = Program.C.SetBoard(testBoardDataBigLocation);

            board.Droplets.Add("Wat1", d);
            Droplet_Actions.InputDroplet(d, board.Input["in0"], 60, board.Output["out0"]);
            board.PrintBoardState();

            Assert.AreEqual(d, board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(d, board.Electrodes[0, 5].Occupant);
            Assert.AreEqual(d, board.Electrodes[0, 6].Occupant);
            Assert.AreEqual(d, board.Electrodes[0, 7].Occupant);
            Assert.AreEqual(d, board.Electrodes[0, 8].Occupant);
            Assert.AreEqual(d, board.Electrodes[1, 8].Occupant);
        }

        [TestMethod()]
        public void InputDropletTest_LargeWithoutDest()
        {
            board = Program.C.SetBoard(testBoardDataBigLocation);
            Droplet test = new Droplet("Water", "Wat1");
            board.Droplets.Add("Wat1", test);
            Droplet_Actions.InputDroplet(test, board.Input["in0"], 60);

            Assert.AreEqual(1, board.Droplets.Count);
            Assert.AreEqual(6, test.Occupy.Count);

            Assert.AreEqual(test, board.Electrodes[0, 3].Occupant);
            Assert.AreEqual(test, board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(test, board.Electrodes[0, 5].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 3].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 5].Occupant);
        }

        [TestMethod()]
        public void InputDropletTest_OntoOtherDroplet()
        {

            // Tests await legal move

            // Create board and both droplets
            board = Program.C.SetBoard(testBoardDataLocation);
            Droplet Wat1 = new Droplet("Water", "Wat1");
            Droplet Wat2 = new Droplet("Water", "Wat2");
            board.Droplets.Add("Wat1", Wat1);
            board.Droplets.Add("Wat2", Wat2);
            Wat1.Thread.Start();
            Wat2.Thread.Start();

            // Input one droplet
            Task input = new(() => Droplet_Actions.InputDroplet(Wat1, board.Input["in0"], 11));
            Wat1.GiveWork(input);
            input.Wait();

            // Get input coords
            int inX = board.Input["in0"].pointers[0].EPosX;
            int inY = board.Input["in0"].pointers[0].EPosY;

            // Check that Wat1 is at in0
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[inX, inY].Occupant);

            // Try to input other droplet
            Task input2 = new(() => Droplet_Actions.InputDroplet(Wat2, board.Input["in0"], 11));
            Wat2.GiveWork(input2);


            // Check that Wat1 is at in0 and Wat2 is not
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[inX, inY].Occupant);
            Assert.AreNotEqual(board.Droplets["Wat2"], board.Electrodes[inX, inY].Occupant);

            // Move Wat1 out of the way and check that Wat2 is not input until the coast is clear
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[inX + 1, inY].Occupant);
            Assert.AreEqual(null, board.Electrodes[inX, inY].Occupant);
            Assert.AreNotEqual(board.Droplets["Wat2"], board.Electrodes[inX, inY].Occupant);

            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[inX + 2, inY].Occupant);
            input2.Wait();
            Assert.AreEqual(board.Droplets["Wat2"], board.Electrodes[inX, inY].Occupant);
        }

        [TestMethod()]
        public void MoveDropletTest_Small1x1DropletRight()
        {
            // Input a droplet of size 1
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 11);

            // Remember old position
            int oldPosX = board.Droplets["Wat1"].Occupy[0].EPosX;
            int oldPosY = board.Droplets["Wat1"].Occupy[0].EPosY;

            // Move droplet right
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);

            // Check if move was correct
            Assert.AreEqual(null, board.Electrodes[oldPosX, oldPosY].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[oldPosX + 1, oldPosY].Occupant);
        }

        [TestMethod()]
        public void MoveDropletTest_Small1x1DropletUp()
        {
            // Input a droplet of size 1
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 11);

            // Remember old position
            int oldPosX = board.Droplets["Wat1"].Occupy[0].EPosX;
            int oldPosY = board.Droplets["Wat1"].Occupy[0].EPosY;

            // Move droplet up
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.UP);

            // Check if move was correct
            Assert.AreEqual(null, board.Electrodes[oldPosX, oldPosY].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[oldPosX, oldPosY - 1].Occupant);
        }

        [TestMethod()]
        public void MoveDropletTest_Small1x1DropletLeft()
        {
            // Input a droplet of size 1
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 11);

            // Move right first so there is space for the droplet to move left (right is already tested)
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);

            // Remember old position
            int oldPosX = board.Droplets["Wat1"].Occupy[0].EPosX;
            int oldPosY = board.Droplets["Wat1"].Occupy[0].EPosY;

            // Move droplet left
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.LEFT);

            // Check if move was correct
            Assert.AreEqual(null, board.Electrodes[oldPosX, oldPosY].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[oldPosX - 1, oldPosY].Occupant);
        }

        [TestMethod()]
        public void MoveDropletTest_Small1x1DropletDown()
        {
            // Input a droplet of size 1
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 11);

            // Remember old position
            int oldPosX = board.Droplets["Wat1"].Occupy[0].EPosX;
            int oldPosY = board.Droplets["Wat1"].Occupy[0].EPosY;

            // Move droplet down
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.DOWN);

            // Check if move was correct
            Assert.AreEqual(null, board.Electrodes[oldPosX, oldPosY].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[oldPosX, oldPosY + 1].Occupant);
        }

        [TestMethod()]
        public void MoveDropletTest_Large2x3Droplet()
        {
            board = Program.C.SetBoard(testBoardDataBigLocation);
            Droplet test = new Droplet("Water", "Wat1");
            board.Droplets.Add("Wat1", test);
            Droplet_Actions.InputDroplet(test, board.Input["in0"], 60);

            Assert.AreEqual(1, board.Droplets.Count);
            Assert.AreEqual(6, test.Occupy.Count);

            Assert.AreEqual(test, board.Electrodes[0, 3].Occupant);
            Assert.AreEqual(test, board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(test, board.Electrodes[0, 5].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 3].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 5].Occupant);

            Droplet_Actions.MoveDroplet(test, Direction.RIGHT);

            Assert.AreEqual(test, board.Electrodes[1, 3].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 5].Occupant);
            Assert.AreEqual(test, board.Electrodes[2, 3].Occupant);
            Assert.AreEqual(test, board.Electrodes[2, 4].Occupant);
            Assert.AreEqual(test, board.Electrodes[2, 5].Occupant);
        }

        [TestMethod()]
        public void MoveDropletTest_DropletOverBorder()
        {
            // Input a droplet, in this case 1x1
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 11);

            // Remember old position
            int oldPosX = board.Droplets["Wat1"].Occupy[0].EPosX;
            int oldPosY = board.Droplets["Wat1"].Occupy[0].EPosY;

            // Droplet is inserted by left boarder. Try to move droplet left.
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.LEFT);

            // Check that droplet did not move.
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[oldPosX, oldPosY].Occupant);
        }

        [TestMethod()]
        public void MoveDropletTest_MoveCloseToDroplet()
        {
            // Create board and add droplets
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            board.Droplets.Add("Wat2", new Droplet("Water", "Wat2"));

            // Input and move droplet that will be moved close to
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 11);
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);

            // Input other droplet
            Droplet_Actions.InputDroplet(board.Droplets["Wat2"], board.Input["in0"], 11);

            // Save Wat2 placement
            int oldX = board.Droplets["Wat2"].Occupy[0].EPosX;
            int oldY = board.Droplets["Wat2"].Occupy[0].EPosY;

            // Move Wat2 towards Wat1, first move should be allowed, second move should be stopped
            Droplet_Actions.MoveDroplet(board.Droplets["Wat2"], Direction.RIGHT);
            Assert.AreEqual(null, board.Electrodes[oldX, oldY].Occupant);
            Assert.AreEqual(board.Droplets["Wat2"], board.Electrodes[oldX + 1, oldY].Occupant);
            Droplet_Actions.MoveDroplet(board.Droplets["Wat2"], Direction.RIGHT);
            Assert.AreEqual(board.Droplets["Wat2"], board.Electrodes[oldX + 1, oldY].Occupant);
            Assert.AreEqual(null, board.Electrodes[oldX + 2, oldY].Occupant);
        }

        [TestMethod()]
        public void MoveDropletTest_MoveIntoContam()
        {
            // Create board and add droplets
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            board.Droplets.Add("Blood1", new Droplet("Blood", "Blood1"));
            // Water is contaminated by blood
            board.Droplets["Wat1"].Contamintants = ["Blood"];
            board.Droplets["Blood1"].Contamintants = [];

            // Insert and move water to be ready to cross blood
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 11);
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.UP);
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.RIGHT);

            // Insert and move blood to make a trail to cross
            Droplet_Actions.InputDroplet(board.Droplets["Blood1"], board.Input["in0"], 11);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.DOWN);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);

            // Move water to right before crossing blood
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.LEFT);
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.DOWN);

            // Save water position
            int watX = board.Droplets["Wat1"].Occupy[0].EPosX;
            int watY = board.Droplets["Wat1"].Occupy[0].EPosY;

            // Check contamination
            Assert.IsTrue(board.Electrodes[watX, watY + 1].GetContaminants().Any()); // True if there is any contaminants

            // Check current position and contaminated position
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[watX, watY].Occupant);
            Assert.AreEqual(null, board.Electrodes[watX, watY + 1].Occupant);

            // Attempt move into contamination
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.DOWN);

            // Check where droplet is
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[watX, watY].Occupant);
            Assert.AreEqual(null, board.Electrodes[watX, watY + 1].Occupant);
        }

        [TestMethod()]
        public void MoveDropletTest_InputIntoContam()
        {
            // Create board and add droplets
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            board.Droplets.Add("Blood1", new Droplet("Blood", "Blood1"));
            // Water is contaminated by blood
            board.Droplets["Wat1"].Contamintants = ["Blood"];
            board.Droplets["Blood1"].Contamintants = [];

            // Input and move blood
            Droplet_Actions.InputDroplet(board.Droplets["Blood1"], board.Input["in0"], 11);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);

            // Input water


            //Assert.ThrowsException<Exception>(new Action(() => Mission_Tasks.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 11)));
        }

        [TestMethod()]
        public void MoveDropletTest_InputAgain()
        {
            board = Program.C.SetBoard(testBoardDataLocation);
            Droplet drop1 = new("Water", "Wat1");
            board.Droplets.Add("Wat1", drop1);


            // Input and move blood
            Droplet_Actions.InputDroplet(drop1, board.Input["in0"], 11);
            Droplet_Actions.MoveDroplet(drop1, Direction.RIGHT);
            Droplet_Actions.MoveDroplet(drop1, Direction.RIGHT);
            Droplet_Actions.MoveDroplet(drop1, Direction.RIGHT);
            Droplet_Actions.MoveDroplet(drop1, Direction.RIGHT);

            // Input water


            Assert.ThrowsException<Exception>(new Action(() => Mission_Tasks.InputDroplet(drop1, board.Input["in0"], 11)));
        }

        [TestMethod()]
        public void MixTest()
        {
            board = Program.C.SetBoard(testBoardDataLocation);
            Droplet test = new Droplet("Water", "Wat1");
            board.Droplets.Add("Wat1", test);
            Droplet_Actions.InputDroplet(test, board.Input["in0"], 12);

            Assert.AreEqual(1, board.Droplets.Count);
            Assert.AreEqual(2, test.Occupy.Count);

            int amountOn = 0;
            foreach (Electrode e in board.Electrodes)
            {
                if (e.Occupant != null) { amountOn++; }
            }

            Assert.AreEqual(2, amountOn);

            Assert.IsTrue(board.Electrodes[0, 0].Occupant == test);
            Assert.IsTrue(board.Electrodes[0, 1].Occupant == test);

            Assert.IsTrue(board.Electrodes[0, 0].GetContaminants().Count == 0);
            Assert.IsTrue(board.Electrodes[0, 1].GetContaminants().Count == 0);
            Assert.IsTrue(board.Electrodes[0, 2].GetContaminants().Count == 0);
            Assert.IsTrue(board.Electrodes[1, 0].GetContaminants().Count == 0);
            Assert.IsTrue(board.Electrodes[1, 1].GetContaminants().Count == 0);
            Assert.IsTrue(board.Electrodes[1, 2].GetContaminants().Count == 0);

            Mission_Tasks.MixDroplet(test, "square");
            Assert.IsTrue(board.Electrodes[0, 0].GetContaminants().Count == 1 && board.Electrodes[0, 0].GetContaminants()[0] == "Water");
            Assert.IsTrue(board.Electrodes[0, 1].GetContaminants().Count == 1 && board.Electrodes[0, 1].GetContaminants()[0] == "Water");
            Assert.IsTrue(board.Electrodes[0, 2].GetContaminants().Count == 1 && board.Electrodes[0, 2].GetContaminants()[0] == "Water");
            Assert.IsTrue(board.Electrodes[1, 0].GetContaminants().Count == 1 && board.Electrodes[1, 0].GetContaminants()[0] == "Water");
            Assert.IsTrue(board.Electrodes[1, 1].GetContaminants().Count == 1 && board.Electrodes[1, 1].GetContaminants()[0] == "Water");
            Assert.IsTrue(board.Electrodes[1, 2].GetContaminants().Count == 1 && board.Electrodes[1, 2].GetContaminants()[0] == "Water");


        }

        [TestMethod()]
        public void SnekMoveTest_Down()
        {
            // Done with snake of size 3
            // Because of the functionality of the program, we need to force it into a snake position.


            // Make droplet
            Droplet d = new Droplet("Water", "Wat1");

            // Make board
            board = Program.C.SetBoard(testBoardDataBigLocation);
            board.Droplets.Add("Wat1", d);

            // Force droplet into snake
            d.SnekMode = true;
            d.SetSizes(24);

            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[2, 4]);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[1, 4], false);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[0, 4], false);

            // Check if expected position
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 4].Occupant);

            // Move down
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.DOWN);

            // Check if expected position
            Assert.AreEqual(null, board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 5].Occupant);
        }

        [TestMethod()]
        public void SnekMoveTest_Right()
        {
            // Make droplet
            Droplet d = new Droplet("Water", "Wat1");

            // Make board
            board = Program.C.SetBoard(testBoardDataBigLocation);
            board.Droplets.Add("Wat1", d);

            // Force droplet into snake
            d.SnekMode = true;
            d.SetSizes(24);

            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[2, 4]);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[1, 4], false);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[0, 4], false);

            // Check if expected position
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 4].Occupant);

            // Move down
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);

            board.PrintBoardState();

            // Check if expected position
            Assert.AreEqual(null, board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 4].Occupant);
        }

        [TestMethod()]
        public void SnekMoveTest_Up()
        {
            // Done with snake of size 3
            // Because of the functionality of the program, we need to force it into a snake position.

            // Make droplet
            Droplet d = new Droplet("Water", "Wat1");

            // Make board
            board = Program.C.SetBoard(testBoardDataBigLocation);
            board.Droplets.Add("Wat1", d);

            // Force droplet into snake
            d.SnekMode = true;
            d.SetSizes(24);

            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[2, 4]);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[1, 4], false);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[0, 4], false);

            // Check if expected position
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 4].Occupant);

            // Move up
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.UP);

            // Check if expected position
            Assert.AreEqual(null, board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 3].Occupant);
        }

        [TestMethod()]
        public void SnekMoveTest_Left()
        {
            // Done with snake of size 3
            // Because of the functionality of the program, we need to force it into a snake position.

            // Make droplet
            Droplet d = new Droplet("Water", "Wat1");

            // Make board
            board = Program.C.SetBoard(testBoardDataBigLocation);
            board.Droplets.Add("Wat1", d);

            // Force droplet into snake
            d.SnekMode = true;
            d.SetSizes(24);

            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[2, 4]);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[1, 4], false);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[0, 4], false);

            // Move up, then left
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.UP);
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.LEFT);

            // Check if expected position
            Assert.AreEqual(null, board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(null, board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 3].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 3].Occupant);
        }

        [TestMethod()]
        public void SnekMoveTest_IntoSelf()
        {
            // Done with snake of size 5.
            // Because of the functionality of the program, we need to force it into a snake position.

            // Make droplet
            Droplet d = new Droplet("Water", "Wat1");

            // Make board
            board = Program.C.SetBoard(testBoardDataBigLocation);
            board.Droplets.Add("Wat1", d);

            // Force droplet into snake
            d.SnekMode = true;
            d.SetSizes(50);

            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[4, 4]);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[3, 4], false);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[2, 4], false);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[1, 4], false);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[0, 4], false);

            // Check placement
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[4, 4].Occupant);

            // Move snek into position to move into self
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.UP);
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.LEFT);

            board.PrintBoardState();

            // Check placement
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 3].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[4, 3].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[4, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 4].Occupant);

            // Try to move into self
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.DOWN);

            board.PrintBoardState();

            // Check that placement did not change
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 3].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[4, 3].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[4, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 4].Occupant);
        }

        [TestMethod()]
        public void SnekMoveTest_OutOfBoard()
        {
            // Done with snake of size 3
            // Because of the functionality of the program, we need to force it into a snake position.

            // Make droplet
            Droplet d = new Droplet("Water", "Wat1");

            // Make board
            board = Program.C.SetBoard(testBoardDataBigLocation);
            board.Droplets.Add("Wat1", d);

            // Force droplet into snake
            d.SnekMode = true;
            d.SetSizes(24);

            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[7, 2]);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[6, 2], false);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[5, 2], false);

            // Move right
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);
            // Move right off board
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);

            // Check if expected position
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[8, 2].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[7, 2].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[6, 2].Occupant);
        }

        [TestMethod()]
        public void SnekMoveTest_BackIntoBody()
        {
            // Done with snake of size 3
            // Because of the functionality of the program, we need to force it into a snake position.

            // Make droplet
            Droplet d = new Droplet("Water", "Wat1");

            // Make board
            board = Program.C.SetBoard(testBoardDataBigLocation);
            board.Droplets.Add("Wat1", d);

            // Force droplet into snake
            d.SnekMode = true;
            d.SetSizes(24);

            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[2, 1]);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[1, 1], false);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[0, 1], false);

            // Move left
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.LEFT);

            // Check position
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 1].Occupant);
        }

        [TestMethod()]
        public void SnekMoveTest_IntoOtherDroplet()
        {
            // Make droplets
            Droplet d1 = new Droplet("Water", "Wat1");
            Droplet d2 = new Droplet("Water", "Wat2");

            // Make board
            board = Program.C.SetBoard(testBoardDataBigLocation);
            board.Droplets.Add("Wat1", d1);
            board.Droplets.Add("Wat2", d2);

            // Force droplet into snake
            d1.SnekMode = true;
            d2.SnekMode = true;
            d1.SetSizes(24);
            d2.SetSizes(11);

            // Place droplets
            Droplet_Actions.MoveOnElectrode(d1, board.Electrodes[2, 4]);
            Droplet_Actions.MoveOnElectrode(d1, board.Electrodes[1, 4], false);
            Droplet_Actions.MoveOnElectrode(d1, board.Electrodes[0, 4], false);

            Droplet_Actions.MoveOnElectrode(d2, board.Electrodes[4, 4]);

            // Check position of Wat1
            Assert.AreEqual(d1, board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(d1, board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(d1, board.Electrodes[2, 4].Occupant);

            // Move Wat1 into Wat2's boarder
            Droplet_Actions.SnekMove(d1, Direction.RIGHT);

            // Check that Wat1 did not move.
            Assert.AreEqual(d1, board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(d1, board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(d1, board.Electrodes[2, 4].Occupant);
        }

        [TestMethod()]
        public void SnekMoveTest_IntoContam()
        {
            // Done with a water snake of size 2 and blood of size 1
            // Because of the functionality of the program, we need to force it into a snake position.

            // Make droplets
            Droplet w = new Droplet("Water", "Wat1");
            Droplet b = new Droplet("Blood", "Blood1");

            // Make board
            board = Program.C.SetBoard(testBoardDataBigLocation);
            board.Droplets.Add("Wat1", w);
            board.Droplets.Add("Blood1", b);

            // Force droplet into snake
            w.SnekMode = true;
            w.SetSizes(12);

            Droplet_Actions.MoveOnElectrode(w, board.Electrodes[3, 0]);
            Droplet_Actions.MoveOnElectrode(w, board.Electrodes[2, 0], false);

            // Water is contaminated by blood
            board.Droplets["Wat1"].Contamintants = ["Blood"];
            board.Droplets["Blood1"].Contamintants = [];

            // Input and move blood
            Droplet_Actions.InputDroplet(board.Droplets["Blood1"], board.Input["in0"], 11);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);

            board.PrintBoardState();

            // Move water close to contam
            Droplet_Actions.SnekMove(w, Direction.DOWN);
            Droplet_Actions.SnekMove(w, Direction.DOWN);
            Droplet_Actions.SnekMove(w, Direction.DOWN);

            // Check positions and contam
            Assert.IsTrue(board.Electrodes[3, 4].GetContaminants().Any());
            Assert.AreEqual(null, board.Electrodes[3, 4].Occupant);
            Assert.AreEqual(board.Droplets["Blood1"], board.Electrodes[6, 4].Occupant); // Blood is far enough away to not interfere
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 2].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 3].Occupant);

            // Try to move into contam
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.DOWN);

            // Check that snake did not move
            Assert.AreEqual(null, board.Electrodes[3, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 2].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 3].Occupant);
        }

        [TestMethod()]
        public void SnekReversalTest()
        {
            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataBigLocation);
            Droplet drop1 = new Droplet("Water", "Wat1");
            board.Droplets.Add("Wat1", drop1);
            Droplet_Actions.InputDroplet(drop1, board.Input["in0"], 24);
            drop1.Important = true; // To make sure it doesn't coil
            Droplet_Actions.UncoilSnek(drop1, board.Electrodes[3, 4]);

            Electrode oldHead = drop1.SnekList.First.Value;
            Electrode newHead = drop1.SnekList.Last.Value;

            Assert.AreEqual(oldHead, drop1.SnekList.First.Value);

            Droplet_Actions.SnekReversal(drop1);

            Assert.AreEqual(newHead, drop1.SnekList.First.Value);
        }

        [TestMethod()]
        public void CoilSnekTest()
        {
            // Test with snake size 6
            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataBigLocation);
            Droplet drop1 = new Droplet("Water", "Wat1");
            board.Droplets.Add("Wat1", drop1);
            Droplet_Actions.InputDroplet(drop1, board.Input["in0"], 60);

            // Check placement
            Assert.AreEqual(drop1, board.Electrodes[0, 3].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 5].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[1, 3].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[1, 5].Occupant);

            // UncoilSnek() coils the snake at the destination, thus testing CoilSnek()
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[6, 4]);

            // Check placement
            Assert.AreEqual(drop1, board.Electrodes[5, 3].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[5, 4].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[5, 5].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[6, 3].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[6, 4].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[6, 5].Occupant);

            // More in depth tests will follow when algorithm is implemented
        }


        [TestMethod()]
        public void CoilSnekTest_InCorner()
        {
            // Test with snake size 4
            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataBigLocation);
            Droplet drop1 = new Droplet("Water", "Wat1");
            board.Droplets.Add("Wat1", drop1);
            Droplet_Actions.InputDroplet(drop1, board.Input["in0"], 36);

            // Check placement
            Assert.AreEqual(drop1, board.Electrodes[0, 3].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 5].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[1, 4].Occupant);

            // Coil
            Droplet_Actions.UncoilSnek(drop1, board.Electrodes[0, 8]); // Uncoil coils at the destination

            // Check placement
            Assert.AreEqual(drop1, board.Electrodes[0, 8].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 7].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[1, 8].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[1, 7].Occupant);
        }

        [TestMethod()]
        public void CoilSnekTest_CloseToOtherDroplet()
        {
            board = Program.C.SetBoard(testBoardDataBigLocation);
            Droplet drop1 = new Droplet("Water", "Wat1");
            Droplet drop2 = new Droplet("Water", "Wat2");
            board.Droplets.Add("Wat1", drop1);
            board.Droplets.Add("Wat2", drop2);
            Droplet_Actions.InputDroplet(drop1, board.Input["in0"], 11); // Size = 1

            Droplet_Actions.UncoilSnek(drop1, board.Electrodes[2, 4]);
            Assert.AreEqual(drop1, board.Electrodes[2, 4].Occupant);

            Droplet_Actions.InputDroplet(drop2, board.Input["in0"], 120); // Size = 10

            Assert.AreEqual(drop2, board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(drop2, board.Electrodes[0, 2].Occupant);
            Assert.AreEqual(drop2, board.Electrodes[0, 3].Occupant);
            Assert.AreEqual(drop2, board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(drop2, board.Electrodes[0, 5].Occupant);
            Assert.AreEqual(drop2, board.Electrodes[0, 6].Occupant);
            Assert.AreEqual(drop2, board.Electrodes[0, 7].Occupant);
            Assert.AreEqual(null, board.Electrodes[0, 8].Occupant);

            Assert.AreEqual(null, board.Electrodes[1, 0].Occupant);
            Assert.AreEqual(null, board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(drop2, board.Electrodes[1, 2].Occupant);
            Assert.AreEqual(null, board.Electrodes[1, 3].Occupant);
            Assert.AreEqual(null, board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(null, board.Electrodes[1, 5].Occupant);
            Assert.AreEqual(drop2, board.Electrodes[1, 6].Occupant);
            Assert.AreEqual(drop2, board.Electrodes[1, 7].Occupant);
            Assert.AreEqual(null, board.Electrodes[1, 8].Occupant);

            Assert.AreEqual(null, board.Electrodes[2, 0].Occupant);
            Assert.AreEqual(null, board.Electrodes[2, 1].Occupant);
            Assert.AreEqual(drop2, board.Electrodes[2, 2].Occupant);
            Assert.AreEqual(null, board.Electrodes[2, 3].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[2, 4].Occupant);
            Assert.AreEqual(null, board.Electrodes[2, 5].Occupant);
            Assert.AreEqual(null, board.Electrodes[2, 6].Occupant);
            Assert.AreEqual(null, board.Electrodes[2, 7].Occupant);
            Assert.AreEqual(null, board.Electrodes[2, 8].Occupant);

        }

        [TestMethod()]
        public void CoilSnekTest_TooCloseToApperature()
        {
            board = Program.C.SetBoard(testBoardDataBigLocation);
            Droplet drop1 = new Droplet("Water", "Wat1");
            board.Droplets.Add("Wat1", drop1);
            Droplet_Actions.InputDroplet(drop1, board.Input["in0"], 36);

            // Check placement
            Assert.AreEqual(drop1, board.Electrodes[0, 3].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 5].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[1, 4].Occupant);

            // Coil
            Droplet_Actions.UncoilSnek(drop1, board.Electrodes[0, 0]);

            Assert.AreEqual(drop1, board.Electrodes[0, 0].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 2].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[1, 2].Occupant);

        }
        [TestMethod()]
        public void CoilSnekTest_CloseToApperature()
        {
            board = Program.C.SetBoard(testBoardDataBigLocation);
            Droplet drop1 = new Droplet("Water", "Wat1");
            board.Droplets.Add("Wat1", drop1);
            Droplet_Actions.InputDroplet(drop1, board.Input["in0"], 36);

            // Check placement
            Assert.AreEqual(drop1, board.Electrodes[0, 3].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 5].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[1, 4].Occupant);

            // Coil
            Droplet_Actions.UncoilSnek(drop1, board.Electrodes[0, 2]); // Uncoil coils at the destination

            // Check placement
            Assert.AreEqual(drop1, board.Electrodes[0, 6].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 7].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[1, 6].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[1, 7].Occupant);
        }

        [TestMethod()]
        public void UncoilSnekTest()
        {
            /* This test is moot as UncoilSnek has been changed to uncoiling, moving, and then recoiling.
            // Test with snake size 6
            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataBigLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 60);
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[6, 4]);

            // Check placement
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[4, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[5, 4].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[6, 4].Occupant);

            // More complicated tests follow when algorithm is implemented
            */

            // Test with snake size 6
            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataBigLocation);
            Droplet drop1 = new Droplet("Water", "Wat1");
            board.Droplets.Add("Wat1", drop1);
            Droplet_Actions.InputDroplet(drop1, board.Input["in0"], 60);
            Droplet_Actions.UncoilSnek(drop1, board.Electrodes[7, 2]);

            // Check placement
            Assert.AreEqual(drop1, board.Electrodes[7, 1].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[7, 2].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[6, 2].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[6, 3].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[8, 2].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[8, 1].Occupant);
        }

        [TestMethod()]
        public void UncoilSnek_OutIntoSelf()
        {
            board = Program.C.SetBoard(testBoardDataBigLocation);
            Droplet d = new("Water", "Wat1");
            board.Droplets.Add("Wat1", d);
            Droplet_Actions.InputDroplet(d, board.Input["in0"], 36);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[2, 4]);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[2, 5]);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[2, 6]);
            Droplet_Actions.MoveOnElectrode(d, board.Electrodes[2, 7]);
            d.Size = 8;

            Assert.AreEqual(8, d.Occupy.Count);

            Droplet_Actions.UncoilSnek(d, board.Electrodes[8, 5]);

            Assert.AreEqual(8, d.Occupy.Count);

            Assert.AreEqual(d, board.Electrodes[8, 8].Occupant);
            Assert.AreEqual(d, board.Electrodes[8, 7].Occupant);
            Assert.AreEqual(d, board.Electrodes[8, 6].Occupant);
            Assert.AreEqual(d, board.Electrodes[7, 6].Occupant);
            Assert.AreEqual(d, board.Electrodes[7, 7].Occupant);
            Assert.AreEqual(d, board.Electrodes[7, 8].Occupant);
            Assert.AreEqual(d, board.Electrodes[6, 6].Occupant);
            Assert.AreEqual(d, board.Electrodes[6, 7].Occupant);



        }

        [TestMethod()]
        public void OutputTest_Size1()
        {
            board = Program.C.SetBoard(testBoardDataLocation);
            Droplet d = new("Water", "Wat1");
            board.Droplets.Add("Wat1", d);
            Droplet_Actions.InputDroplet(d, board.Input["in0"], 11);
            Assert.AreNotEqual(0, d.Occupy.Count);
            Mission_Tasks.OutputDroplet(d, board.Output["out0"]);
            Assert.AreEqual(0, d.Occupy.Count);
        }

        [TestMethod()]
        public void OutputTest_Size3()
        {
            board = Program.C.SetBoard(testBoardDataLocation);
            Droplet d = new("Water", "Wat1");
            board.Droplets.Add("Wat1", d);
            Droplet_Actions.InputDroplet(d, board.Input["in0"], 35);
            Assert.AreNotEqual(0, d.Occupy.Count);
            Mission_Tasks.OutputDroplet(d, board.Output["out0"]);
            Assert.AreEqual(0, d.Occupy.Count);
        }


        [TestMethod()]
        public void UncoilSnekTest_CloseToOtherDroplet()
        {
            board = Program.C.SetBoard(testBoardDataBigLocation);
            Droplet drop1 = new Droplet("Water", "Wat1");
            Droplet drop2 = new Droplet("Water", "Wat2");
            board.Droplets.Add("Wat1", drop1);
            board.Droplets.Add("Wat2", drop2);
            Droplet_Actions.InputDroplet(drop1, board.Input["in0"], 11); // Size = 1

            Droplet_Actions.UncoilSnek(drop1, board.Electrodes[2, 4]);
            Assert.AreEqual(drop1, board.Electrodes[2, 4].Occupant);

            Droplet_Actions.InputDroplet(drop2, board.Input["in0"], 24); // Size = 3
            Droplet_Actions.UncoilSnek(drop2, board.Electrodes[4, 4]);

            Assert.AreEqual(drop2, board.Electrodes[4, 4].Occupant);
            Assert.AreEqual(drop2, board.Electrodes[4, 3].Occupant);
            Assert.AreEqual(drop2, board.Electrodes[5, 4].Occupant);
        }

        [TestMethod()]
        public void UncoilSnekTest_InSmallSpace()
        {
            board = Program.C.SetBoard(testBoardDataBigLocation);
            Droplet drop1 = new Droplet("Water", "Wat1");
            drop1.Contamintants.Add("Blood");
            board.Droplets.Add("Wat1", drop1);
            Droplet_Actions.InputDroplet(drop1, board.Input["in0"], 36);

            // Check placement
            Assert.AreEqual(drop1, board.Electrodes[0, 3].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 4].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 5].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[1, 4].Occupant);

            board.Electrodes[1, 3].Contaminate("Blood");
            board.Electrodes[2, 4].Contaminate("Blood");
            board.Electrodes[1, 5].Contaminate("Blood");
            board.Electrodes[0, 6].Contaminate("Blood");

            Droplet_Actions.UncoilSnek(drop1, board.Electrodes[0, 7]);

            Assert.AreEqual(drop1, board.Electrodes[0, 7].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[0, 8].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[1, 6].Occupant);
            Assert.AreEqual(drop1, board.Electrodes[1, 7].Occupant);

        }

        /*
        [TestMethod()]
        public void MergeMoveTest()
        {
            // Make board, make droplets, place droplets ready for merge.
            Droplet w = new Droplet("Water", "Wat1");
            Droplet b = new Droplet("Blood", "Blood1");
            Droplet mix = new Droplet("BloodWater", "Bw");
            board = Program.C.SetBoard(testBoardDataBigLocation);
            board.Droplets.Add("Wat1", w);
            board.Droplets.Add("Blood1", b);
            board.Droplets.Add("Bw", mix);

            w.SetSizes(48);
            b.SetSizes(36);

            Droplet_Actions.MoveOnElectrode(w, board.Electrodes[2, 4]);
            Droplet_Actions.MoveOnElectrode(w, board.Electrodes[2, 5]);
            Droplet_Actions.MoveOnElectrode(w, board.Electrodes[2, 6]);
            Droplet_Actions.MoveOnElectrode(w, board.Electrodes[3, 4]);
            Droplet_Actions.MoveOnElectrode(w, board.Electrodes[3, 5]);

            Droplet_Actions.MoveOnElectrode(b, board.Electrodes[5, 5]);
            Droplet_Actions.MoveOnElectrode(b, board.Electrodes[5, 6]);
            Droplet_Actions.MoveOnElectrode(b, board.Electrodes[6, 6]);
            Droplet_Actions.MoveOnElectrode(b, board.Electrodes[6, 7]);

            Printer.PrintBoard();

            // It's time... TO MIX!!
            Droplet_Actions.MergeMove(mix, [w, b], board.Electrodes[4, 5]);
            Printer.PrintBoard();

            Assert.AreEqual(0, w.Occupy.Count);
            Assert.AreEqual(0, b.Occupy.Count);
            Assert.AreEqual(8, mix.Occupy.Count);

            Assert.AreEqual(mix, board.Electrodes[3, 5].Occupant);
            Assert.AreEqual(mix, board.Electrodes[3, 6].Occupant);
            Assert.AreEqual(mix, board.Electrodes[4, 4].Occupant);
            Assert.AreEqual(mix, board.Electrodes[4, 5].Occupant);
            Assert.AreEqual(mix, board.Electrodes[4, 6].Occupant);
            Assert.AreEqual(mix, board.Electrodes[5, 4].Occupant);
            Assert.AreEqual(mix, board.Electrodes[5, 5].Occupant);
            Assert.AreEqual(mix, board.Electrodes[5, 6].Occupant);

            Assert.AreEqual(null, board.Electrodes[2, 4].Occupant);
            Assert.AreEqual(null, board.Electrodes[2, 5].Occupant);
            Assert.AreEqual(null, board.Electrodes[2, 6].Occupant);
            Assert.AreEqual(null, board.Electrodes[3, 4].Occupant);

            Assert.AreEqual(null, board.Electrodes[6, 6].Occupant);
            Assert.AreEqual(null, board.Electrodes[6, 7].Occupant);
        }
        */

        [TestMethod()]
        public void splitDropletTest_TwoSplitters()
        {
            Droplet w1 = new Droplet("Water", "Wat1");
            Droplet w2 = new Droplet("Water", "Wat2");
            Droplet w3 = new Droplet("Water", "Wat3");

            board = Program.C.SetBoard(testBoardDataBigWithMoreHeatLocation);

            board.Droplets.Add("Wat1", w1);
            board.Droplets.Add("Wat2", w2);
            board.Droplets.Add("Wat3", w3);

            w2.nextDestination = board.Actuators["heat1"];
            w2.nextElectrodeDestination = board.Electrodes[7, 0];
            w3.nextDestination = board.Actuators["heat2"];
            w3.nextElectrodeDestination = board.Electrodes[7, 8];

            Droplet_Actions.InputDroplet(w1, board.Input["in0"], 48);

            Dictionary<string, double> ratios = new Dictionary<string, double>();
            ratios.Add(w2.Name, 30);
            ratios.Add(w3.Name, 70);

            Dictionary<string, UsefulSemaphore> sems = new Dictionary<string, UsefulSemaphore>();
            sems.Add(w2.Name, new UsefulSemaphore(1, 2)); // Start with 1 to artificially give it what AwaitWork would
            sems.Add(w3.Name, new UsefulSemaphore(1, 2)); // Start with 1 to artificially give it what AwaitWork would

            Droplet_Actions.SplitDroplet(w1, ratios, sems);

            // Check existance/sizes
            Assert.IsTrue(w1.Removed);
            Assert.AreEqual(2, w2.Occupy.Count);
            Assert.AreEqual(3, w3.Occupy.Count);

            // Check placements
            Assert.AreEqual(board.Electrodes[0,0].Occupant, w2);
            Assert.AreEqual(board.Electrodes[0,1].Occupant, w2);

            Assert.AreEqual(board.Electrodes[1,3].Occupant, w3);
            Assert.AreEqual(board.Electrodes[1,4].Occupant, w3);
            Assert.AreEqual(board.Electrodes[1,5].Occupant, w3);



            Printer.PrintBoard();
        }

        [TestMethod()]
        public void splitDropletTest_SeveralSplitters()
        {
            Droplet w1 = new Droplet("Water", "Wat1");
            Droplet w2 = new Droplet("Water", "Wat2");
            Droplet w3 = new Droplet("Water", "Wat3");
            Droplet w4 = new Droplet("Water", "Wat4");
            Droplet w5 = new Droplet("Water", "Wat5");

            board = Program.C.SetBoard(testBoardDataBigWithMoreHeatLocation);

            board.Droplets.Add("Wat1", w1);
            board.Droplets.Add("Wat2", w2);
            board.Droplets.Add("Wat3", w3);
            board.Droplets.Add("Wat4", w4);
            board.Droplets.Add("Wat5", w5);

            w2.nextElectrodeDestination = board.Electrodes[0, 0];
            w3.nextElectrodeDestination = board.Electrodes[0, 8];
            w4.nextElectrodeDestination = board.Electrodes[8, 0];
            w5.nextElectrodeDestination = board.Electrodes[8, 8];

            Droplet_Actions.InputDroplet(w1, board.Input["in0"], 84);

            Dictionary<string, int> ratios = [];
            ratios.Add(w2.Name, 50);
            ratios.Add(w3.Name, 50);
            ratios.Add(w4.Name, 50);
            ratios.Add(w5.Name, 50);

            List<string> OutputDroplets = [w2.Name, w3.Name, w4.Name, w5.Name];

            Dictionary<string, double> correctRatios = Calc.FindPercentages(ratios, OutputDroplets);

            Dictionary<string, UsefulSemaphore> sems = new Dictionary<string, UsefulSemaphore>();
            sems.Add(w2.Name, new UsefulSemaphore(1, 2)); // Start with 1 to artificially give it what AwaitWork would
            sems.Add(w3.Name, new UsefulSemaphore(1, 2)); // Start with 1 to artificially give it what AwaitWork would
            sems.Add(w4.Name, new UsefulSemaphore(1, 2)); // Start with 1 to artificially give it what AwaitWork would
            sems.Add(w5.Name, new UsefulSemaphore(1, 2)); // Start with 1 to artificially give it what AwaitWork would

            Droplet_Actions.SplitDroplet(w1, correctRatios, sems);

            Assert.AreEqual(true, w1.Removed);
            Assert.AreEqual(0, w1.Occupy.Count);
            Assert.AreEqual(2, w2.Occupy.Count);
            Assert.AreEqual(2, w3.Occupy.Count);
            Assert.AreEqual(2, w4.Occupy.Count);
            Assert.AreEqual(2, w5.Occupy.Count);

            // Check placements
            Assert.AreEqual(board.Electrodes[0, 0].Occupant, w2);
            Assert.AreEqual(board.Electrodes[1, 0].Occupant, w2);

            Assert.AreEqual(board.Electrodes[0, 6].Occupant, w3);
            Assert.AreEqual(board.Electrodes[1, 6].Occupant, w3);

            Assert.AreEqual(board.Electrodes[3, 1].Occupant, w4);
            Assert.AreEqual(board.Electrodes[3, 2].Occupant, w4);

            Assert.AreEqual(board.Electrodes[3, 4].Occupant, w5);
            Assert.AreEqual(board.Electrodes[4, 4].Occupant, w5);
        }

        [TestMethod()]
        public void splitDropletTest_DestInSourceBoarder()
        {
            Droplet w1 = new Droplet("Water", "Wat1");
            Droplet w2 = new Droplet("Water", "Wat2");
            Droplet w3 = new Droplet("Water", "Wat3");

            board = Program.C.SetBoard(testBoardDataBigWithMoreHeatLocation);

            board.Droplets.Add("Wat1", w1);
            board.Droplets.Add("Wat2", w2);
            board.Droplets.Add("Wat3", w3);

            w2.nextElectrodeDestination = board.Electrodes[2, 2]; // on boarder
            w3.nextDestination = board.Actuators["heat2"];

            Droplet_Actions.InputDroplet(w1, board.Input["in0"], 84);

            Dictionary<string, int> ratios = [];
            ratios.Add(w2.Name, 50);
            ratios.Add(w3.Name, 50);

            List<string> OutputDroplets = [w2.Name, w3.Name];

            Dictionary<string, double> correctRatios = Calc.FindPercentages(ratios, OutputDroplets);

            Dictionary<string, UsefulSemaphore> sems = new Dictionary<string, UsefulSemaphore>();
            sems.Add(w2.Name, new UsefulSemaphore(1, 2)); // Start with 1 to artificially give it what AwaitWork would
            sems.Add(w3.Name, new UsefulSemaphore(1, 2)); // Start with 1 to artificially give it what AwaitWork would

            Droplet_Actions.SplitDroplet(w1, correctRatios, sems);

            Printer.PrintBoard();

            // Check existence
            Assert.IsTrue(w1.Removed);
            Assert.AreEqual(4, w2.Size);
            Assert.AreEqual(4, w3.Size);

            // Check positions
            Assert.AreEqual(board.Electrodes[3, 3].Occupant, w2);
            Assert.AreEqual(board.Electrodes[3, 4].Occupant, w2);
            Assert.AreEqual(board.Electrodes[4, 4].Occupant, w2);
            Assert.AreEqual(board.Electrodes[3, 5].Occupant, w2);

            Assert.AreEqual(board.Electrodes[0, 5].Occupant, w3);
            Assert.AreEqual(board.Electrodes[0, 6].Occupant, w3);
            Assert.AreEqual(board.Electrodes[1, 5].Occupant, w3);
            Assert.AreEqual(board.Electrodes[0, 7].Occupant, w3);
        }

        [TestMethod()]
        public void splitDropletTest_DestInSource()
        {
            Droplet w1 = new Droplet("Water", "Wat1");
            Droplet w2 = new Droplet("Water", "Wat2");
            Droplet w3 = new Droplet("Water", "Wat3");

            board = Program.C.SetBoard(testBoardDataBigWithMoreHeatLocation);

            board.Droplets.Add("Wat1", w1);
            board.Droplets.Add("Wat2", w2);
            board.Droplets.Add("Wat3", w3);

            w2.nextElectrodeDestination = board.Electrodes[0, 3]; // in source
            w3.nextDestination = board.Actuators["heat2"];

            Droplet_Actions.InputDroplet(w1, board.Input["in0"], 84);

            Dictionary<string, int> ratios = [];
            ratios.Add(w2.Name, 50);
            ratios.Add(w3.Name, 50);

            List<string> OutputDroplets = [w2.Name, w3.Name];

            Dictionary<string, double> correctRatios = Calc.FindPercentages(ratios, OutputDroplets);

            Dictionary<string, UsefulSemaphore> sems = new Dictionary<string, UsefulSemaphore>();
            sems.Add(w2.Name, new UsefulSemaphore(1, 2)); // Start with 1 to artificially give it what AwaitWork would
            sems.Add(w3.Name, new UsefulSemaphore(1, 2)); // Start with 1 to artificially give it what AwaitWork would

            Droplet_Actions.SplitDroplet(w1, correctRatios, sems);

            Printer.PrintBoard();

            // Check existence
            Assert.IsTrue(w1.Removed);
            Assert.AreEqual(4, w2.Size);
            Assert.AreEqual(4, w3.Size);

            // Check positions
            Assert.AreEqual(board.Electrodes[3, 3].Occupant, w2);
            Assert.AreEqual(board.Electrodes[3, 4].Occupant, w2);
            Assert.AreEqual(board.Electrodes[4, 4].Occupant, w2);
            Assert.AreEqual(board.Electrodes[3, 5].Occupant, w2);

            Assert.AreEqual(board.Electrodes[0, 5].Occupant, w3);
            Assert.AreEqual(board.Electrodes[0, 6].Occupant, w3);
            Assert.AreEqual(board.Electrodes[1, 5].Occupant, w3);
            Assert.AreEqual(board.Electrodes[0, 7].Occupant, w3);
        }

        [TestMethod()]
        public void splitDropletTest_WithObstacles()
        {
            Droplet w1 = new Droplet("Water", "Wat1");
            Droplet w2 = new Droplet("Water", "Wat2");
            Droplet w3 = new Droplet("Water", "Wat3");

            board = Program.C.SetBoard(testBoardDataBigWithMoreHeatLocation);

            board.Droplets.Add("Wat1", w1);
            board.Droplets.Add("Wat2", w2);
            board.Droplets.Add("Wat3", w3);

            // Water is contaminated by blood
            board.Droplets["Wat1"].Contamintants = ["Blood"];
            board.Droplets["Wat2"].Contamintants = ["Blood"];
            board.Droplets["Wat3"].Contamintants = ["Blood"];

            w2.nextDestination = board.Actuators["heat1"];
            w2.nextElectrodeDestination = board.Electrodes[7, 0];
            w3.nextDestination = board.Actuators["heat2"];
            w3.nextElectrodeDestination = board.Electrodes[7, 8];

            Droplet_Actions.InputDroplet(w1, board.Input["in0"], 84);

            // Artificially contaminate.
            board.Electrodes[0, 1].Contaminate("Blood");
            board.Electrodes[1, 1].Contaminate("Blood");
            board.Electrodes[2, 1].Contaminate("Blood");
            board.Electrodes[2, 2].Contaminate("Blood");
            board.Electrodes[2, 4].Contaminate("Blood");
            board.Electrodes[2, 5].Contaminate("Blood");
            board.Electrodes[2, 6].Contaminate("Blood");
            board.Electrodes[1, 6].Contaminate("Blood");

            Dictionary<string, int> ratios = [];
            ratios.Add(w2.Name, 50);
            ratios.Add(w3.Name, 50);

            List<string> OutputDroplets = [w2.Name, w3.Name];

            Dictionary<string, double> correctRatios = Calc.FindPercentages(ratios, OutputDroplets);

            Dictionary<string, UsefulSemaphore> sems = new Dictionary<string, UsefulSemaphore>();
            sems.Add(w2.Name, new UsefulSemaphore(1, 2)); // Start with 1 to artificially give it what AwaitWork would
            sems.Add(w3.Name, new UsefulSemaphore(1, 2)); // Start with 1 to artificially give it what AwaitWork would

            Droplet_Actions.SplitDroplet(w1, correctRatios, sems);

            Printer.PrintBoard();

            Assert.IsTrue(w1.Removed);
            Assert.AreEqual(4, w2.Size);
            Assert.AreEqual(4, w2.Occupy.Count);
            Assert.AreEqual(4, w3.Size);
            Assert.AreEqual(4, w3.Occupy.Count);

            Assert.AreEqual(board.Electrodes[0, 7].Occupant, w2);
            Assert.AreEqual(board.Electrodes[0, 8].Occupant, w2);
            Assert.AreEqual(board.Electrodes[1, 8].Occupant, w2);
            Assert.AreEqual(board.Electrodes[1, 8].Occupant, w2);

            Assert.AreEqual(board.Electrodes[0, 5].Occupant, w3);
            Assert.AreEqual(board.Electrodes[1, 5].Occupant, w3);
            Assert.AreEqual(board.Electrodes[1, 4].Occupant, w3);
            Assert.AreEqual(board.Electrodes[1, 3].Occupant, w3);
        }

        [TestMethod()]
        public void TempDropletNormal()
        {
            Droplet Wat1 = new Droplet("Water", "Wat1");
            board = Program.C.SetBoard(testBoardDataBigLocation);
            board.Droplets.Add(Wat1.Name, Wat1);

            Droplet_Actions.InputDroplet(Wat1, board.Input["in0"], 36);

            Droplet_Actions.MoveToApparature(Wat1, board.Actuators["heat1"]);

            int time = 1;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Mission_Tasks.TempDroplet(Wat1, (Heater)board.Actuators["heat1"], time);
            watch.Stop();

            var elapsedMs = watch.ElapsedMilliseconds;

            //Assert.IsTrue(elapsedMs >= time * 1000); Time is no longer slept in simulation
        }

        [TestMethod()]
        public void tempDropletNewType()
        {
            Droplet Wat1 = new Droplet("Water", "Wat1");
            board = Program.C.SetBoard(testBoardDataBigLocation);
            board.Droplets.Add(Wat1.Name, Wat1);

            Droplet_Actions.InputDroplet(Wat1, board.Input["in0"], 11);

            Droplet_Actions.MoveToApparature(Wat1, board.Actuators["heat1"]);

            int time = 1;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Mission_Tasks.TempDroplet(Wat1, (Heater)board.Actuators["heat1"], time, newType : "HotWater");
            watch.Stop();

            var elapsedMs = watch.ElapsedMilliseconds;

            //Assert.IsTrue(elapsedMs >= time * 1000); Time is no longer slept in simulation
            Assert.AreEqual("HotWater", Wat1.Substance_Name);
        }

        [TestMethod()]
        public void TempDropletNoTime()
        {
            Droplet Wat1 = new("Water", "Wat1");
            board = Program.C.SetBoard(testBoardDataBigLocation);
            board.Droplets.Add(Wat1.Name, Wat1);

            Droplet_Actions.InputDroplet(Wat1, board.Input["in0"], 11);

            int time = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Assert.ThrowsException<ArgumentException>(() => Mission_Tasks.TempDroplet(Wat1, (Heater)board.Actuators["heat1"], time));
            watch.Stop();

            var elapsedMs = watch.ElapsedMilliseconds;

            //Assert.IsTrue(elapsedMs >= time * 1000); Time is no longer slept in simulation
        }



    }
}