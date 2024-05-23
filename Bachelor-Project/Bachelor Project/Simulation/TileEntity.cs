using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    abstract public class TileEntity
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }
        

        

        public int[] IDs = [];

        public TileEntity(int x, int y, int sizeX, int sizeY, string name = "")
        {
            Name = name;
            PositionX = x;
            PositionY = y;
            SizeX = sizeX;
            SizeY = sizeY;
        }
 
        public (int,int) GetCenter()
        {
            return (PositionX + SizeX / 2, PositionY + SizeY / 2);
        }

       public string getName() 
       {             
            return Name;
       }

    }
}
