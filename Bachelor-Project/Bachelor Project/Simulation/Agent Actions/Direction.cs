using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation.Agent_Actions
{
    /// <summary>
    /// A <see langword=""="enum"/> representing four directions.
    /// </summary>
    public enum Direction
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    /// <summary>
    /// A utility class containing functions relevent to <see cref="Direction"/>s
    /// </summary>
    public static class DirectionUtils
    {
        /// <summary>
        /// Finds the opposite <see cref="Direction"/> of <paramref name="dir"/>.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns>The opposide <see cref="Direction"/></returns>
        public static Direction GetOppositeDirection(Direction dir)
        {
            return dir + 2 % 4;
        }

        /// <summary>
        /// Returns the x and y change of the specific <see cref="Direction"/> <paramref name="dir"/>.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns>The x and y change</returns>
        /// <exception cref="Exception"></exception>
        public static (int x,int y) GetXY(Direction dir)
        {
            return dir switch
            {
                Direction.UP => (0, -1),
                Direction.RIGHT => (1, 0),
                Direction.DOWN => (0, 1),
                Direction.LEFT => (-1, 0),
                _ => throw new Exception("Unkown Direction"),
            };
        }

    }

}
