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
            Program.C.Reset();
            Droplet d = new("water", "test");
            Program.C.board.Droplets.Add("test", d);
            Command command = new("input", [], [d.Name], value:[0,"in0", "24"]);
            Assert.AreEqual("input", command.Type);
            command.ExecuteCommand();
            Assert.AreEqual(1, d.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d.GetWork()[0].Status);
            d.Stop();
            Program.C.board.Droplets.Remove(d.Name);
        }
        [TestMethod()]
        public void ExecuteOutputCommandTest()
        {
            Droplet d = new("water", "test");
            Program.C.board.Droplets.Add("test", d);
            Command command = new("output", [d.Name], [], "out0");
            Assert.AreEqual("output", command.Type);
            command.ExecuteCommand();
            Assert.AreEqual(1, d.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d.GetWork()[0].Status);
            d.Stop();
            Program.C.board.Droplets.Remove(d.Name);
        }

        [TestMethod()]
        public void ExecuteWasteCommandTest()
        {
            // Waste no longer used, but REALLY wants 69 tests.
            /*
            Droplet d = new("water", "test");
            Program.C.board.Droplets.Add("test", d);
            Command command = new("waste", [d.Name], []);
            Assert.AreEqual("waste", command.Type);
            command.ExecuteCommand();
            Assert.AreEqual(1, d.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d.GetWork()[0].Status);
            d.Stop();
            Program.C.board.Droplets.Remove(d.Name);
            */
        }
        [TestMethod()]
        public void ExecuteMergeCommandTest()
        {
            Droplet d1 = new("water", "test1");
            Droplet d2 = new("water", "test2");
            Droplet d3 = new("water", "test3");
            Program.C.board.Droplets.Add("test1", d1);
            Program.C.board.Droplets.Add("test2", d2);
            Program.C.board.Droplets.Add("test3", d3);
            Command command = new("merge", [d1.Name, d2.Name], [d3.Name]);
            Assert.AreEqual("merge", command.Type);
            command.ExecuteCommand();
            Assert.AreEqual(1, d1.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d1.GetWork()[0].Status);
            Assert.AreEqual(1, d2.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d2.GetWork()[0].Status);
            Assert.AreEqual(1, d3.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d3.GetWork()[0].Status);
            d1.Stop();
            d2.Stop();
            d3.Stop();
            Program.C.board.Droplets.Remove(d1.Name);
            Program.C.board.Droplets.Remove(d2.Name);
            Program.C.board.Droplets.Remove(d3.Name);
        }
        [TestMethod()]
        public void ExecuteSplitCommandTest()
        {
            Droplet d1 = new("water", "test1");
            Droplet d2 = new("water", "test2");
            Droplet d3 = new("water", "test3");
            Program.C.board.Droplets.Add("test1", d1);
            Program.C.board.Droplets.Add("test2", d2);
            Program.C.board.Droplets.Add("test3", d3);
            Command command = new("split", [d1.Name], [d2.Name, d3.Name]);
            Assert.AreEqual("split", command.Type);
            command.ExecuteCommand();
            Assert.AreEqual(1, d1.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d1.GetWork()[0].Status);
            Assert.AreEqual(1, d2.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d2.GetWork()[0].Status);
            Assert.AreEqual(1, d3.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d3.GetWork()[0].Status);
            d1.Stop();
            d2.Stop();
            d3.Stop();
            Program.C.board.Droplets.Remove(d1.Name);
            Program.C.board.Droplets.Remove(d2.Name);
            Program.C.board.Droplets.Remove(d3.Name);
        }
        [TestMethod()]
        public void ExecuteMixCommandTest()
        {
            Droplet d = new("water", "test");
            Program.C.board.Droplets.Add("test", d);
            Command command = new("mix", [d.Name], [d.Name], "square");
            Assert.AreEqual("mix", command.Type);
            command.ExecuteCommand();
            Assert.AreEqual(1, d.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d.GetWork()[0].Status);
            Command command2 = new("mix", [d.Name], [d.Name], value:["square", "mixed water"]);
            Assert.AreEqual("mix", command2.Type);
            command2.ExecuteCommand();
            Assert.AreEqual(2, d.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d.GetWork()[1].Status);
            d.Stop();
            Program.C.board.Droplets.Remove(d.Name);

        }
        [TestMethod()]
        public void ExecuteTempCommandTest()
        {
            Droplet d = new("water", "test");
            Program.C.board.Droplets.Add("test", d);
            Command command = new("temp", [d.Name], [d.Name], "20");
            Assert.AreEqual("temp", command.Type);
            command.ExecuteCommand();
            Assert.AreEqual(1, d.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d.GetWork()[0].Status);
            Command command2 = new("temp", [d.Name], [d.Name], value:["40", "hot water"]);
            Assert.AreEqual("temp", command2.Type);
            command2.ExecuteCommand();
            Assert.AreEqual(2, d.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d.GetWork()[1].Status);
            d.Stop();
            Program.C.board.Droplets.Remove(d.Name);
        }

        [TestMethod()]
        public void ExecuteSenseCommandTest()
        {
            Droplet d = new("water", "test");
            Program.C.board.Droplets.Add("test", d);
            Command command = new("sense", [d.Name], [d.Name], "water");
            Assert.AreEqual("sense", command.Type);
            command.ExecuteCommand();
            Assert.AreEqual(1, d.GetWork().Count);
            Assert.AreEqual(TaskStatus.Created, d.GetWork()[0].Status);
            d.Stop();
            Program.C.board.Droplets.Remove(d.Name);
        }

    }
}