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
        public void MoveDropletTest_Small1x1Droplet()
        {
            // TODO: Move a 1x1 droplet
            Assert.Fail();
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
            // TODO: Try to move a droplet outside the board
            Assert.Fail();
        }

        [TestMethod()]
        public void MoveDropletTest_MoveIntoDroplet()
        {
            // TODO: Try to move a droplet into another droplet
            Assert.Fail();
        }

        [TestMethod()]
        public void MoveDropletTest_MoveCloseToDroplet()
        {
            // TODO: Try to move a droplet close to another droplet
            Assert.Fail();
        }

        [TestMethod()]
        public void MoveDropletTest_MoveIntoContam()
        {
            Assert.Fail();
        }
    }
}