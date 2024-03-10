using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Utility;
using System.Security.Cryptography;

namespace Bachelor_Project.Simulation.Agent_Actions.Tests
{
    [TestClass()]
    public class Droplet_ActionsTests
    {
        static string inputfiles = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Input Files\\";
        static string testBoardData = "TestBoardData.json";

        static string testBoardDataLocation = inputfiles + "\\" + testBoardData;

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
            // TODO: Input droplet with a destination
            Assert.Fail();
        }

        [TestMethod()]
        public void InputDropletTest_LargeWithoutDest()
        {
            board = Program.C.SetBoard(testBoardDataLocation);
            Droplet test = new Droplet("Water", "Wat1");
            board.Droplets.Add("Wat1", test);
            Droplet_Actions.InputDroplet(test, board.Input["in0"], 60);

            Assert.AreEqual(1, board.Droplets.Count);
            Assert.AreEqual(6, test.Occupy.Count);

            Assert.AreEqual(test, board.Electrodes[0, 0].Occupant);
            Assert.AreEqual(test, board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(test, board.Electrodes[0, 2].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 0].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 2].Occupant);
        }

        [TestMethod()]
        public void InputDropletTest_OntoOtherDroplet()
        {
            // TODO: Unable to test ATM, come back later
            //Assert.Fail();

            // Tests await legal move

            // Create board and both droplets
            board = Program.C.SetBoard(testBoardDataLocation);
            Droplet Wat1 = new Droplet("Water", "Wat1");
            Droplet Wat2 = new Droplet("Water", "Wat2");
            board.Droplets.Add("Wat1", Wat1);
            board.Droplets.Add("Wat2", Wat2);
            Wat1.StartAgent();
            Wat2.StartAgent();

            // Input one droplet
            Task input = new(() => Droplet_Actions.InputDroplet(Wat1, board.Input["in0"], 11));
            Wat1.GiveWork(input);
            input.Wait();

            // Get input coords
            int inX = board.Input["in0"].pointers[0].ePosX;
            int inY = board.Input["in0"].pointers[0].ePosY;

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
            int oldPosX = board.Droplets["Wat1"].Occupy[0].ePosX;
            int oldPosY = board.Droplets["Wat1"].Occupy[0].ePosY;

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
            int oldPosX = board.Droplets["Wat1"].Occupy[0].ePosX;
            int oldPosY = board.Droplets["Wat1"].Occupy[0].ePosY;

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
            int oldPosX = board.Droplets["Wat1"].Occupy[0].ePosX;
            int oldPosY = board.Droplets["Wat1"].Occupy[0].ePosY;

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
            int oldPosX = board.Droplets["Wat1"].Occupy[0].ePosX;
            int oldPosY = board.Droplets["Wat1"].Occupy[0].ePosY;

            // Move droplet down
            Droplet_Actions.MoveDroplet(board.Droplets["Wat1"], Direction.DOWN);

            // Check if move was correct
            Assert.AreEqual(null, board.Electrodes[oldPosX, oldPosY].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[oldPosX, oldPosY + 1].Occupant);
        }

        [TestMethod()]
        public void MoveDropletTest_Large2x3Droplet()
        {
            board = Program.C.SetBoard(testBoardDataLocation);
            Droplet test = new Droplet("Water", "Wat1");
            board.Droplets.Add("Wat1", test);
            Droplet_Actions.InputDroplet(test, board.Input["in0"], 60);

            Assert.AreEqual(1, board.Droplets.Count);
            Assert.AreEqual(6, test.Occupy.Count);

            Assert.AreEqual(test, board.Electrodes[0, 0].Occupant);
            Assert.AreEqual(test, board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(test, board.Electrodes[0, 2].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 0].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 2].Occupant);

            Droplet_Actions.MoveDroplet(test, Direction.RIGHT);

            Assert.AreEqual(test, board.Electrodes[1, 0].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(test, board.Electrodes[1, 2].Occupant);
            Assert.AreEqual(test, board.Electrodes[2, 0].Occupant);
            Assert.AreEqual(test, board.Electrodes[2, 1].Occupant);
            Assert.AreEqual(test, board.Electrodes[2, 2].Occupant);
            

        }

        [TestMethod()]
        public void MoveDropletTest_DropletOverBorder()
        {
            // Input a droplet, in this case 1x1
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 11);

            // Remember old position
            int oldPosX = board.Droplets["Wat1"].Occupy[0].ePosX;
            int oldPosY = board.Droplets["Wat1"].Occupy[0].ePosY;

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
            int oldX = board.Droplets["Wat2"].Occupy[0].ePosX;
            int oldY = board.Droplets["Wat2"].Occupy[0].ePosY;

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
            int watX = board.Droplets["Wat1"].Occupy[0].ePosX;
            int watY = board.Droplets["Wat1"].Occupy[0].ePosY;

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
        public void MoveDropletTest_InsertIntoContam()
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

            // Input water
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 11);

            Assert.AreEqual(null, board.Input["in0"].pointers[0].Occupant);
        }

        [TestMethod()]
        public void MoveDropletTest_InsertAgain()
        {
            // TODO: Input droplet already on board
            Assert.Fail();
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

            Droplet_Actions.Mix(test);
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

            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 24);

            // Uncoil snek
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[3, 1]);

            // Check if expected position
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 1].Occupant);

            // Move down
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.DOWN);

            // Check if expected position
            Assert.AreEqual(null, board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 2].Occupant);
        }

        [TestMethod()]
        public void SnekMoveTest_Right()
        {
            // Done with snake of size 3

            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 24);

            // Uncoil snek
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[3, 1]);

            // Check if expected position
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 1].Occupant);

            // Move right
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);

            // Check if expected position
            Assert.AreEqual(null, board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 1].Occupant);
        }

        [TestMethod()]
        public void SnekMoveTest_Up()
        {
            // Done with snake of size 3

            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 24);

            // Uncoil snek
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[3, 1]);

            // Check if expected position
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 1].Occupant);

            // Move up
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.UP);

            // Check if expected position
            Assert.AreEqual(null, board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 1].Occupant);
        }

        [TestMethod()]
        public void SnekMoveTest_Left()
        {
            // Done with snake of size 3

            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 24);

            // Uncoil snek
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[3, 1]);

            // Check if expected position
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 1].Occupant);

            // Move up, then left
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.UP);
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.LEFT);

            // Check if expected position
            Assert.AreEqual(null, board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(null, board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 0].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 0].Occupant);
        }

        [TestMethod()]
        public void SnekMoveTest_IntoSelf()
        {
            // Need larger snek & board
            Assert.Fail();
        }

        [TestMethod()]
        public void SnekMoveTest_OutOfBoard()
        {
            // Done with snake of size 3

            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 24);

            // Uncoil snek
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[3, 1]);

            // Move right
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);
            // Move right off board
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);

            // Check if expected position
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 1].Occupant);
        }

        [TestMethod()]
        public void SnekMoveTest_BackIntoBody()
        {
            // Done with snake of size 3

            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 24);

            // Uncoil snek
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[3, 1]);

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
            // TODO: There needs to be a test trying to move a droplet that is not input yet.
            // Done with snake of size 2 and other droplet of size 1

            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            board.Droplets.Add("Wat2", new Droplet("Water", "Wat2"));

            // Input and move other droplet (Wat2)
            Droplet_Actions.InputDroplet(board.Droplets["Wat2"], board.Input["in0"], 11);
            Droplet_Actions.MoveDroplet(board.Droplets["Wat2"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Wat2"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Wat2"], Direction.RIGHT);

            // Input and uncoil droplet (Wat1)
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 12);
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[3, 1]);

            // Check state
            Assert.AreEqual(board.Droplets["Wat2"], board.Electrodes[3, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[0,1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1,1].Occupant);

            // Try to move Wat1 into Wat2's boarder
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);

            // Check that it did not move
            Assert.AreEqual(board.Droplets["Wat2"], board.Electrodes[3, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 1].Occupant);
        }

        [TestMethod()]
        public void SnekMoveTest_IntoContam()
        {
            // Done with water snake of size 2 and blood droplet of size 1

            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            board.Droplets.Add("Blood1", new Droplet("Blood", "Blood1"));

            // Water is contaminated by blood
            board.Droplets["Wat1"].Contamintants = ["Blood"];
            board.Droplets["Blood1"].Contamintants = [];

            // Input and move snake (Wat1)
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 12);
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[3, 1]);
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.RIGHT);
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.UP);

            // Input and move blood
            Droplet_Actions.InputDroplet(board.Droplets["Blood1"], board.Input["in0"], 11);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.DOWN);
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.LEFT); // move water out of the way
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);
            Droplet_Actions.MoveDroplet(board.Droplets["Blood1"], Direction.RIGHT);

            // Move water close to contam
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.LEFT);

            // Check positions and contam
            Assert.IsTrue(board.Electrodes[1,1].GetContaminants().Any());
            Assert.AreEqual(null, board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Blood1"], board.Electrodes[3, 2].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 0].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 0].Occupant);

            // Try to move into contam
            Droplet_Actions.SnekMove(board.Droplets["Wat1"], Direction.DOWN);

            // Check that snake did not move
            Assert.AreEqual(null, board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 0].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 0].Occupant);
        }

        [TestMethod()]
        public void SnekReversalTest()
        {
            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 24);
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[3, 1]);

            Electrode oldHead = board.Droplets["Wat1"].Occupy[0];
            Electrode newHead = board.Droplets["Wat1"].Occupy[^1];

            Assert.AreEqual(oldHead, board.Droplets["Wat1"].Occupy[0]);

            Droplet_Actions.SnekReversal(board.Droplets["Wat1"]);

            Assert.AreEqual(newHead, board.Droplets["Wat1"].Occupy[0]);
        }

        [TestMethod()]
        public void CoilSnekTest()
        {
            // Test with snake size 3 (to not be at boarder)
            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 24);
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[3, 1]);

            // Check placement
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[0, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 1].Occupant);

            // Coil
            Droplet_Actions.CoilSnek(board.Droplets["Wat1"]);

            // Check placement
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 0].Occupant);
        }

        [TestMethod()]
        public void CoilSnekTest_AtBoarderRight()
        {
            // Test with snake size 4 (to not be at boarder)
            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 36);
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[3, 1]);

            // Check placement
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[0, 0].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[1, 0].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 0].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 0].Occupant);

            // Coil
            Droplet_Actions.CoilSnek(board.Droplets["Wat1"]);

            // Check placement
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 0].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 0].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[3, 1].Occupant);
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[2, 1].Occupant);
        }

        [TestMethod()]
        public void CoilSnekTest_AtBoarderTop()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CoilSnekTest_AtBoarderLeft()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CoilSnekTest_AtBoarderBottom()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CoilSnekTest_CloseToOtherDroplet()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CoilSnekTest_InTightSpace()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UncoilSnekTest()
        {
            // Uncoil droplet of size 4

            // Create board and input droplet
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 36);

            // Larger droplets are coiled around input
            Assert.AreEqual(board.Droplets["Wat1"], board.Input["in0"].pointers[0].Occupant); // On in
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[board.Input["in0"].pointers[0].ePosX, board.Input["in0"].pointers[0].ePosY - 1].Occupant); // Above in
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[board.Input["in0"].pointers[0].ePosX, board.Input["in0"].pointers[0].ePosY + 1].Occupant); // Below in
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[board.Input["in0"].pointers[0].ePosX + 1, board.Input["in0"].pointers[0].ePosY - 1].Occupant); // Above, right in

            // Uncoil snek towards 3,1
            Droplet_Actions.UncoilSnek(board.Droplets["Wat1"], board.Electrodes[3, 1]);

            // Check
            Assert.Fail();
            // Frick. Boardet er ikke stort nok og der er fejl i uncoil snek.
            // Det giver nul mening bare at have den til at gå til højre, hvad hvis den man skal hen mod er til venstre?
            // Den stopper heller ikke hvis man går uden for boardet.
            
        }

        [TestMethod()]
        public void UncoilSnekTest_AtBoarder()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UncoilSnekTest_CloseToOtherDroplet()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UncoilSnekTest_InSmallSpace()
        {
            Assert.Fail();
        }
    }
}