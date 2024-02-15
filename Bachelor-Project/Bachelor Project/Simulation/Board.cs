using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Electrode_Types.Actuator_Types;
using Bachelor_Project.Electrode_Types.Sensor_Types;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    internal class Board
    {
        public Electrode[,] Electrodes  { get; set; }     
        public Actuator[] Actuators     { get; set; }
        public Sensor[] Sensors         { get; set; }
        public Input[] Input    { get; set; }
        public Output[] Output { get; set; }
        public Droplet[] Droplets       { get; set; }
        public Information Information{ get; set; }
        public String?[] Unclassified   { get; set; }
        
        public Board(Information information, Electrode[,] electrodes, Actuator[] actuators, Sensor[] sensors, Input[] input, Output[] output, Droplet[] droplets, String?[] unclassified)
        {
            this.Information = information;
            this.Electrodes = electrodes;
            this.Actuators = actuators;
            this.Sensors = sensors;
            this.Input = input;
            this.Output = output;
            this.Droplets = droplets;
            this.Unclassified = unclassified;

        }

        private void InitElectrodes(Electrode[,] electrodes)
        {

        }

        public void SetElectrodes(Electrode[,] electrodes)
        {
            this.Electrodes = electrodes;
        }

        public Electrode[,] GetElectrodes()
        {
            return Electrodes;
        }

        public int GetWidth()
        {
            return Information.sizeX;
        }

        public int GetHeight()
        {
            return Information.sizeY;
        }

        static public Board ImportBoardData(string json)
        {
            JObject jObject = JObject.Parse(json);

            Information i = jObject["information"][0].ToObject<Information>();
            Electrode[] eList = jObject["electrodes"].ToObject<Electrode[]>();
            int eRow = i.sizeX / eList[0].SizeX; // Number of electrodes in a row
            int eColumn = i.sizeY / eList[0].SizeY; // Number of electrodes in a column
            Electrode[,] eArray = new Electrode[eRow,eColumn];
            for (int j = 0; j < eList.Length; j++)
            {
                eArray[j % eRow, j / eRow] = eList[j];
            }

            JArray aList = jObject["actuators"] != null ? (JArray)jObject["actuators"] : [] ;
            Actuator[] actuators = [];
            foreach (var item in aList)
            {
                switch (item["type"].ToString())
                {
                    case "heater":
                        actuators = [..actuators, item.ToObject<Heater>()];
                        break;
                    default:
                        Console.WriteLine("Actuator type not recognized");
                        break;
                }
            }
            Console.WriteLine(jObject["sensors"]);
            JArray sList = jObject["sensors"] == null ? (JArray)jObject["sensors"] : [] ;
            Sensor[] sensors = [];
            foreach (var item in sList)
            {
                switch (item.Type.ToString())
                {
                    case "RGB_color":
                        sensors = [..sensors,item.ToObject<RGB_Sensor>()];
                        break;
                    case "size":
                        sensors = [..sensors,item.ToObject<SizeSensor>()];
                        break;
                    default:
                        Console.WriteLine("Sensor type not recognized");
                        break;
                }
            }
            Input[] iList = jObject["inputs"] == null ? jObject["input"].ToObject<Input[]>():[] ;
            Output[] oList = jObject["outputs"] == null? jObject["input"].ToObject<Output[]>() : [];
            Droplet[] droplets = jObject["droplets"] == null ? jObject["input"].ToObject<Droplet[]>() : [];
            String?[] unclassified = jObject["unclassified"] == null ? jObject["input"].ToObject<string?[]>() : [];

            Console.WriteLine(i.sizeX);
            return new Board(i, eArray, actuators, sensors, iList, oList, droplets, unclassified);
        }

        public void PrintBoardState()
        {
            // Write horizontal lines one by one
            for (int i = 0; i < Information.sizeY/20; i++)
            {
                Console.WriteLine(BuildPrintLine(i));
            }
        }

        public string BuildPrintLine(int h)
        {
            String line = string.Empty;

            // Check each electrode in line and add to string - O (empty), Z (contaminated), X (droplet)
            for (int i = 0; i < Information.sizeX/20; i++)
            {
                if (Electrodes[i,h].GetStatus() == 0)
                {
                    String[] cont = Electrodes[i,h].GetContaminants();
                    if (cont.Length == 0)
                    {
                        // Tile is completely clear
                        line += "O";
                    }
                    else
                    {
                        // Tile has no droplet but is contaminated
                        line += "Z";
                    }
                }
                else
                {
                    // Tile has a droplet
                    line += "X";
                }
                
            }

            return line;
        }
    }
}
