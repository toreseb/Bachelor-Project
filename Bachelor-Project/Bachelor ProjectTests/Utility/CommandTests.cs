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
        [TestInitialize]
        public void Setup()
        {
            //Commander C = new(null, inputfiles + "\\" + boarddata);
        }

        [TestMethod()]
        public void ExecuteInputCommandTest()
        {
            Droplet d = new Droplet("water", "test");
            Command command = new("input", [], [d.Name], "in0", "24");
            Assert.AreEqual("input", command.Type);
            command.ExecuteCommand();
            Assert.AreEqual(1, d.GetWork().Count);
            Assert.AreEqual(TaskStatus.WaitingForActivation, d.GetWork()[0].Status);
            d.StartAgent();
            Assert.AreEqual(TaskStatus.Running, d.GetWork()[0].Status);
            d.Stop();
        }
    }
}