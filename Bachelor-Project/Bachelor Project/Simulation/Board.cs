using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Electrode_Types.Actuator_Types;
using Bachelor_Project.Electrode_Types.Sensor_Types;
using Bachelor_Project.Utility;
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
    public class Board
    {
        public Electrode[,] Electrodes  { get; set; }     
        public Dictionary<string,Actuator> Actuators     { get; set; }
        public Dictionary<string,Sensor> Sensors         { get; set; }
        public Dictionary<string,Input> Input    { get; set; }
        public Dictionary<string, Output> Output { get; set; }
        public Dictionary<string, Droplet> Droplets       { get; set; }
        public Information Information{ get; set; }
        public String?[] Unclassified   { get; set; }

        // For printing
        int Squarewidth = 6;

        public Board(Information information, Electrode[,] electrodes, Dictionary<String, Actuator> actuators, Dictionary<String, Sensor> sensors, Dictionary<String, Input> input, Dictionary<String, Output> output, Dictionary<String, Droplet> droplets, String?[] unclassified)
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

        public int GetXElectrodes()
        {
            return Information.eRow;
        }

        public int GetYElectrodes()
        {
            return Information.eCol;
        }

        static public Board ImportBoardData(string json)
        {
            JObject jObject = JObject.Parse(json);

            Information inf = jObject["information"][0].ToObject<Information>();
            Electrode[] eList = jObject["electrodes"].ToObject<Electrode[]>();
            inf.electrode_size = eList[0].SizeX;
            inf.eRow = inf.sizeX / inf.electrode_size; // Number of electrodes in a row
            inf.eCol = inf.sizeY / inf.electrode_size; // Number of electrodes in a column
            Electrode[,] eArray = new Electrode[inf.eRow,inf.eCol];
            for (int j = 0; j < eList.Length; j++)
            {
                eArray[j % inf.eRow, j / inf.eRow] = eList[j];
                eList[j].ePosX = j % inf.eRow;
                eList[j].ePosY = j / inf.eRow;
            }

            JArray aList = jObject["actuators"].Type != JTokenType.Null ? (JArray)jObject["actuators"] : [] ;
            Dictionary<string, Actuator> actuators = [];
            
            foreach (var item in aList)

            {
                switch (item["type"].ToString())
                {
                    case "heater":
                        actuators.Add(item["name"].ToString(), item.ToObject<Heater>());
                        break;
                    default:
                        Printer.PrintLine("Actuator type not recognized");
                        break;
                    }
                int startX = (int)item["positionX"] / inf.electrode_size;
                int startY = (int)item["positionY"] / inf.electrode_size;

                int endX = ((int)item["positionX"] + (int)item["sizeX"]) / inf.electrode_size;
                int endY = ((int)item["positionY"] + (int)item["sizeY"]) / inf.electrode_size;
                int i = endX - startX;
                while (i >= 0)
                {
                    int j = endY - startY;
                    while (j >= 0)
                    {
                        actuators[item["name"].ToString()].pointers.Add(eArray[startX + i, startY + j]);
                        eArray[startX + i, startY + j].Apparature = actuators[item["name"].ToString()];
                        j--;
                    }
                    i--;
                }

                
                
            }
            JArray sList = jObject["sensors"].Type != JTokenType.Null ? (JArray)jObject["sensors"] : [] ;
            Dictionary<string,Sensor> sensors = [];
            foreach (var item in sList)
            {
                Sensor trueItem;
                switch (item["type"].ToString())
                {
                    case "RGB_color":
                        trueItem = item.ToObject<RGBSensor>();
                        break;
                    case "size":
                        trueItem = item.ToObject<SizeSensor>();
                        break;
                    default:
                        Printer.PrintLine("Sensor type not recognized");
                        throw new NotImplementedException();
                }
                sensors.Add(item["name"].ToString(), trueItem);
                trueItem.pointers.Add(eArray[trueItem.PositionX/inf.electrode_size, trueItem.PositionY/inf.electrode_size]);
                eArray[trueItem.PositionX / inf.electrode_size, trueItem.PositionY / inf.electrode_size].Apparature = trueItem;
            }
            Input[] iList = jObject["inputs"].Type != JTokenType.Null ? jObject["inputs"].ToObject<Input[]>():[] ;
            Dictionary<string, Input> iDict = [];
            foreach (var item in iList)
            {
                iDict.Add(item.Name, item);
                item.pointers.Add(eArray[item.PositionX/inf.electrode_size,item.PositionY/inf.electrode_size]);
                eArray[item.PositionX / inf.electrode_size, item.PositionY / inf.electrode_size].Apparature = item;
            }
            Output[] oList = jObject["outputs"].Type != JTokenType.Null ? jObject["outputs"].ToObject<Output[]>() : [];
            Dictionary<string, Output> oDict = [];
            foreach (var item in oList)
            {
                oDict.Add(item.Name, item);
                item.pointers.Add(eArray[item.PositionX / inf.electrode_size, item.PositionY / inf.electrode_size]);
                eArray[item.PositionX / inf.electrode_size, item.PositionY / inf.electrode_size].Apparature = item;
            }
            Droplet[] droplets = jObject["droplets"].Type != JTokenType.Null ? jObject["droplets"].ToObject<Droplet[]>() : [];
            Dictionary<string, Droplet> dDict = [];
            foreach (var item in droplets)
            {
                dDict.Add(item.Name, item);
            }
            string?[] unclassified = jObject["unclassified"].Type != JTokenType.Null ? jObject["unclassified"].ToObject<string?[]>() : [];

            return new Board(inf, eArray, actuators, sensors, iDict, oDict, dDict, unclassified);
        }

        public bool PrintBoardState() // Row and Col are switched, so the board is printed correctly
        {
            List<TileEntity>[][] squares = new List<TileEntity>[Information.eCol][];
            for (int i = 0; i < Information.eCol; i++)
            {
                squares[i] = new List<TileEntity>[Information.eRow];
                for (int j = 0; j < Information.eRow; j++)
                {
                    squares[i][j] = [];
                }
            }
            Actuators.Values.ToList().ForEach(x => x.pointers.ToList().ForEach(y => squares[y.ePosY][y.ePosX].Add(x)));
            
            Sensors.Values.ToList().ForEach(x => x.pointers.ToList().ForEach(y => squares[y.ePosY][y.ePosX].Add(x)));

            Input.Values.ToList().ForEach(x => squares[x.pointers[0].ePosY][x.pointers[0].ePosX].Add(x));

            Output.Values.ToList().ForEach(x => squares[x.pointers[0].ePosY][x.pointers[0].ePosX].Add(x));

            Console.WriteLine("Board State:");
            Console.WriteLine(new string('-',1+Information.eRow*(3+Squarewidth)));
            // Write horizontal lines one by one
            for (var j = 0; j < squares.Length; j++)
            {
                Console.WriteLine(BuildPrintLine(squares[j],j));
                Console.WriteLine(new string('-', 1 + Information.eRow * (3 + 6)));
            }
            Console.WriteLine();
            return true;
        }

        public string BuildPrintLine(List<TileEntity>[] row, int j)
        {
            
            string line1 = "| ";
            string line2 = "| ";
            string line3 = "| ";
            int i = 0;
            foreach (var square in row)
            {
                int used1 = 0;
                int used2 = 0;
                int used3 = 0;
                /*
                if (Electrodes[i, j].smallestGScore != null)
                {
                    line1 += Electrodes[i, j].smallestGScore.ToString();
                    used1 += Electrodes[i, j].smallestGScore.ToString().Length;
                }
                */
                
                foreach (var item in square)
                {
                    string name;
                    if (item.Name.Length > Squarewidth)
                    {
                        name = item.Name[..Squarewidth];
                    }
                    else
                    {
                        name = item.Name;
                    }
                    Type t = item.GetType();
                    if (t.IsSubclassOf(typeof(Actuator)) || t.IsSubclassOf(typeof(Sensor)))
                    {
                        line1 += name;
                        used1 += name.Length;
                    }
                    else if (t.IsSubclassOf(typeof(Accessor)))
                    {
                        line3 += name;
                        used3 += name.Length;
                    }
                }
                
                

                if (Electrodes[i,j].Occupant != null)
                {
                    string name;
                    if (Electrodes[i, j].Occupant.Name.Length > Squarewidth)
                    {
                        name = Electrodes[i, j].Occupant.Name[..Squarewidth];
                    }
                    else
                    {
                        name = Electrodes[i, j].Occupant.Name;
                    }
                    line2 += name;
                    used2 += name.Length;
                }else if (Electrodes[i,j].GetContaminants().Count != 0)
                {
                    line2 += Electrodes[i, j].GetContaminants()[0].Substring(0,1);
                    used2 += 1;
                }
                else
                {
                    string name;
                    
                    if (Electrodes[i, j].Name.Length > Squarewidth)
                    {
                        name = Electrodes[i, j].Name[..Squarewidth];
                    }
                    else
                    {
                        name = Electrodes[i, j].Name;
                    }
                    
                    
                    line2 += name;
                    used2 += name.Length;
                }
                line1 += new string(' ', Math.Max(Squarewidth - used1, 0)) + " | ";
                line2 += new string(' ', Math.Max(Squarewidth - used2,0)) + " | ";
                line3 += new string(' ', Squarewidth-used3) + " | ";
                i++;
            }
            string totalline = line1 + "\n" + line2 + "\n" + line3;

            return totalline;
        }
    }
}
