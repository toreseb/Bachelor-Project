using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Utility;
using Antlr4.Runtime;
using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Electrode_Types.Actuator_Types;

namespace Bachelor_Project.Parsing.Tests
{
    [TestClass()]
    public class ParsingTests
    {
        static string inputfiles = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Input Files\\";
        static string testprogramcode = "testProgram.txt";

        static string testprogramlocation = inputfiles + "\\" + testprogramcode;


        [TestMethod()]
        public void ParseFileTest()
        {
            var data = Parsing.ParseFile(testprogramlocation);
            Assert.AreEqual(data.Item1.Count, 9);
        }
        [TestMethod()]
        public void ParseWrongFileTest()
        {
            string wrongtestprogramlocation = "wrongtest";
            Assert.ThrowsException<FileNotFoundException>(() => Parsing.ParseFile(wrongtestprogramlocation));
        }

        [TestMethod()]
        public void ParseStringTest()
        {
            string text = File.ReadAllText(testprogramlocation);
            var data = Parsing.ParseString(text);
            Assert.AreEqual(data.Item1.Count, 9);
        }
        [TestMethod()]
        public void ParseWrongStringTest()
        {
            string wrongtext = "wrongtest";
            Assert.ThrowsException<ArgumentException>(() => Parsing.ParseString(wrongtext));
        }

        [TestMethod()]
        public void DecodeTest()
        {
            (List<Command> commands, Dictionary<string, string> dropletpairs, Dictionary<string, List<string>> contaminated, Dictionary<string, List<string>> contaminates) data = Parsing.ParseFile(testprogramlocation);

            //Test input
            Command command = data.commands[0];
            Assert.IsTrue(command.Type == "input" && command.InputDroplets.Count == 0 && command.OutputDroplets.Count == 1 && (string)command.ActionValue[0] == "in0" && int.Parse((string)command.ActionValue[1]) == 24);

            //Test output
            command = data.commands[3];
            Assert.IsTrue(command.Type == "output" && command.InputDroplets.Count == 1 && command.OutputDroplets.Count == 0 && command.NextName == "out0" && command.NextType == typeof(Output));

            //Test contam
            Assert.AreEqual(data.contaminated["water"][0], "blood");
            Assert.AreEqual(data.contaminates["blood"][0], "water");

            //Test merge
            command = data.commands[2];
            Assert.IsTrue(command.Type == "merge" && command.InputDroplets.Count == 2 && command.OutputDroplets.Count == 1 && (string)command.ActionValue[0] == "water");

            //Test split
            command = data.commands[1];
            Assert.IsTrue(command.Type == "split" && command.InputDroplets.Count == 1 && command.OutputDroplets.Count == 3);

            //Test mix
            command = data.commands[4];
            Assert.IsTrue(command.Type == "mix" && command.InputDroplets.Count == 1 && command.OutputDroplets.Count == 1 && (string)command.ActionValue[0] == "square" && (string)command.ActionValue[1] == "water");
            command = data.commands[5];
            Assert.IsTrue(command.Type == "mix" && command.InputDroplets.Count == 1 && command.OutputDroplets.Count == 1 && (string)command.ActionValue[0] == "square" && (string)command.ActionValue[1] == "mixedwater");

            //Test temp
            command = data.commands[6];
            Assert.IsTrue(command.Type == "temp" && command.InputDroplets.Count == 1 && command.OutputDroplets.Count == 1 && (string)command.ActionValue[0] == "water" && (string)command.ActionValue[1] == "20" && command.NextName == "heat1" && command.NextType == typeof(Heater));
            command = data.commands[7];
            Assert.IsTrue(command.Type == "temp" && command.InputDroplets.Count == 1 && command.OutputDroplets.Count == 1 && (string)command.ActionValue[0] == "heatedwater" && (string)command.ActionValue[1] == "30" && command.NextName == "heat1" && command.NextType == typeof(Heater));

            //Test sense
            command = data.commands[8];
            Assert.IsTrue(command.Type == "sense" && command.InputDroplets.Count == 1 && command.OutputDroplets.Count == 1 && command.NextName == "sens0" && command.NextType == typeof(Sensor));

        }
    }
}