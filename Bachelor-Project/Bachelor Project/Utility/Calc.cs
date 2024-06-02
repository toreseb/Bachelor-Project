using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Utility
{
    public static class Calc
    {
        public static Dictionary<string, double> FindPercentages(Dictionary<string, int>? Percentages, List<string> OutputDroplets)
        {
            Dictionary<string, double> result = [];
            if (Percentages == null)
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
                    sum += Percentages[d];
                }

                foreach (string d in OutputDroplets)
                {
                    result.Add(d, Percentages[d] / sum * 100);
                }
            }

            return result;
        }
    }
}
