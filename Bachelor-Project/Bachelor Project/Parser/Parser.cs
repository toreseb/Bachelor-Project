using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Parser
{
    static class Parser
    {
        static List<String> Dropletnames = [];
        static List<String> Droplettypes = [];
        static ArrayList[] Commands = []; 

        public static (ArrayList[], List<String>, List<String>) ParseFile(string path)
        {
            String data = File.ReadAllText(path);
            ICharStream stream = CharStreams.fromString(data);
            ITokenSource lexer = new ProgramLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            ProgramParser parser = new ProgramParser(tokens);
            IParseTree tree = parser.program();
            ProgramDecoder decoder = new ProgramDecoder();
            ParseTreeWalker.Default.Walk(decoder, tree);

            return (Commands, Dropletnames, Droplettypes);

        }

        public static void Decode(ProgramParser.CommandContext context)
        {
            string output;
            int i;
            ArrayList Command = [];
            switch (context.GetChild(0).GetText())
            {
                case "input": //INPUT , droplet name , droplet type , input name
                    Console.WriteLine($"input droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} of type: {context.GetChild<ProgramParser.DroplettypeContext>(0).GetText()} at input: {context.GetChild<ProgramParser.InputContext>(0).GetText()}");
                    Commands = [..Commands, new ArrayList() { "input", context.GetChild<ProgramParser.DropletnameContext>(0).GetText(), context.GetChild<ProgramParser.DroplettypeContext>(0).GetText(), context.GetChild<ProgramParser.InputContext>(0).GetText() }];
                    break;
                case "output": //OUTPUT , droplet name , output name
                    Console.WriteLine($"output droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} at output: {context.GetChild<ProgramParser.OutputContext>(0).GetText()}");
                    Commands = [..Commands, new ArrayList() { "output", context.GetChild<ProgramParser.DropletnameContext>(0).GetText(), context.GetChild<ProgramParser.OutputContext>(0).GetText() }];
                    break;
                case "waste": //WASTE , droplet name
                    Console.WriteLine($"waste droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()}");
                    Commands = [..Commands, new ArrayList() { "waste", context.GetChild<ProgramParser.DropletnameContext>(0).GetText() }];
                    break;
                case "merge": //MERGE , new droplet name , new droplet type , droplet1 name , droplet2 name , (dropletN name)?
                    Command.Add("merge");
                    Command.Add(context.GetChild<ProgramParser.DropletnameContext>(0).GetText());
                    Command.Add(context.GetChild<ProgramParser.DroplettypeContext>(0).GetText());
                    output = $"merge to make droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} of type: {context.GetChild<ProgramParser.DroplettypeContext>(0).GetText()} by merging: ";
                    i = 1;
                    while (context.GetChild<ProgramParser.DropletnameContext>(i) != null)
                    {
                        output = string.Concat(output, context.GetChild<ProgramParser.DropletnameContext>(i).GetText() + " ").ToString();
                        Command.Add(context.GetChild<ProgramParser.DropletnameContext>(i).GetText());
                        i++;
                    }
                    Console.WriteLine(output);
                    Commands = [..Commands, new ArrayList(Command)];
                    Command.Clear();
                    break;
                case "split": //SPLIT , droplet name , new droplet1 name , new droplet2 name , (new dropletN name)?
                    Command.Add("split");
                    Command.Add(context.GetChild<ProgramParser.DropletnameContext>(0).GetText());
                    output = $"split droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} to make: ";
                    i = 1;
                    while (context.GetChild<ProgramParser.DropletnameContext>(i) != null)
                    {
                        output = string.Concat(output, context.GetChild<ProgramParser.DropletnameContext>(i).GetText() + " ").ToString();
                        Command.Add(context.GetChild<ProgramParser.DropletnameContext>(i).GetText());
                        i++;
                    }
                    Console.WriteLine(output);
                    Commands = [..Commands, new ArrayList(Command)];
                    Command.Clear();
                    break;
                case "mix": //MIX , droplet name, (new droplet type)? , pattern
                    Command.Add("mix");
                    Command.Add(context.GetChild<ProgramParser.DropletnameContext>(0).GetText());
                    Console.Write($"mix droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} in the shape: {context.GetChild<ProgramParser.ShapeContext>(0).GetText()} pattern");
                    if(context.GetChild<ProgramParser.DroplettypeContext>(0) != null)
                    {
                        Console.Write($" with new type: {context.GetChild<ProgramParser.DroplettypeContext>(0).GetText()}");
                        Command.Add(context.GetChild<ProgramParser.DroplettypeContext>(0).GetText());
                    }
                    Console.WriteLine();
                    Commands = [..Commands, Command];
                    Command.Clear();
                    break;
                case "temp": //TEMP , droplet name , (new droplet type)? , temperature
                    Command.Add("temp");
                    Command.Add(context.GetChild<ProgramParser.DropletnameContext>(0).GetText());
                    Console.Write($"set temperature of droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} to the temperature: {context.GetChild<ProgramParser.NumberContext>(0).INT()}");
                    if (context.GetChild<ProgramParser.DroplettypeContext>(0) != null)
                    {
                        Console.Write($" with new type: {context.GetChild<ProgramParser.DroplettypeContext>(0).GetText()}");
                        Command.Add(context.GetChild<ProgramParser.DroplettypeContext>(0).GetText());
                    }
                    Console.WriteLine();
                    Commands = [..Commands, Command];
                    Command.Clear();
                    break;
                case "sense": //SENSE , droplet name , sensor
                    Console.WriteLine($"sense droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} with sensor: {context.GetChild<ProgramParser.SensorContext>(0).GetText()} sensor");
                    Commands = [..Commands, new ArrayList() { "sense", context.GetChild<ProgramParser.DropletnameContext>(0).GetText(), context.GetChild<ProgramParser.SensorContext>(0).GetText() }];
                    break;
                default:
                    break;
            }
        }

        public static void AddName(String name)
        {
            if (!Dropletnames.Contains(name))
            {
                Dropletnames.Add(name);
            }
        }

        public static void AddType(String type)
        {
            if (!Droplettypes.Contains(type))
            {
                Droplettypes.Add(type);
            }
        }

    }
}
