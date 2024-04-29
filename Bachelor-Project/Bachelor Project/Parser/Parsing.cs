using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Electrode_Types.Actuator_Types;
using Bachelor_Project.Utility;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Parsing
{
    static public class Parsing
    {
        static Dictionary<string, string> Dropletpairs = []; // Dropletname with their type
        static Dictionary<string, List<string>> Contaminates = [];
        static Dictionary<string, List<string>> Contaminated = [];
        static List<Command> Commands = []; 

        public static (List<Command>, Dictionary<string,string>, Dictionary<string, List<string>>,Dictionary<string, List<string>>) ParseFile(string path)
        {
            string data = File.ReadAllText(path);

            return ParseString(data);

        }

        public static (List<Command>, Dictionary<string, string>, Dictionary<string, List<string>>, Dictionary<string, List<string>>) ParseString(string data)
        {
            Dropletpairs = [];
            Contaminates = [];
            Contaminated = [];
            Commands = [];
            ICharStream stream = CharStreams.fromString(data);
            ITokenSource lexer = new ProgramLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            ProgramParser parser = new ProgramParser(tokens);
            parser.AddErrorListener(new ErrorListener());
            IParseTree tree = parser.program();
            ProgramDecoder decoder = new ProgramDecoder();
            ParseTreeWalker.Default.Walk(decoder, tree);

            

            return (Commands, Dropletpairs, Contaminated, Contaminates);
        }

        public static void Decode(ProgramParser.CommandContext context)
        {
            string output;
            int i;
            string start = context.GetChild(0).GetType().Name;
            switch (start)
            {
                case "InputContext": //INPUT , droplet name , droplet type , input name, input volume
                    Printer.PrintLine($"input droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} of type: {context.GetChild<ProgramParser.DroplettypeContext>(0).GetText()} at input: {context.GetChild<ProgramParser.TileentityContext>(0).GetText()} with volume: {context.GetChild<ProgramParser.NumberContext>(0).GetText()}");
                    try
                    {
                        Dropletpairs.Add(context.GetChild<ProgramParser.DropletnameContext>(0).GetText(), context.GetChild<ProgramParser.DroplettypeContext>(0).GetText());
                    }
                    catch (Exception)
                    {
                        Printer.PrintLine($"Droplet with name: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} already exists");
                    }
                    Commands.Add(new Command("input", [], [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], value:[context.GetChild<ProgramParser.TileentityContext>(0).GetText(), context.GetChild<ProgramParser.NumberContext>(0).GetText()]));
                    break;
                case "OutputContext": //OUTPUT , droplet name , output name
                    Printer.PrintLine($"output droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} at output: {context.GetChild<ProgramParser.TileentityContext>(0).GetText()}");
                    Commands.Add(new Command( "output", [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], [], nextName: context.GetChild<ProgramParser.TileentityContext>(0).GetText(), nextType: typeof(Output)));
                    break;
                case"WasteContext": //WASTE , droplet name
                    Printer.PrintLine($"waste droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()}");
                    Commands.Add(new Command("waste", [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], []));
                    break;
                case "ContaminateContext": //CONTAMINATE, droplet type, (droplet type N)*
                    string contaminator = context.GetChild<ProgramParser.DroplettypeContext>(0).GetText();
                    Printer.Print($"droplets of type: {contaminator} contaminates droplets of type: ");
                    List<string> contaminants = [];
                    i = 1;
                    while (context.GetChild<ProgramParser.DroplettypeContext>(i) != null)
                    {
                        Printer.Print(context.GetChild<ProgramParser.DroplettypeContext>(i).GetText() + " ");
                        contaminants.Add(context.GetChild<ProgramParser.DroplettypeContext>(i).GetText());
                        if (Contaminated.ContainsKey(context.GetChild<ProgramParser.DroplettypeContext>(i).GetText()))
                        {
                            Contaminated[context.GetChild<ProgramParser.DroplettypeContext>(i).GetText()].Add(contaminator);
                        }
                        else
                        {
                            Contaminated.Add(context.GetChild<ProgramParser.DroplettypeContext>(i).GetText(), [contaminator]);
                        }

                        i++;
                    }
                    Printer.PrintLine();
                    if (!Contaminated.ContainsKey(contaminator))
                    {
                        Contaminated.Add(contaminator, []);
                    }
                    Contaminates.Add(contaminator, contaminants);
                    break;
                case "MergeContext": //MERGE , new droplet name , new droplet type , droplet1 name , droplet2 name , (dropletN name)?
                    List<string> mergers = [];
                    output = $"merge to make droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} of type: {context.GetChild<ProgramParser.DroplettypeContext>(0).GetText()} by merging: ";
                    try
                    {
                        Dropletpairs.Add(context.GetChild<ProgramParser.DropletnameContext>(0).GetText(), context.GetChild<ProgramParser.DroplettypeContext>(0).GetText());
                    }
                    catch (Exception)
                    {
                        Printer.Print($"Droplet with name: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} already exists");
                    }
                    i = 1;
                    while (context.GetChild<ProgramParser.DropletnameContext>(i) != null)
                    {
                        output = string.Concat(output, context.GetChild<ProgramParser.DropletnameContext>(i).GetText() + " ").ToString();
                        mergers.Add(context.GetChild<ProgramParser.DropletnameContext>(i).GetText());
                        i++;
                    }
                    Printer.PrintLine(output);
                    Commands.Add(new Command("merge", mergers, [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], value: [context.GetChild<ProgramParser.DroplettypeContext>(0).GetText()]));
                    break;
                case "SplitContext": //SPLIT , olddroplet name , new droplet1 name, ratio1? , new droplet2 name, ratio2? , (new dropletN name, ratioN? )*
                    output = $"split droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} to make: ";
                    List<string> splits = [];
                    Dictionary<string, int> ratios = [];
                    i = 1;
                    int usingRatios = -1; //unassigned
                    while (context.GetChild<ProgramParser.DropletnameContext>(i) != null)
                    {
                        if (context.GetChild<ProgramParser.NumberContext>(i-1) != null)
                        {
                            if (usingRatios == 0)
                            {
                                throw new ArgumentException("Either use no ratios or all ratios");
                            }
                            usingRatios = 1; // using ratios
                            ratios.Add(context.GetChild<ProgramParser.DropletnameContext>(i).GetText(), int.Parse(context.GetChild<ProgramParser.NumberContext>(i-1).GetText()));
                        }
                        else if (usingRatios == 1)
                        {
                            throw new ArgumentException("Either use no ratios or all ratios");
                        }
                        else
                        {
                            usingRatios = 0; // not using ratios
                        }
                        output = string.Concat(output, context.GetChild<ProgramParser.DropletnameContext>(i).GetText() + " ").ToString();
                        splits.Add(context.GetChild<ProgramParser.DropletnameContext>(i).GetText());
                        try
                        {
                            Dropletpairs.Add(context.GetChild<ProgramParser.DropletnameContext>(i).GetText(), Dropletpairs[context.GetChild<ProgramParser.DropletnameContext>(0).GetText()]);
                        }
                        catch (Exception)
                        {
                            Printer.Print($"Droplet with name: {context.GetChild<ProgramParser.DropletnameContext>(i).GetText()} already exists");
                            throw;
                        }
                        i++;
                    }
                    Printer.PrintLine(output);
                    if (usingRatios == 1)
                    {
                        Commands.Add(new Command("split", [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], splits, value: ratios));
                    }
                    else
                    {
                        Commands.Add(new Command("split", [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], splits));
                    }
                    
                    break;
                case "MixContext": //MIX , droplet name , pattern , (new droplet type)?
                    Printer.Print($"mix droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} in the shape: {context.GetChild<ProgramParser.ShapeContext>(0).GetText()} pattern");
                    string newType;
                    if(context.GetChild<ProgramParser.DroplettypeContext>(0) != null)
                    {
                        newType = context.GetChild<ProgramParser.DroplettypeContext>(0).GetText();
                        Printer.Print($" with new type: {context.GetChild<ProgramParser.DroplettypeContext>(0).GetText()}");
                    }
                    else
                    {
                        newType = Dropletpairs[context.GetChild<ProgramParser.DropletnameContext>(0).GetText()];
                    }
                    Printer.PrintLine();
                    Commands.Add(new Command("mix", [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], value: [context.GetChild<ProgramParser.ShapeContext>(0).GetText(), newType]));
                    break;
                case "TempContext": //TEMP , droplet name , heater name , (new droplet type)?
                    Printer.Print($"heat droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} with heater: {context.GetChild<ProgramParser.TileentityContext>(0).GetText()}");
                    if (context.GetChild<ProgramParser.DroplettypeContext>(0) != null)
                    {
                        newType = context.GetChild<ProgramParser.DroplettypeContext>(0).GetText();
                        Printer.Print($" with new type: {context.GetChild<ProgramParser.DroplettypeContext>(0).GetText()}");
                    }
                    else
                    {
                        newType = Dropletpairs[context.GetChild<ProgramParser.DropletnameContext>(0).GetText()];
                    }
                    Printer.PrintLine();
                    Commands.Add(new Command("temp", [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], nextName: context.GetChild<ProgramParser.TileentityContext>(0).GetText(), nextType: typeof(Heater),  value: [newType, context.GetChild<ProgramParser.NumberContext>(0).GetText()]));
                    break;
                case "SenseContext": //SENSE , droplet name , sensor
                    Printer.PrintLine($"sense droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} with sensor: {context.GetChild<ProgramParser.TileentityContext>(0).GetText()}");
                    Commands.Add(new Command("sense", [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], nextName: context.GetChild<ProgramParser.TileentityContext>(0).GetText(), nextType: typeof(Sensor)));
                    break;
                case "WaitContext": //WAIT , droplet name , time
                    Printer.PrintLine($"droplet: {context.GetChild<ProgramParser.DropletnameContext>(0).GetText()} waits for {int.Parse(context.GetChild<ProgramParser.NumberContext>(0).GetText())} milliseconds");
                    Commands.Add(new Command("wait", [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], [context.GetChild<ProgramParser.DropletnameContext>(0).GetText()], value: [int.Parse(context.GetChild<ProgramParser.NumberContext>(0).GetText())]));
                    break;
                default:
                    break;
            }
        }


    }
}
