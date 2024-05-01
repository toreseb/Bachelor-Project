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

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
[System.CLSCompliant(false)]
public partial class ProgramParser : Parser {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, T__10=11, T__11=12, T__12=13, T__13=14, T__14=15, T__15=16, T__16=17, 
		T__17=18, T__18=19, T__19=20, T__20=21, T__21=22, T__22=23, T__23=24, 
		T__24=25, T__25=26, T__26=27, T__27=28, T__28=29, T__29=30, T__30=31, 
		T__31=32, T__32=33, T__33=34, NEWLINE=35, STRING=36, INT=37;
	public const int
		RULE_program = 0, RULE_dropletname = 1, RULE_droplettype = 2, RULE_number = 3, 
		RULE_shape = 4, RULE_tileentity = 5, RULE_command = 6, RULE_input = 7, 
		RULE_output = 8, RULE_waste = 9, RULE_contaminate = 10, RULE_merge = 11, 
		RULE_split = 12, RULE_mix = 13, RULE_temp = 14, RULE_sense = 15, RULE_wait = 16;
	public static readonly string[] ruleNames = {
		"program", "dropletname", "droplettype", "number", "shape", "tileentity", 
		"command", "input", "output", "waste", "contaminate", "merge", "split", 
		"mix", "temp", "sense", "wait"
	};

	private static readonly string[] _LiteralNames = {
		null, "';'", "'square'", "'Square'", "'circle'", "'Circle'", "' '", "'input'", 
		"'in'", "'Input'", "'In'", "'output'", "'out'", "'Output'", "'Out'", "'waste'", 
		"'Waste'", "'contam'", "'contaminate'", "'Contam'", "'Contaminate'", "'merge'", 
		"'Merge'", "'split'", "'Split'", "'mix'", "'Mix'", "'temp'", "'heat'", 
		"'Temp'", "'Heat'", "'sense'", "'Sense'", "'wait'", "'Wait'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, "NEWLINE", 
		"STRING", "INT"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "Program.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static ProgramParser() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

		public ProgramParser(ITokenStream input) : this(input, Console.Out, Console.Error) { }

		public ProgramParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	public partial class ProgramContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public CommandContext[] command() {
			return GetRuleContexts<CommandContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public CommandContext command(int i) {
			return GetRuleContext<CommandContext>(i);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode[] NEWLINE() { return GetTokens(ProgramParser.NEWLINE); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode NEWLINE(int i) {
			return GetToken(ProgramParser.NEWLINE, i);
		}
		public ProgramContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_program; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterProgram(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitProgram(this);
		}
	}

	[RuleVersion(0)]
	public ProgramContext program() {
		ProgramContext _localctx = new ProgramContext(Context, State);
		EnterRule(_localctx, 0, RULE_program);
		int _la;
		try {
			int _alt;
			EnterOuterAlt(_localctx, 1);
			{
			State = 40;
			ErrorHandler.Sync(this);
			_alt = Interpreter.AdaptivePredict(TokenStream,0,Context);
			while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					State = 34;
					command();
					State = 35;
					Match(T__0);
					State = 36;
					Match(NEWLINE);
					}
					} 
				}
				State = 42;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,0,Context);
			}
			State = 43;
			command();
			State = 44;
			Match(T__0);
			State = 46;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			if (_la==NEWLINE) {
				{
				State = 45;
				Match(NEWLINE);
				}
			}

			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class DropletnameContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode STRING() { return GetToken(ProgramParser.STRING, 0); }
		public DropletnameContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_dropletname; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterDropletname(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitDropletname(this);
		}
	}

	[RuleVersion(0)]
	public DropletnameContext dropletname() {
		DropletnameContext _localctx = new DropletnameContext(Context, State);
		EnterRule(_localctx, 2, RULE_dropletname);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 48;
			Match(STRING);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class DroplettypeContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode STRING() { return GetToken(ProgramParser.STRING, 0); }
		public DroplettypeContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_droplettype; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterDroplettype(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitDroplettype(this);
		}
	}

