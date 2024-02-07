using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    internal class TileEntity
    {
        readonly string Name;
        readonly int ID;
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Status { get; set; }

        public int[] IDs = [];

        public TileEntity(int x, int y, string name = "")
        {
            GetIDs();
            GenerateID(IDs);
            Name = name;
            PositionX = x;
            PositionY = y;
            Status = 0;
        }

        protected virtual void GetIDs()
        {
            IDs = IDs.Append(ID).ToArray();
        }

        protected virtual void GenerateID(params int[] values)
        {
            values[0] = 0; //ID
        }
    }
}
