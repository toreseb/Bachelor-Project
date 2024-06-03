using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Utility
{
    /// <summary>
    /// Calculates the percentages for splitdroplet
    /// </summary>
    public static class Calc
    {
        /// <summary>
        /// Finds the percentages for each <see cref="Droplet"/> in <paramref name="OutputDroplets"/> base of their <paramref name="ratio"/>.
        /// </summary>
        /// <param name="ratio"></param>
        /// <param name="OutputDroplets"></param>
        /// <returns>A total <see cref="Dictionary{TKey, TValue}"/> of all the <paramref name="OutputDroplets"/> percentages.</returns>
        public static Dictionary<string, double> FindPercentages(Dictionary<string, int>? ratio, List<string> OutputDroplets)
        {
            Dictionary<string, double> result = [];
            if (ratio == null)
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
                    sum += ratio[d];
                }

                foreach (string d in OutputDroplets)
                {
                    result.Add(d, ratio[d] / sum * 100);
                }
            }

            return result;
        }
    }
}
