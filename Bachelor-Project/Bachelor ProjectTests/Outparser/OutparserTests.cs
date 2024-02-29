using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bachelor_Project.Outparser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Simulation;

namespace Bachelor_Project.Outparser.Tests
{
    [TestClass()]
    public class OutparserTests
    {
        [TestMethod()]
        public void ElectrodeOnTest()
        {
            Electrode e = new(20, 20);
            Outparser.ElectrodeOn(e);
            Assert.AreEqual(1, e.Status);
        }

        [TestMethod()]
        public void ElectrodeOffTest()
        {
            Electrode e = new(20, 20);
            Outparser.ElectrodeOn(e);
            Outparser.ElectrodeOff(e);
            Assert.AreEqual(0, e.Status);
        }
    }
}