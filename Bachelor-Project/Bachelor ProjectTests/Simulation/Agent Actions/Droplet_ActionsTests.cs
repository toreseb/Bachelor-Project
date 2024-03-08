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
            // TODO: Input droplet without a destination (coils around input)
            Assert.Fail();
        }

        [TestMethod()]
        public void InputDropletTest_OntoOtherDroplet()
        {
            // TODO: Unable to test ATM, come back later
            Assert.Fail();

            // Tests await legal move

            // Create board and both droplets
            board = Program.C.SetBoard(testBoardDataLocation);
            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            board.Droplets.Add("Wat2", new Droplet("Water", "Wat2"));

            // Input one droplet
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 11);

            // Get input coords
            int inX = board.Input["in0"].pointers[0].ePosX;
            int inY = board.Input["in0"].pointers[0].ePosY;

            // Check that Wat1 is at in0
            Assert.AreEqual(board.Droplets["Wat1"], board.Electrodes[inX,inY].Occupant);

            // Try to input other droplet
            Droplet_Actions.InputDroplet(board.Droplets["Wat2"], board.Input["in0"], 11);

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
            // TODO: Move a 2x3 droplet
            Assert.Fail();
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
    }
}