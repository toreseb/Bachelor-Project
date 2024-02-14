using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    internal class Board
    {
        private int width;
        private int height;
        private Electrode[,] Electrodes;
        private Droplet[] droplets;

        public Board(int width, int height)
        {
            this.width = width;
            this.height = height;
            Electrodes = new Electrode[width, height];
            InitElectrodes(Electrodes);
            droplets = [];
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
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public void PrintBoardState()
        {
            // Write horizontal lines one by one
            for (int i = 0; i < height; i++)
            {
                Console.WriteLine(BuildPrintLine(i));
            }
        }

        public string BuildPrintLine(int h)
        {
            String line = string.Empty;

            // Check each electrode in line and add to string - O (empty), Z (contaminated), X (droplet)
            for (int i = 0; i < width; i++)
            {
                if (Electrodes[i, h].GetStatus() == 0)
                {
                    String cont = Electrodes[i, h].GetContaminants;
                    if (cont.Equals(string.Empty))
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
        }
    }
}
