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
        private Electrode[,] board;
        private Droplet[] droplets;

        public Board(int width, int height)
        {
            this.width = width;
            this.height = height;
            board = new Electrode[width, height];
            droplets = [];
        }

        public void SetBoard(Electrode[,] board)
        {
            this.board = board;
        }

        public Electrode[,] GetBoard()
        {
            return board;
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
