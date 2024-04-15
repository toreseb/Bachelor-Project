using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Utility
{
    public static class Calc
    {
        public static Dictionary<string, double> Ratio(Dictionary<string, int>? ratios, List<string> OutputDroplets)
        {
            Dictionary<string, double> result = [];
            if (ratios == null)
            {
                foreach (string d in OutputDroplets)
                {
                    result.Add(d, 100 / OutputDroplets.Count);
                }
            }
            else
            {
                double sum = 0;

                foreach (string d in OutputDroplets)
                {
                    sum += ratios[d];
                }

                foreach (string d in OutputDroplets)
                {
                    result.Add(d, ratios[d] / sum * 100);
                }
            }

            return result;
        }
    }
}
