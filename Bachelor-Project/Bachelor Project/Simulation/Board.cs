using Bachelor_Project.Electrode_Types;
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
        public Electrode[] Electrodes   { get; set; }     
        public Actuator[] Actuators     { get; set; }
        public Sensor[] Sensors         { get; set; }
        public Input[] Inputs           { get; set; }
        public Output[] Outputs         { get; set; }
        public Droplet[] Droplets       { get; set; }
        public Information[] Information{ get; set; }
        public String?[] unclassified   { get; set; }
        



        private void InitElectrodes(Electrode[] electrodes)
        {

        }

        public void SetElectrodes(Electrode[] electrodes)
        {
            this.Electrodes = electrodes;
        }

        public Electrode[] GetElectrodes()
        {
            return Electrodes;
        }

        public int GetWidth()
        {
            return Information[0].sizeX;
        }

        public int GetHeight()
        {
            return Information[0].sizeY;
        }

        public void PrintBoardState()
        {
            // Write horizontal lines one by one
            for (int i = 0; i < Information[0].sizeY/20; i++)
            {
                Console.WriteLine(BuildPrintLine(i));
            }
        }

        public string BuildPrintLine(int h)
        {
            String line = string.Empty;

            // Check each electrode in line and add to string - O (empty), Z (contaminated), X (droplet)
            for (int i = 0; i < Information[0].sizeX/20; i++)
            {
                if (Electrodes[i+ h* Information[0].sizeX / 20].GetStatus() == 0)
                {
                    String[] cont = Electrodes[i+ h* Information[0].sizeX / 20].GetContaminants();
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
