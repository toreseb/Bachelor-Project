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

        static string testprogramlocation = inputfiles + "\\" + testBoardData;

        static string json = File.ReadAllText(testprogramlocation);
        public static Board board;




        [TestMethod()]
        public void InputDropletTest()
        {
            // Input a droplet of size 1 electrode (1-12 ml)
            board = Board.ImportBoardData(json);

            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 12);

            Assert.IsTrue(board.Electrodes.Length == 1);

            int amountOn = 0;
            foreach (Electrode e in board.Electrodes)
            {
                if (e.Occupant != null) { amountOn++; }
            }

            Assert.IsTrue (amountOn == 1);

            Assert.IsTrue(board.Input["in0"].pointers[0].Status == 1);

            // Input a droplet of size larger than 1 electrode (2: 13-24 ml)
            board = Board.ImportBoardData(json);

            board.Droplets.Add("Wat1", new Droplet("Water", "Wat1"));
            Droplet_Actions.InputDroplet(board.Droplets["Wat1"], board.Input["in0"], 24);

            Assert.IsTrue(board.Electrodes.Length == 2);

            amountOn = 0;
            foreach (Electrode e in board.Electrodes)
            {
                if (e.Occupant != null) { amountOn++; }
            }

            Assert.IsTrue(amountOn == 2);

            // TODO: Input droplet with a destination

            // TODO: Input droplet without a destination (coils around input)
        }
    }
}