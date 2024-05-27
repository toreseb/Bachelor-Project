//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Program.g4 by ANTLR 4.13.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="ProgramParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
[System.CLSCompliant(false)]
public interface IProgramListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterProgram([NotNull] ProgramParser.ProgramContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitProgram([NotNull] ProgramParser.ProgramContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.dropletname"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDropletname([NotNull] ProgramParser.DropletnameContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.dropletname"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDropletname([NotNull] ProgramParser.DropletnameContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.droplettype"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDroplettype([NotNull] ProgramParser.DroplettypeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.droplettype"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDroplettype([NotNull] ProgramParser.DroplettypeContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.number"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNumber([NotNull] ProgramParser.NumberContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.number"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNumber([NotNull] ProgramParser.NumberContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.shape"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterShape([NotNull] ProgramParser.ShapeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.shape"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitShape([NotNull] ProgramParser.ShapeContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.tileentity"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTileentity([NotNull] ProgramParser.TileentityContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.tileentity"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTileentity([NotNull] ProgramParser.TileentityContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCommand([NotNull] ProgramParser.CommandContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCommand([NotNull] ProgramParser.CommandContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.input"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterInput([NotNull] ProgramParser.InputContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.input"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitInput([NotNull] ProgramParser.InputContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.output"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterOutput([NotNull] ProgramParser.OutputContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.output"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitOutput([NotNull] ProgramParser.OutputContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.waste"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterWaste([NotNull] ProgramParser.WasteContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.waste"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitWaste([NotNull] ProgramParser.WasteContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.contaminate"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterContaminate([NotNull] ProgramParser.ContaminateContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.contaminate"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitContaminate([NotNull] ProgramParser.ContaminateContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.merge"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMerge([NotNull] ProgramParser.MergeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.merge"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMerge([NotNull] ProgramParser.MergeContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.split"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSplit([NotNull] ProgramParser.SplitContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.split"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSplit([NotNull] ProgramParser.SplitContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.mix"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMix([NotNull] ProgramParser.MixContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.mix"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMix([NotNull] ProgramParser.MixContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.temp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTemp([NotNull] ProgramParser.TempContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.temp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTemp([NotNull] ProgramParser.TempContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.sense"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSense([NotNull] ProgramParser.SenseContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.sense"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSense([NotNull] ProgramParser.SenseContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ProgramParser.wait"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterWait([NotNull] ProgramParser.WaitContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ProgramParser.wait"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitWait([NotNull] ProgramParser.WaitContext context);
}
class ProgramDecoder : ProgramBaseListener
{
    public override void ExitCommand([NotNull] ProgramParser.CommandContext context)
    {
        Bachelor_Project.Parsing.Parsing.Decode(context);
        base.ExitCommand(context);
    }
}
class ErrorListener : BaseErrorListener
{

    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        throw new ArgumentException("Syntax error at line " + line + ":" + charPositionInLine + " " + msg);
    }
}