// See https://aka.ms/new-console-template for more information

using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Simulation;
using System.Collections;

class Program
{
    static void Main(string[] args)
    {
        Sensor e = new(1,2);
        foreach (int item in e.IDs)
        {
            Console.WriteLine(item);
        }

    }
}

