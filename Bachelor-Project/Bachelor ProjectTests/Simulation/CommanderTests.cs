using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bachelor_Project.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Utility;

namespace Bachelor_Project.Simulation.Tests
{
    [TestClass()]
    public class CommanderTests
    {

        static string inputfiles = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Input Files\\";
        static string testBoardData = "TestBoardData.json";
        static string testBoardDataBig = "TestBoardDataBig.json";

        static string testBoardDataLocation = inputfiles + "\\" + testBoardData;
        static string testBoardDataBigLocation = inputfiles + "\\" + testBoardDataBig;

        static string specialBoardDataLocation = inputfiles + "\\" + "Special Boards";


        static string testprogramcode = "testProgram.txt";

        static string testprogramlocation = inputfiles + "\\" + testprogramcode;
        static Board board;


        [TestMethod()]
        public void ProtocolTest()
        {
            var data = Parsing.Parsing.ParseFile(testprogramlocation);
            Commander C = new(data, specialBoardDataLocation + "\\WithSensors.json");
            Program.C = C;
            board = Program.C.board;
            C.Setup();
            C.Start();


        }
    }
}