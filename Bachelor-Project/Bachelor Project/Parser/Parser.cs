using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Parser
{
    static class Parser
    {
        static TextFieldParser parser;

        public static void ParseFile(string path)
        {
            parser = new TextFieldParser(path);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                string[] usefields = [];
                foreach (string field in fields)
                {
                    usefields = [.. usefields, field.Trim().ToLower()];
                }
                
                Decode(usefields);
            }
        }

        private static void Decode(string[] fields)
        {
            switch (fields[0])
            {
                case "input": //input , droplet name
                    Console.WriteLine($"input droplet name: {fields[1]}");
                    break;
                case "output": //output , droplet name
                    Console.WriteLine($"output droplet name: {fields[1]}");
                    break;
                case "waste": //waste , droplet name
                    Console.WriteLine($"waste droplet name: {fields[1]}");
                    break;
                
                case "merge": //merge , droplet1 , droplet2 , new droplet name
                    Console.WriteLine($"merge droplet {fields[1]} with {fields[2]} making droplet with name: {fields[3]}");
                    break;
                case "multi-merge": //multi-merge , droplets , new droplet name
                    int amount = fields.Length - 1;
                    string output = "merge droplets ";
                    foreach (string field in fields[1..(amount)])
                    {
                        output = string.Concat(output,field+", ").ToString();
                    }
                    output = string.Concat(output, "making droplet with name: " + fields[amount]).ToString();
                    Console.WriteLine(output.ToString());
                    break;
                case "split": //split , droplet name , new droplet1 name , new droplet2 name
                    Console.WriteLine($"split droplet {fields[1]} into droplets {fields[2]} and {fields[3]}");
                    break;
                case "multi-split": //multi-split , droplet name , droplets
                    amount = fields.Length;
                    output = "split droplet " + fields[1] + " into droplets ";
                    foreach (string field in fields[2..(amount)])
                    {
                        output = string.Concat(output,field+", ").ToString();
                    }
                    Console.WriteLine(output.ToString());
                    break;
                case "mix": //mix , droplet , pattern
                    Console.WriteLine($"mix droplet {fields[1]} in {fields[2]} pattern");
                    break;
                case "temp": //temp , droplet , temperature
                    Console.WriteLine($"set temperature of droplet {fields[1]} to {fields[2]}");
                    break;
                case "sense": //sense , droplet , sensor
                    Console.WriteLine($"sense droplet {fields[1]} with {fields[2]} sensor");
                    break;
                default:
                    break;
            }
        }

    }
}