	[RuleVersion(0)]
	public DroplettypeContext droplettype() {
		DroplettypeContext _localctx = new DroplettypeContext(Context, State);
		EnterRule(_localctx, 4, RULE_droplettype);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 50;
			Match(STRING);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class NumberContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode INT() { return GetToken(ProgramParser.INT, 0); }
		public NumberContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_number; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterNumber(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitNumber(this);
		}
	}

	[RuleVersion(0)]
	public NumberContext number() {
		NumberContext _localctx = new NumberContext(Context, State);
		EnterRule(_localctx, 6, RULE_number);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 52;
			Match(INT);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ShapeContext : ParserRuleContext {
		public ShapeContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_shape; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterShape(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitShape(this);
		}
	}

	[RuleVersion(0)]
	public ShapeContext shape() {
		ShapeContext _localctx = new ShapeContext(Context, State);
		EnterRule(_localctx, 8, RULE_shape);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 54;
			_la = TokenStream.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 60L) != 0)) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
				ErrorHandler.ReportMatch(this);
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class TileentityContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode STRING() { return GetToken(ProgramParser.STRING, 0); }
		public TileentityContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_tileentity; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterTileentity(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitTileentity(this);
		}
	}

	[RuleVersion(0)]
	public TileentityContext tileentity() {
		TileentityContext _localctx = new TileentityContext(Context, State);
		EnterRule(_localctx, 10, RULE_tileentity);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 56;
			Match(STRING);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class CommandContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public InputContext input() {
			return GetRuleContext<InputContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public DropletnameContext[] dropletname() {
			return GetRuleContexts<DropletnameContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public DropletnameContext dropletname(int i) {
			return GetRuleContext<DropletnameContext>(i);
		}
		[System.Diagnostics.DebuggerNonUserCode] public DroplettypeContext[] droplettype() {
			return GetRuleContexts<DroplettypeContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public DroplettypeContext droplettype(int i) {
			return GetRuleContext<DroplettypeContext>(i);
		}
		[System.Diagnostics.DebuggerNonUserCode] public TileentityContext tileentity() {
			return GetRuleContext<TileentityContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public NumberContext[] number() {
			return GetRuleContexts<NumberContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public NumberContext number(int i) {
			return GetRuleContext<NumberContext>(i);
		}
		[System.Diagnostics.DebuggerNonUserCode] public OutputContext output() {
			return GetRuleContext<OutputContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public WasteContext waste() {
			return GetRuleContext<WasteContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ContaminateContext contaminate() {
			return GetRuleContext<ContaminateContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public MergeContext merge() {
			return GetRuleContext<MergeContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public SplitContext split() {
			return GetRuleContext<SplitContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public MixContext mix() {
			return GetRuleContext<MixContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ShapeContext shape() {
			return GetRuleContext<ShapeContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public TempContext temp() {
			return GetRuleContext<TempContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public SenseContext sense() {
			return GetRuleContext<SenseContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public WaitContext wait() {
			return GetRuleContext<WaitContext>(0);
		}
		public CommandContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_command; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterCommand(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitCommand(this);
		}
	}

	[RuleVersion(0)]
	public CommandContext command() {
		CommandContext _localctx = new CommandContext(Context, State);
		EnterRule(_localctx, 12, RULE_command);
		int _la;
		try {
			State = 162;
			ErrorHandler.Sync(this);
			switch (TokenStream.LA(1)) {
			case T__6:
			case T__7:
			case T__8:
			case T__9:
				EnterOuterAlt(_localctx, 1);
				{
				State = 58;
				input();
				State = 59;
				Match(T__5);
				State = 60;
				dropletname();
				State = 61;
				Match(T__5);
				State = 62;
				droplettype();
				State = 63;
				Match(T__5);
				State = 64;
				tileentity();
				State = 65;
				Match(T__5);
				State = 66;
				number();
				}
				break;
			case T__10:
			case T__11:
			case T__12:
			case T__13:
				EnterOuterAlt(_localctx, 2);
				{
				State = 68;
				output();
				State = 69;
				Match(T__5);
				State = 70;
				dropletname();
				State = 71;
				Match(T__5);
				State = 72;
				tileentity();
				}
				break;
			case T__14:
			case T__15:
				EnterOuterAlt(_localctx, 3);
				{
				State = 74;
				waste();
				State = 75;
				Match(T__5);
				State = 76;
				dropletname();
				}
				break;
			case T__16:
			case T__17:
			case T__18:
			case T__19:
				EnterOuterAlt(_localctx, 4);
				{
				State = 78;
				contaminate();
				State = 79;
				Match(T__5);
				State = 80;
				droplettype();
				State = 85;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
				while (_la==T__5) {
					{
					{
					State = 81;
					Match(T__5);
					State = 82;
					droplettype();
					}
					}
					State = 87;
					ErrorHandler.Sync(this);
					_la = TokenStream.LA(1);
				}
				}
				break;
			case T__20:
			case T__21:
				EnterOuterAlt(_localctx, 5);
				{
				State = 88;
				merge();
				State = 89;
				Match(T__5);
				State = 90;
				dropletname();
				State = 91;
				Match(T__5);
				State = 92;
				droplettype();
				State = 93;
				Match(T__5);
				State = 94;
				dropletname();
				State = 95;
				Match(T__5);
				State = 96;
				dropletname();
				State = 101;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
				while (_la==T__5) {
					{
					{
					State = 97;
					Match(T__5);
					State = 98;
					dropletname();
					}
					}
					State = 103;
					ErrorHandler.Sync(this);
					_la = TokenStream.LA(1);
				}
				}
				break;
			case T__22:
			case T__23:
				EnterOuterAlt(_localctx, 6);
				{
				State = 104;
				split();
				State = 105;
				Match(T__5);
				State = 106;
				dropletname();
				State = 107;
				Match(T__5);
				State = 108;
				dropletname();
				State = 111;
				ErrorHandler.Sync(this);
				switch ( Interpreter.AdaptivePredict(TokenStream,4,Context) ) {
				case 1:
					{
					State = 109;
					Match(T__5);
					State = 110;
					number();
					}
					break;
				}
				State = 113;
				Match(T__5);
				State = 114;
				dropletname();
				State = 117;
				ErrorHandler.Sync(this);
				switch ( Interpreter.AdaptivePredict(TokenStream,5,Context) ) {
				case 1:
					{
					State = 115;
					Match(T__5);
					State = 116;
					number();
					}
					break;
				}
				State = 127;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
				while (_la==T__5) {
					{
					{
					State = 119;
					Match(T__5);
					State = 120;
					dropletname();
					State = 123;
					ErrorHandler.Sync(this);
					switch ( Interpreter.AdaptivePredict(TokenStream,6,Context) ) {
					case 1:
						{
						State = 121;
						Match(T__5);
						State = 122;
						number();
						}
						break;
					}
					}
					}
					State = 129;
					ErrorHandler.Sync(this);
					_la = TokenStream.LA(1);
				}
				}
				break;
			case T__24:
			case T__25:
				EnterOuterAlt(_localctx, 7);
				{
				State = 130;
				mix();
				State = 131;
				Match(T__5);
				State = 132;
				dropletname();
				State = 133;
				Match(T__5);
				State = 134;
				shape();
				State = 137;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
				if (_la==T__5) {
					{
					State = 135;
					Match(T__5);
					State = 136;
					droplettype();
					}
				}

				}
				break;
			case T__26:
			case T__27:
			case T__28:
			case T__29:
				EnterOuterAlt(_localctx, 8);
				{
				State = 139;
				temp();
				State = 140;
				Match(T__5);
				State = 141;
				dropletname();
				State = 142;
				Match(T__5);
				State = 143;
				tileentity();
				State = 144;
				Match(T__5);
				State = 145;
				number();
				State = 148;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
				if (_la==T__5) {
					{
					State = 146;
					Match(T__5);
					State = 147;
					droplettype();
					}
				}

				}
				break;
			case T__30:
			case T__31:
				EnterOuterAlt(_localctx, 9);
				{
				State = 150;
				sense();
				State = 151;
				Match(T__5);
				State = 152;
				dropletname();
				State = 153;
				Match(T__5);
				State = 154;
				tileentity();
				}
				break;
			case T__32:
			case T__33:
				EnterOuterAlt(_localctx, 10);
				{
				State = 156;
				wait();
				State = 157;
				Match(T__5);
				State = 158;
				dropletname();
				State = 159;
				Match(T__5);
				State = 160;
				number();
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class InputContext : ParserRuleContext {
		public InputContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_input; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterInput(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitInput(this);
		}
	}

	[RuleVersion(0)]
	public InputContext input() {
		InputContext _localctx = new InputContext(Context, State);
		EnterRule(_localctx, 14, RULE_input);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 164;
			_la = TokenStream.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 1920L) != 0)) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
				ErrorHandler.ReportMatch(this);
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class OutputContext : ParserRuleContext {
		public OutputContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_output; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterOutput(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitOutput(this);
		}
	}

	[RuleVersion(0)]
	public OutputContext output() {
		OutputContext _localctx = new OutputContext(Context, State);
		EnterRule(_localctx, 16, RULE_output);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 166;
			_la = TokenStream.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 30720L) != 0)) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
				ErrorHandler.ReportMatch(this);
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class WasteContext : ParserRuleContext {
		public WasteContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_waste; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterWaste(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitWaste(this);
		}
	}

	[RuleVersion(0)]
	public WasteContext waste() {
		WasteContext _localctx = new WasteContext(Context, State);
		EnterRule(_localctx, 18, RULE_waste);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 168;
			_la = TokenStream.LA(1);
			if ( !(_la==T__14 || _la==T__15) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
				ErrorHandler.ReportMatch(this);
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ContaminateContext : ParserRuleContext {
		public ContaminateContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_contaminate; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterContaminate(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitContaminate(this);
		}
	}

	[RuleVersion(0)]
	public ContaminateContext contaminate() {
		ContaminateContext _localctx = new ContaminateContext(Context, State);
		EnterRule(_localctx, 20, RULE_contaminate);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 170;
			_la = TokenStream.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 1966080L) != 0)) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
				ErrorHandler.ReportMatch(this);
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class MergeContext : ParserRuleContext {
		public MergeContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_merge; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterMerge(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitMerge(this);
		}
	}

	[RuleVersion(0)]
	public MergeContext merge() {
		MergeContext _localctx = new MergeContext(Context, State);
		EnterRule(_localctx, 22, RULE_merge);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 172;
			_la = TokenStream.LA(1);
			if ( !(_la==T__20 || _la==T__21) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
				ErrorHandler.ReportMatch(this);
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class SplitContext : ParserRuleContext {
		public SplitContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_split; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterSplit(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitSplit(this);
		}
	}

	[RuleVersion(0)]
	public SplitContext split() {
		SplitContext _localctx = new SplitContext(Context, State);
		EnterRule(_localctx, 24, RULE_split);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 174;
			_la = TokenStream.LA(1);
			if ( !(_la==T__22 || _la==T__23) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
				ErrorHandler.ReportMatch(this);
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class MixContext : ParserRuleContext {
		public MixContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_mix; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterMix(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitMix(this);
		}
	}

	[RuleVersion(0)]
	public MixContext mix() {
		MixContext _localctx = new MixContext(Context, State);
		EnterRule(_localctx, 26, RULE_mix);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 176;
			_la = TokenStream.LA(1);
			if ( !(_la==T__24 || _la==T__25) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
				ErrorHandler.ReportMatch(this);
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class TempContext : ParserRuleContext {
		public TempContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_temp; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterTemp(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitTemp(this);
		}
	}

	[RuleVersion(0)]
	public TempContext temp() {
		TempContext _localctx = new TempContext(Context, State);
		EnterRule(_localctx, 28, RULE_temp);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 178;
			_la = TokenStream.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 2013265920L) != 0)) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
				ErrorHandler.ReportMatch(this);
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class SenseContext : ParserRuleContext {
		public SenseContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_sense; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterSense(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitSense(this);
		}
	}

	[RuleVersion(0)]
	public SenseContext sense() {
		SenseContext _localctx = new SenseContext(Context, State);
		EnterRule(_localctx, 30, RULE_sense);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 180;
			_la = TokenStream.LA(1);
			if ( !(_la==T__30 || _la==T__31) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
				ErrorHandler.ReportMatch(this);
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class WaitContext : ParserRuleContext {
		public WaitContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_wait; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.EnterWait(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IProgramListener typedListener = listener as IProgramListener;
			if (typedListener != null) typedListener.ExitWait(this);
		}
	}

	[RuleVersion(0)]
	public WaitContext wait() {
		WaitContext _localctx = new WaitContext(Context, State);
		EnterRule(_localctx, 32, RULE_wait);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 182;
			_la = TokenStream.LA(1);
			if ( !(_la==T__32 || _la==T__33) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
				ErrorHandler.ReportMatch(this);
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	private static int[] _serializedATN = {
		4,1,37,185,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,6,2,7,
		7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,14,7,14,
		2,15,7,15,2,16,7,16,1,0,1,0,1,0,1,0,5,0,39,8,0,10,0,12,0,42,9,0,1,0,1,
		0,1,0,3,0,47,8,0,1,1,1,1,1,2,1,2,1,3,1,3,1,4,1,4,1,5,1,5,1,6,1,6,1,6,1,
		6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,
		1,6,1,6,1,6,1,6,5,6,84,8,6,10,6,12,6,87,9,6,1,6,1,6,1,6,1,6,1,6,1,6,1,
		6,1,6,1,6,1,6,1,6,5,6,100,8,6,10,6,12,6,103,9,6,1,6,1,6,1,6,1,6,1,6,1,
		6,1,6,3,6,112,8,6,1,6,1,6,1,6,1,6,3,6,118,8,6,1,6,1,6,1,6,1,6,3,6,124,
		8,6,5,6,126,8,6,10,6,12,6,129,9,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,3,6,138,
		8,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,3,6,149,8,6,1,6,1,6,1,6,1,6,1,
		6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,3,6,163,8,6,1,7,1,7,1,8,1,8,1,9,1,9,1,10,
		1,10,1,11,1,11,1,12,1,12,1,13,1,13,1,14,1,14,1,15,1,15,1,16,1,16,1,16,
		0,0,17,0,2,4,6,8,10,12,14,16,18,20,22,24,26,28,30,32,0,11,1,0,2,5,1,0,
		7,10,1,0,11,14,1,0,15,16,1,0,17,20,1,0,21,22,1,0,23,24,1,0,25,26,1,0,27,
		30,1,0,31,32,1,0,33,34,186,0,40,1,0,0,0,2,48,1,0,0,0,4,50,1,0,0,0,6,52,
		1,0,0,0,8,54,1,0,0,0,10,56,1,0,0,0,12,162,1,0,0,0,14,164,1,0,0,0,16,166,
		1,0,0,0,18,168,1,0,0,0,20,170,1,0,0,0,22,172,1,0,0,0,24,174,1,0,0,0,26,
		176,1,0,0,0,28,178,1,0,0,0,30,180,1,0,0,0,32,182,1,0,0,0,34,35,3,12,6,
		0,35,36,5,1,0,0,36,37,5,35,0,0,37,39,1,0,0,0,38,34,1,0,0,0,39,42,1,0,0,
		0,40,38,1,0,0,0,40,41,1,0,0,0,41,43,1,0,0,0,42,40,1,0,0,0,43,44,3,12,6,
		0,44,46,5,1,0,0,45,47,5,35,0,0,46,45,1,0,0,0,46,47,1,0,0,0,47,1,1,0,0,
		0,48,49,5,36,0,0,49,3,1,0,0,0,50,51,5,36,0,0,51,5,1,0,0,0,52,53,5,37,0,
		0,53,7,1,0,0,0,54,55,7,0,0,0,55,9,1,0,0,0,56,57,5,36,0,0,57,11,1,0,0,0,
		58,59,3,14,7,0,59,60,5,6,0,0,60,61,3,2,1,0,61,62,5,6,0,0,62,63,3,4,2,0,
		63,64,5,6,0,0,64,65,3,10,5,0,65,66,5,6,0,0,66,67,3,6,3,0,67,163,1,0,0,
		0,68,69,3,16,8,0,69,70,5,6,0,0,70,71,3,2,1,0,71,72,5,6,0,0,72,73,3,10,
		5,0,73,163,1,0,0,0,74,75,3,18,9,0,75,76,5,6,0,0,76,77,3,2,1,0,77,163,1,
		0,0,0,78,79,3,20,10,0,79,80,5,6,0,0,80,85,3,4,2,0,81,82,5,6,0,0,82,84,
		3,4,2,0,83,81,1,0,0,0,84,87,1,0,0,0,85,83,1,0,0,0,85,86,1,0,0,0,86,163,
		1,0,0,0,87,85,1,0,0,0,88,89,3,22,11,0,89,90,5,6,0,0,90,91,3,2,1,0,91,92,
		5,6,0,0,92,93,3,4,2,0,93,94,5,6,0,0,94,95,3,2,1,0,95,96,5,6,0,0,96,101,
		3,2,1,0,97,98,5,6,0,0,98,100,3,2,1,0,99,97,1,0,0,0,100,103,1,0,0,0,101,
		99,1,0,0,0,101,102,1,0,0,0,102,163,1,0,0,0,103,101,1,0,0,0,104,105,3,24,
		12,0,105,106,5,6,0,0,106,107,3,2,1,0,107,108,5,6,0,0,108,111,3,2,1,0,109,
		110,5,6,0,0,110,112,3,6,3,0,111,109,1,0,0,0,111,112,1,0,0,0,112,113,1,
		0,0,0,113,114,5,6,0,0,114,117,3,2,1,0,115,116,5,6,0,0,116,118,3,6,3,0,
		117,115,1,0,0,0,117,118,1,0,0,0,118,127,1,0,0,0,119,120,5,6,0,0,120,123,
		3,2,1,0,121,122,5,6,0,0,122,124,3,6,3,0,123,121,1,0,0,0,123,124,1,0,0,
		0,124,126,1,0,0,0,125,119,1,0,0,0,126,129,1,0,0,0,127,125,1,0,0,0,127,
		128,1,0,0,0,128,163,1,0,0,0,129,127,1,0,0,0,130,131,3,26,13,0,131,132,
		5,6,0,0,132,133,3,2,1,0,133,134,5,6,0,0,134,137,3,8,4,0,135,136,5,6,0,
		0,136,138,3,4,2,0,137,135,1,0,0,0,137,138,1,0,0,0,138,163,1,0,0,0,139,
		140,3,28,14,0,140,141,5,6,0,0,141,142,3,2,1,0,142,143,5,6,0,0,143,144,
		3,10,5,0,144,145,5,6,0,0,145,148,3,6,3,0,146,147,5,6,0,0,147,149,3,4,2,
		0,148,146,1,0,0,0,148,149,1,0,0,0,149,163,1,0,0,0,150,151,3,30,15,0,151,
		152,5,6,0,0,152,153,3,2,1,0,153,154,5,6,0,0,154,155,3,10,5,0,155,163,1,
		0,0,0,156,157,3,32,16,0,157,158,5,6,0,0,158,159,3,2,1,0,159,160,5,6,0,
		0,160,161,3,6,3,0,161,163,1,0,0,0,162,58,1,0,0,0,162,68,1,0,0,0,162,74,
		1,0,0,0,162,78,1,0,0,0,162,88,1,0,0,0,162,104,1,0,0,0,162,130,1,0,0,0,
		162,139,1,0,0,0,162,150,1,0,0,0,162,156,1,0,0,0,163,13,1,0,0,0,164,165,
		7,1,0,0,165,15,1,0,0,0,166,167,7,2,0,0,167,17,1,0,0,0,168,169,7,3,0,0,
		169,19,1,0,0,0,170,171,7,4,0,0,171,21,1,0,0,0,172,173,7,5,0,0,173,23,1,
		0,0,0,174,175,7,6,0,0,175,25,1,0,0,0,176,177,7,7,0,0,177,27,1,0,0,0,178,
		179,7,8,0,0,179,29,1,0,0,0,180,181,7,9,0,0,181,31,1,0,0,0,182,183,7,10,
		0,0,183,33,1,0,0,0,11,40,46,85,101,111,117,123,127,137,148,162
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
