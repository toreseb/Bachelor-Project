using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation.Agent_Actions
{
    public enum Direction
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }
    public static class DirectionUtils
    {
        public static Direction GetOppositeDirection(Direction dir)
        {
            return dir + 2 % 4;
        }
    }

}
