using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    /// <summary>
    /// The superclass for all objects that are placed on the <see cref="Board"/>.
    /// </summary>
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
 
        /// <summary>
        /// Finds the center of the <see cref="TileEntity"/>.
        /// </summary>
        /// <returns>The center</returns>
        public (int,int) GetCenter()
        {
            return (PositionX + SizeX / 2, PositionY + SizeY / 2);
        }

        /// <summary>
        /// Getter for <see cref="Name"/>.
        /// </summary>
        /// <returns></returns>
        public string getName()
        {
            return Name;
        }


    }
}
