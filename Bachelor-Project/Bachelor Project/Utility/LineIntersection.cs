using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Utility
{
    /// <summary>
    /// Taken from https://gamedev.stackexchange.com/questions/26004/how-to-detect-2d-line-on-line-collision
    /// 
    /// <para>There is an issue where in some specific cases it returns true, when it might not be necesarry. If this is shown to be a big problem, a larger more complicated solution that fixes that problem is also refrences in the link</para> 
    /// </summary>
    static class LineIntersection
    {
        public static bool IsIntersecting(Point a, Point b, Point c, Point d)
        {
            float denominator = ((b.X - a.X) * (d.Y - c.Y)) - ((b.Y - a.Y) * (d.X - c.X));
            float numerator1 = ((a.Y - c.Y) * (d.X - c.X)) - ((a.X - c.X) * (d.Y - c.Y));
            float numerator2 = ((a.Y - c.Y) * (b.X - a.X)) - ((a.X - c.X) * (b.Y - a.Y));

            // Detect coincident lines (has a problem, read below)
            if (denominator == 0) return numerator1 == 0 && numerator2 == 0;

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
        }
    }
}
