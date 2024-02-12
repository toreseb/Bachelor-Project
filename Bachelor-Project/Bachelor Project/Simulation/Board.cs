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
    }
}
