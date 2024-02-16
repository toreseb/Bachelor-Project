using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Electrode_Types.Actuator_Types;
using Bachelor_Project.Electrode_Types.Sensor_Types;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
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
            i.electrode_size = eList[0].SizeX;
            i.eRow = i.sizeX / i.electrode_size; // Number of electrodes in a row
            i.eCol = i.sizeY / i.electrode_size; // Number of electrodes in a column
            Electrode[,] eArray = new Electrode[i.eRow,i.eCol];
            for (int j = 0; j < eList.Length; j++)
            {
                eArray[j % i.eRow, j / i.eRow] = eList[j];
                eList[j].ePosX = j % i.eRow;
                eList[j].ePosY = j / i.eRow;
            }

            JArray aList = jObject["actuators"].Type != JTokenType.Null ? (JArray)jObject["actuators"] : [] ;
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
            Console.WriteLine(jObject["sensors"].Type != JTokenType.Null);
            JArray sList = jObject["sensors"].Type != JTokenType.Null ? (JArray)jObject["sensors"] : [] ;
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
            Input[] iList = jObject["inputs"].Type != JTokenType.Null ? jObject["inputs"].ToObject<Input[]>():[] ;
            foreach (var item in iList)
            {
                item.point = eArray[item.PositionX/i.electrode_size,item.PositionY/i.electrode_size];
            }
            Output[] oList = jObject["outputs"].Type != JTokenType.Null ? jObject["outputs"].ToObject<Output[]>() : [];
            foreach (var item in oList)
            {
                item.point = eArray[item.PositionX / i.electrode_size, item.PositionY / i.electrode_size];
            }
            Droplet[] droplets = jObject["droplets"].Type != JTokenType.Null ? jObject["droplets"].ToObject<Droplet[]>() : [];
            String?[] unclassified = jObject["unclassified"].Type != JTokenType.Null ? jObject["unclassified"].ToObject<string?[]>() : [];

            Console.WriteLine(i.sizeX);
            return new Board(i, eArray, actuators, sensors, iList, oList, droplets, unclassified);
        }

        public void PrintBoardState() // Row and Col are switched, so the board is printed correctly
        {
            ArrayList[][] squares = new ArrayList[Information.eCol][];
            for (int i = 0; i < Information.eCol; i++)
            {
                squares[i] = new ArrayList[Information.eRow];
                for (int j = 0; j < Information.eRow; j++)
                {
                    squares[i][j] = new ArrayList();
                }
            }

            foreach (var actuator in Actuators)
            {
                int startX = actuator.PositionX / Information.electrode_size;
                int startY = actuator.PositionY / Information.electrode_size;

                int endX = (actuator.PositionX + actuator.SizeX) / Information.electrode_size;
                int endY = (actuator.PositionY + actuator.SizeY) / Information.electrode_size;
                int i = endX - startX;
                while(i >= 0)
                {
                    int j = endY - startY;
                    while(j >= 0)
                    {
                        squares[startY + j][startX + i].Add(actuator);
                        j--;
                    }
                    i--;
                }

            }

            foreach (var sensor in Sensors)
            {
                int startX = sensor.PositionX / Information.electrode_size;
                int startY = sensor.PositionY / Information.electrode_size;

                int endX = (sensor.PositionX + sensor.SizeX) / Information.electrode_size;
                int endY = (sensor.PositionY + sensor.SizeY) / Information.electrode_size;
                int i = endX - startX;
                while (i >= 0)
                {
                    int j = endY - startY;
                    while (j >= 0)
                    {
                        squares[startY + j][startX + i].Add(sensor);
                        j--;
                    }
                    i--;
                }
            }

            foreach (var input in Input)
            {
                squares[input.point.ePosY][input.point.ePosX].Add(input);
            }

            foreach (var output in Output)
            {
                squares[output.point.ePosY][output.point.ePosX].Add(output);
            }
            
            Console.WriteLine("Board State:");
            Console.WriteLine(new String('-',1+Information.eRow*(3+6)));
            // Write horizontal lines one by one
            for (var j = 0; j < squares.Length; j++)
            {
                Console.WriteLine(BuildPrintLine(squares[j],j));
                Console.WriteLine(new String('-', 1 + Information.eRow * (3 + 6)));
            }
            
        }

        public string BuildPrintLine(ArrayList[] row, int j)
        {
            String line1 = "| ";
            String line2 = "| ";
            String line3 = "| ";
            int i = 0;
            foreach (var square in row)
            {
                int used1 = 0;
                int used2 = 0;
                int used3 = 0;
                foreach (var item in square)
                {
                    
                    Type t = item.GetType();
                    if (t.IsSubclassOf(typeof(Actuator)))
                    {
                        line1 += ((Actuator)item).Name;
                        used1 += ((Actuator)item).Name.Length;
                    }
                    else if (t.IsSubclassOf(typeof(Sensor)))
                    {
                        line1 += ((Sensor)item).Name;
                        used1 += ((Sensor)item).Name.Length;
                    }
                    else if (t.IsSubclassOf(typeof(Accessor)))
                    {
                        line3 += ((Accessor)item).Name;
                        used3 += ((Accessor)item).Name.Length;
                    }
                }
                
                

                if (Electrodes[i,j].Occupant != null)
                {
                    line2 += Electrodes[i,j].Occupant.Name;
                    used2 += Electrodes[i,j].Occupant.Name.Length;
                }else if (Electrodes[i,j].GetContaminants().Length != 0)
                {
                    line2 += "Z";
                    used2 += 1;
                }
                else
                {
                    line2 += Electrodes[i, j].Name;
                    used2 += Electrodes[i, j].Name.Length;
                }
                line1 += new String(' ', 6 - used1) + " | ";
                line2 += new String(' ', 6 - used2) + " | ";
                line3 += new String(' ', 6-used3) + " | ";
                i++;
            }
            string totalline = line1 + "\n" + line2 + "\n" + line3;

            return totalline;
        }
    }
}
