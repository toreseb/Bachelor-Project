using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Bachelor_Project.Utility;
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
        static Dictionary<String,String> Dropletpairs = []; // Dropletname with their type
        static Dictionary<String, List<String>> Contaminates = [];
        static Dictionary<String, List<String>> Contaminated = [];
        static List<Command> Commands = []; 

        public static (List<Command>, Dictionary<String,String>, Dictionary<String,List<String>>,Dictionary<String,List<String>>) ParseFile(string path)
        {
            String data = File.ReadAllText(path);
            ICharStream stream = CharStreams.fromString(data);
            ITokenSource lexer = new ProgramLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            ProgramParser parser = new ProgramParser(tokens);
            IParseTree tree = parser.program();
            ProgramDecoder decoder = new ProgramDecoder();
            ParseTreeWalker.Default.Walk(decoder, tree);

            return (Commands, Dropletpairs, Contaminated, Contaminates);

        }

        public static void Decode(ProgramParser.CommandContext context)
        {
            string output;
            int i;
            switch (context.GetChild(0).GetText())
            {
                case "input": //INPUT , droplet name , droplet type , input name, input volume
                    Console.WriteLine($"input droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} of type: {context.GetChild<ProgramParser.DroplettypeContext>(0).GetText()} at input: {context.GetChild<ProgramParser.InputContext>(0).GetText()} with volume: {context.GetChild<ProgramParser.NumberContext>(0).GetText()}");
                    try
                    {
                        Dropletpairs.Add(context.GetChild<ProgramParser.DropletnameContext>(0).GetText(), context.GetChild<ProgramParser.DroplettypeContext>(0).GetText());
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Droplet with name: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} already exists");
                    }
                    Commands.Add(new Command("input", [], [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], context.GetChild<ProgramParser.InputContext>(0).GetText(), context.GetChild<ProgramParser.NumberContext>(0).GetText()));
                    break;
                case "output": //OUTPUT , droplet name , output name
                    Console.WriteLine($"output droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} at output: {context.GetChild<ProgramParser.OutputContext>(0).GetText()}");
                    Commands.Add(new Command( "output", [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], [], context.GetChild<ProgramParser.OutputContext>(0).GetText()));
                    break;
                case "waste": //WASTE , droplet name
                    Console.WriteLine($"waste droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()}");
                    Commands.Add(new Command("waste", [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], []));
                    break;
                case "contam": //CONTAMINATE, droplet type, (droplet type N)*
                    string contaminee = context.GetChild<ProgramParser.DroplettypeContext>(0).GetText();
                    Console.Write($"droplets of type: {contaminee} is contaminated by droplets of type: ");
                    List<String> contaminants = [];
                    i = 1;
                    while (context.GetChild<ProgramParser.DroplettypeContext>(i) != null)
                    {
                        Console.Write(context.GetChild<ProgramParser.DroplettypeContext>(i).GetText() + " ");
                        contaminants.Add(context.GetChild<ProgramParser.DroplettypeContext>(i).GetText());
                        if (Contaminates.ContainsKey(context.GetChild<ProgramParser.DroplettypeContext>(i).GetText()))
                        {
                            Contaminates[context.GetChild<ProgramParser.DroplettypeContext>(i).GetText()].Add(contaminee);
                        }
                        else
                        {
                            Contaminates.Add(context.GetChild<ProgramParser.DroplettypeContext>(i).GetText(), [contaminee]);
                        }

                        i++;
                    }
                    Console.WriteLine();
                    if (!Contaminates.ContainsKey(contaminee))
                    {
                        Contaminates.Add(contaminee, []);
                    }
                    Contaminated.Add(contaminee, contaminants);
                    break;
                case "merge": //MERGE , new droplet name , new droplet type , droplet1 name , droplet2 name , (dropletN name)?
                    List<string> mergers = [];
                    output = $"merge to make droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} of type: {context.GetChild<ProgramParser.DroplettypeContext>(0).GetText()} by merging: ";
                    try
                    {
                        Dropletpairs.Add(context.GetChild<ProgramParser.DropletnameContext>(0).GetText(), context.GetChild<ProgramParser.DroplettypeContext>(0).GetText());
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Droplet with name: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} already exists");
                    }
                    i = 1;
                    while (context.GetChild<ProgramParser.DropletnameContext>(i) != null)
                    {
                        output = string.Concat(output, context.GetChild<ProgramParser.DropletnameContext>(i).GetText() + " ").ToString();
                        mergers.Add(context.GetChild<ProgramParser.DropletnameContext>(i).GetText());
                        i++;
                    }
                    Console.WriteLine(output);
                    Commands.Add(new Command("merge", mergers, [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()]));
                    break;
                case "split": //SPLIT , droplet name , new droplet1 name , new droplet2 name , (new dropletN name)?
                    output = $"split droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} to make: ";
                    List<string> splits = [];
                    i = 1;
                    while (context.GetChild<ProgramParser.DropletnameContext>(i) != null)
                    {
                        output = string.Concat(output, context.GetChild<ProgramParser.DropletnameContext>(i).GetText() + " ").ToString();
                        splits.Add(context.GetChild<ProgramParser.DropletnameContext>(i).GetText());
                        try
                        {
                            Dropletpairs.Add(context.GetChild<ProgramParser.DropletnameContext>(i).GetText(), Dropletpairs[context.GetChild<ProgramParser.DropletnameContext>(0).GetText()]);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"Droplet with name: {context.GetChild<ProgramParser.DropletnameContext>(i).GetText()} already exists");
                            throw;
                        }
                        i++;
                    }
                    Console.WriteLine(output);
                    Commands.Add(new Command("split", [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], splits));
                    break;
                case "mix": //MIX , droplet name, (new droplet type)? , pattern
                    Console.Write($"mix droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} in the shape: {context.GetChild<ProgramParser.ShapeContext>(0).GetText()} pattern");
                    string newType;
                    if(context.GetChild<ProgramParser.DroplettypeContext>(0) != null)
                    {
                        newType = context.GetChild<ProgramParser.DroplettypeContext>(0).GetText();
                        Console.Write($" with new type: {context.GetChild<ProgramParser.DroplettypeContext>(0).GetText()}");
                    }
                    else
                    {
                        newType = Dropletpairs[context.GetChild<ProgramParser.DropletnameContext>(0).GetText()];
                    }
                    Console.WriteLine();
                    Commands.Add(new Command("mix", [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], context.GetChild<ProgramParser.ShapeContext>(0).GetText(), newType));
                    break;
                case "temp": //TEMP , droplet name , (new droplet type)? , temperature
                    Console.Write($"set temperature of droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} to the temperature: {context.GetChild<ProgramParser.NumberContext>(0).INT()}");
                    if (context.GetChild<ProgramParser.DroplettypeContext>(0) != null)
                    {
                        newType = context.GetChild<ProgramParser.DroplettypeContext>(0).GetText();
                        Console.Write($" with new type: {context.GetChild<ProgramParser.DroplettypeContext>(0).GetText()}");
                    }
                    else
                    {
                        newType = Dropletpairs[context.GetChild<ProgramParser.DropletnameContext>(0).GetText()];
                    }
                    Console.WriteLine();
                    Commands.Add(new Command("temp", [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], context.GetChild<ProgramParser.NumberContext>(0).GetText(), newType));
                    break;
                case "sense": //SENSE , droplet name , sensor
                    Console.WriteLine($"sense droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} with sensor: {context.GetChild<ProgramParser.SensorContext>(0).GetText()} sensor");
                    Commands.Add(new Command("sense", [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], context.GetChild<ProgramParser.SensorContext>(0).GetText()));
                    break;
                default:
                    break;
            }
        }


    }
}
