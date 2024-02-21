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
        static ArrayList[] Commands = []; 

        public static ArrayList[] ParseFile(string path)
        {
            String data = File.ReadAllText(path);
            ICharStream stream = CharStreams.fromString(data);
            ITokenSource lexer = new ProgramLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            ProgramParser parser = new ProgramParser(tokens);
            IParseTree tree = parser.program();
            ProgramPrinter printer = new ProgramPrinter();
            ParseTreeWalker.Default.Walk(printer, tree);

            return Commands;

        }

        public static void Decode(ProgramParser.CommandContext context)
        {
            string output;
            int i;
            ArrayList Command = [];
            switch (context.GetChild(0).GetText())
            {
                case "input": //input , droplet name
                    Console.WriteLine($"input droplet named: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()}");
                    Commands = [..Commands, new ArrayList() { "input", context.GetChild<ProgramParser.DropletnameContext>(0).GetText() }];
                    break;
                case "output": //output , droplet name
                    Console.WriteLine($"output droplet named: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()}");
                    Commands = [..Commands, new ArrayList() { "output", context.GetChild<ProgramParser.DropletnameContext>(0).GetText() }];
                    break;
                case "waste": //waste , droplet name
                    Console.WriteLine($"waste droplet named: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()}");
                    Commands = [..Commands, new ArrayList() { "waste", context.GetChild<ProgramParser.DropletnameContext>(0).GetText() }];
                    break;
                case "merge": //multi-merge , droplets , new droplet name
                    Command.Add("merge");
                    Command.Add(context.GetChild<ProgramParser.DropletnameContext>(0).GetText());
                    output = $"merge to make droplet {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} by merging ";
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
                case "split": //split , droplet name , new droplet1 name , new droplet2 name
                    Command.Add("split");
                    Command.Add(context.GetChild<ProgramParser.DropletnameContext>(0).GetText());
                    output = $"split droplet {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} to make ";
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
                case "mix": //mix , droplet , pattern
                    Console.WriteLine($"mix {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} in the {context.GetChild<ProgramParser.ShapeContext>(0).GetText()} pattern");
                    Commands = [..Commands, new ArrayList() { "mix", context.GetChild<ProgramParser.DropletnameContext>(0).GetText(), context.GetChild<ProgramParser.ShapeContext>(0).GetText() }];
                    break;
                case "temp": //temp , droplet , temperature
                    Console.WriteLine($"set temperature of {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} to {context.GetChild<ProgramParser.NumberContext>(0).INT()}");
                    Commands = [..Commands, new ArrayList() { "temp", context.GetChild<ProgramParser.DropletnameContext>(0).GetText(), context.GetChild<ProgramParser.NumberContext>(0).INT() }];
                    break;
                case "sense": //sense , droplet , sensor
                    Console.WriteLine($"sense {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} with {context.GetChild<ProgramParser.SensorContext>(0).GetText()} sensor");
                    Commands = [..Commands, new ArrayList() { "sense", context.GetChild<ProgramParser.DropletnameContext>(0).GetText(), context.GetChild<ProgramParser.SensorContext>(0).GetText() }];
                    break;
                default:
                    break;
            }
        }

    }
}
