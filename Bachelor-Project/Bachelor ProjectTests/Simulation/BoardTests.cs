using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bachelor_Project.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Electrode_Types.Actuator_Types;
using Bachelor_Project.Electrode_Types;

namespace Bachelor_Project.Simulation.Tests
{
    [TestClass()]
    public class BoardTests
    {
        static string inputfiles = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Input Files\\";
        static string testBoardData = "TestBoardData.json";

        static string testBoardDataLocation = inputfiles + "\\" + testBoardData;

        [TestMethod()]
        public void ImportBoardDataTest()
        {
            string json = File.ReadAllText(testBoardDataLocation);
            Board board = Board.ImportBoardData(json);

            // Test Information
            Assert.AreEqual("4by3example", board.Information.platform_name);
            Assert.AreEqual("example", board.Information.platform_type);
            Assert.AreEqual("012345678", board.Information.platform_ID);
            Assert.AreEqual(80, board.Information.sizeX);
            Assert.AreEqual(60, board.Information.sizeY);
            Assert.AreEqual(20, board.Information.electrode_size);
            Assert.AreEqual(4, board.Information.eRow);
            Assert.AreEqual(3, board.Information.eCol);

            // Test Electrodes
            Assert.AreEqual(12, board.Electrodes.Length);
            for (int j = 0; j < board.Electrodes.Length; j++)
            {
                Electrode e = board.Electrodes[j % board.GetXElectrodes(), j / board.GetXElectrodes()];
                Assert.AreEqual("el" + j, e.Name);
                Assert.AreEqual(j, e.ID);
                Assert.AreEqual(-1, e.DriverID);
                Assert.AreEqual((j % board.GetXElectrodes()) * 20 , e.PositionX);
                Assert.AreEqual((j % board.GetXElectrodes()), e.ePosX);
                Assert.AreEqual((j / board.GetXElectrodes()) * 20, e.PositionY);
                Assert.AreEqual((j / board.GetXElectrodes()), e.ePosY);
                Assert.AreEqual(20, e.SizeX);
                Assert.AreEqual(20, e.SizeY);
                Assert.AreEqual(0, e.Status);
            }

            // Test Actuators
            Assert.AreEqual(1, board.Actuators.Count);
            Heater a = (Heater)board.Actuators["heat1"];
            Assert.AreEqual("heat1", a.Name);
            Assert.AreEqual(2, a.ID);
            Assert.AreEqual(30, a.ActuatorID);
            Assert.AreEqual("heater", a.Type);
            Assert.AreEqual(20, a.PositionX);
            Assert.AreEqual(0, a.PositionY);
            Assert.AreEqual(20, a.SizeX);
            Assert.AreEqual(20, a.SizeY);
            Assert.AreEqual(0, a.ActualTemperature);
            Assert.AreEqual(10, a.DesiredTemperature);
            Assert.AreEqual(false, a.Status);
            Assert.AreEqual(0, a.NextDesiredTemperature);
            Assert.AreEqual(false, a.NextStatus);

            // Test Sensors : TODO not in the test data

            // Test Inputs
            Assert.AreEqual(1, board.Input.Count);
            Input i = board.Input["in0"];
            Assert.AreEqual("in0", i.Name);
            Assert.AreEqual(12, i.ID);
            Assert.AreEqual(0, i.InputID);
            Assert.AreEqual(10, i.PositionX);
            Assert.AreEqual(30, i.PositionY);
            Assert.AreEqual(1, i.pointers.Count);
            Assert.AreEqual("el4", i.pointers[0].Name);

            // Test Outputs
            Assert.AreEqual(1, board.Output.Count);
            Output o = board.Output["out0"];
            Assert.AreEqual("out0", o.Name);
            Assert.AreEqual(13, o.ID);
            Assert.AreEqual(0, o.OutputID);
            Assert.AreEqual(70, o.PositionX);
            Assert.AreEqual(30, o.PositionY);
            Assert.AreEqual(1, o.pointers.Count);
            Assert.AreEqual("el7", o.pointers[0].Name);

            // Test Droplets : TODO not in the test data and shouldn't be

            // Test Uncategorized : TODO not sure what to do with this

        }
    }
}