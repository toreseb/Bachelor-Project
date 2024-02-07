// See https://aka.ms/new-console-template for more information

using Bachelor_Project.Simulation;
using System.Collections;

class Program
{
    static void Main(string[] args)
    {
        Electrode e = new(1,2);
        Console.WriteLine($"x: {e.X} y: {e.Y}");

    }
}

