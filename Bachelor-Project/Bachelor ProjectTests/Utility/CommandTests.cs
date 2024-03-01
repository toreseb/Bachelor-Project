using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bachelor_Project.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Simulation;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.Design;

namespace Bachelor_Project.Utility.Tests
{
    [TestClass()]
    public class CommandTests
    {

        static string inputfiles = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Input Files\\";
        static string testBoardData = "TestBoardData.json";

        [TestInitialize]
        public void Setup()
        {
            Commander C = new(null, inputfiles + "\\" + testBoardData);
            Program.C = C;
        }

        [TestMethod()]
        public void ExecuteInputCommandTest()
        {
            Droplet d = new("water", "test");
            Program.C.board.Droplets.Add("test", d);
            Command command = new("input", [], [d.Name], "in0", "24");
            Assert.AreEqual("input", command.Type);
            command.ExecuteCommand();
            Assert.AreEqual(1, d.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d.GetWork()[0].Status);
            d.Stop();
        }
    }
}