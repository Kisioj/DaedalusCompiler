//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.7.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Zen.g4 by ANTLR 4.7.2

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

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.7.2")]
[System.CLSCompliant(false)]
public partial class ZenParser : Parser {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, MeshAndBsp=8, 
		Whitespace=9, Newline=10, Int=11, Date=12, Time=13, Value=14, Name=15;
	public const int
		RULE_main = 0, RULE_head = 1, RULE_block = 2, RULE_blockName = 3, RULE_classPath = 4, 
		RULE_attr = 5;
	public static readonly string[] ruleNames = {
		"main", "head", "block", "blockName", "classPath", "attr"
	};

	private static readonly string[] _LiteralNames = {
		null, "'['", "']'", "'[]'", "'%'", "':'", "'\u00A7'", "'='"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, "MeshAndBsp", "Whitespace", 
		"Newline", "Int", "Date", "Time", "Value", "Name"
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

	public override string GrammarFileName { get { return "Zen.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static ZenParser() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

		public ZenParser(ITokenStream input) : this(input, Console.Out, Console.Error) { }

		public ZenParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}
	public partial class MainContext : ParserRuleContext {
		public BlockContext body;
		public HeadContext head() {
			return GetRuleContext<HeadContext>(0);
		}
		public ITerminalNode Eof() { return GetToken(ZenParser.Eof, 0); }
		public BlockContext block() {
			return GetRuleContext<BlockContext>(0);
		}
		public MainContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_main; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IZenVisitor<TResult> typedVisitor = visitor as IZenVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitMain(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public MainContext main() {
		MainContext _localctx = new MainContext(Context, State);
		EnterRule(_localctx, 0, RULE_main);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 12; head();
			{
			State = 13; _localctx.body = block();
			}
			State = 14; Match(Eof);
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

	public partial class HeadContext : ParserRuleContext {
		public IToken version;
		public IToken zenType;
		public IToken saveGame;
		public IToken date;
		public IToken time;
		public IToken user;
		public IToken objectsCount;
		public ITerminalNode[] Name() { return GetTokens(ZenParser.Name); }
		public ITerminalNode Name(int i) {
			return GetToken(ZenParser.Name, i);
		}
		public ITerminalNode[] Int() { return GetTokens(ZenParser.Int); }
		public ITerminalNode Int(int i) {
			return GetToken(ZenParser.Int, i);
		}
		public ITerminalNode Date() { return GetToken(ZenParser.Date, 0); }
		public ITerminalNode Time() { return GetToken(ZenParser.Time, 0); }
		public HeadContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_head; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IZenVisitor<TResult> typedVisitor = visitor as IZenVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitHead(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public HeadContext head() {
		HeadContext _localctx = new HeadContext(Context, State);
		EnterRule(_localctx, 2, RULE_head);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 16; Match(Name);
			State = 17; Match(Name);
			State = 18; Match(Name);
			{
			State = 19; _localctx.version = Match(Int);
			}
			State = 20; Match(Name);
			{
			State = 21; _localctx.zenType = Match(Name);
			}
			State = 22; Match(Name);
			{
			State = 23; _localctx.saveGame = Match(Int);
			}
			State = 24; Match(Name);
			{
			State = 25; _localctx.date = Match(Date);
			}
			{
			State = 26; _localctx.time = Match(Time);
			}
			State = 27; Match(Name);
			{
			State = 28; _localctx.user = Match(Name);
			}
			State = 29; Match(Name);
			State = 30; Match(Name);
			{
			State = 31; _localctx.objectsCount = Match(Int);
			}
			State = 32; Match(Name);
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

	public partial class BlockContext : ParserRuleContext {
		public IToken leftIndex;
		public IToken rightIndex;
		public BlockNameContext blockName() {
			return GetRuleContext<BlockNameContext>(0);
		}
		public ClassPathContext classPath() {
			return GetRuleContext<ClassPathContext>(0);
		}
		public ITerminalNode[] Int() { return GetTokens(ZenParser.Int); }
		public ITerminalNode Int(int i) {
			return GetToken(ZenParser.Int, i);
		}
		public BlockContext[] block() {
			return GetRuleContexts<BlockContext>();
		}
		public BlockContext block(int i) {
			return GetRuleContext<BlockContext>(i);
		}
		public AttrContext[] attr() {
			return GetRuleContexts<AttrContext>();
		}
		public AttrContext attr(int i) {
			return GetRuleContext<AttrContext>(i);
		}
		public BlockContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_block; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IZenVisitor<TResult> typedVisitor = visitor as IZenVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitBlock(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public BlockContext block() {
		BlockContext _localctx = new BlockContext(Context, State);
		EnterRule(_localctx, 4, RULE_block);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 34; Match(T__0);
			State = 35; blockName();
			State = 36; classPath();
			{
			State = 37; _localctx.leftIndex = Match(Int);
			}
			{
			State = 38; _localctx.rightIndex = Match(Int);
			}
			State = 39; Match(T__1);
			State = 44;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==T__0 || _la==Name) {
				{
				State = 42;
				ErrorHandler.Sync(this);
				switch (TokenStream.LA(1)) {
				case T__0:
					{
					State = 40; block();
					}
					break;
				case Name:
					{
					State = 41; attr();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				State = 46;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 47; Match(T__2);
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

	public partial class BlockNameContext : ParserRuleContext {
		public ITerminalNode Name() { return GetToken(ZenParser.Name, 0); }
		public BlockNameContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_blockName; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IZenVisitor<TResult> typedVisitor = visitor as IZenVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitBlockName(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public BlockNameContext blockName() {
		BlockNameContext _localctx = new BlockNameContext(Context, State);
		EnterRule(_localctx, 6, RULE_blockName);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 49;
			_la = TokenStream.LA(1);
			if ( !(_la==T__3 || _la==Name) ) {
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

	public partial class ClassPathContext : ParserRuleContext {
		public ITerminalNode[] Name() { return GetTokens(ZenParser.Name); }
		public ITerminalNode Name(int i) {
			return GetToken(ZenParser.Name, i);
		}
		public ClassPathContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_classPath; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IZenVisitor<TResult> typedVisitor = visitor as IZenVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitClassPath(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ClassPathContext classPath() {
		ClassPathContext _localctx = new ClassPathContext(Context, State);
		EnterRule(_localctx, 8, RULE_classPath);
		int _la;
		try {
			State = 61;
			ErrorHandler.Sync(this);
			switch (TokenStream.LA(1)) {
			case Name:
				EnterOuterAlt(_localctx, 1);
				{
				State = 51; Match(Name);
				State = 56;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
				while (_la==T__4) {
					{
					{
					State = 52; Match(T__4);
					State = 53; Match(Name);
					}
					}
					State = 58;
					ErrorHandler.Sync(this);
					_la = TokenStream.LA(1);
				}
				}
				break;
			case T__5:
				EnterOuterAlt(_localctx, 2);
				{
				State = 59; Match(T__5);
				}
				break;
			case T__3:
				EnterOuterAlt(_localctx, 3);
				{
				State = 60; Match(T__3);
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

	public partial class AttrContext : ParserRuleContext {
		public ITerminalNode Name() { return GetToken(ZenParser.Name, 0); }
		public ITerminalNode Value() { return GetToken(ZenParser.Value, 0); }
		public AttrContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_attr; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IZenVisitor<TResult> typedVisitor = visitor as IZenVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitAttr(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public AttrContext attr() {
		AttrContext _localctx = new AttrContext(Context, State);
		EnterRule(_localctx, 10, RULE_attr);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 63; Match(Name);
			State = 64; Match(T__6);
			State = 65; Match(Value);
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

	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x3', '\x11', '\x46', '\x4', '\x2', '\t', '\x2', '\x4', '\x3', 
		'\t', '\x3', '\x4', '\x4', '\t', '\x4', '\x4', '\x5', '\t', '\x5', '\x4', 
		'\x6', '\t', '\x6', '\x4', '\a', '\t', '\a', '\x3', '\x2', '\x3', '\x2', 
		'\x3', '\x2', '\x3', '\x2', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', 
		'\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\a', '\x4', '-', '\n', '\x4', 
		'\f', '\x4', '\xE', '\x4', '\x30', '\v', '\x4', '\x3', '\x4', '\x3', '\x4', 
		'\x3', '\x5', '\x3', '\x5', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', 
		'\a', '\x6', '\x39', '\n', '\x6', '\f', '\x6', '\xE', '\x6', '<', '\v', 
		'\x6', '\x3', '\x6', '\x3', '\x6', '\x5', '\x6', '@', '\n', '\x6', '\x3', 
		'\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x2', '\x2', 
		'\b', '\x2', '\x4', '\x6', '\b', '\n', '\f', '\x2', '\x3', '\x4', '\x2', 
		'\x6', '\x6', '\x11', '\x11', '\x2', '\x44', '\x2', '\xE', '\x3', '\x2', 
		'\x2', '\x2', '\x4', '\x12', '\x3', '\x2', '\x2', '\x2', '\x6', '$', '\x3', 
		'\x2', '\x2', '\x2', '\b', '\x33', '\x3', '\x2', '\x2', '\x2', '\n', '?', 
		'\x3', '\x2', '\x2', '\x2', '\f', '\x41', '\x3', '\x2', '\x2', '\x2', 
		'\xE', '\xF', '\x5', '\x4', '\x3', '\x2', '\xF', '\x10', '\x5', '\x6', 
		'\x4', '\x2', '\x10', '\x11', '\a', '\x2', '\x2', '\x3', '\x11', '\x3', 
		'\x3', '\x2', '\x2', '\x2', '\x12', '\x13', '\a', '\x11', '\x2', '\x2', 
		'\x13', '\x14', '\a', '\x11', '\x2', '\x2', '\x14', '\x15', '\a', '\x11', 
		'\x2', '\x2', '\x15', '\x16', '\a', '\r', '\x2', '\x2', '\x16', '\x17', 
		'\a', '\x11', '\x2', '\x2', '\x17', '\x18', '\a', '\x11', '\x2', '\x2', 
		'\x18', '\x19', '\a', '\x11', '\x2', '\x2', '\x19', '\x1A', '\a', '\r', 
		'\x2', '\x2', '\x1A', '\x1B', '\a', '\x11', '\x2', '\x2', '\x1B', '\x1C', 
		'\a', '\xE', '\x2', '\x2', '\x1C', '\x1D', '\a', '\xF', '\x2', '\x2', 
		'\x1D', '\x1E', '\a', '\x11', '\x2', '\x2', '\x1E', '\x1F', '\a', '\x11', 
		'\x2', '\x2', '\x1F', ' ', '\a', '\x11', '\x2', '\x2', ' ', '!', '\a', 
		'\x11', '\x2', '\x2', '!', '\"', '\a', '\r', '\x2', '\x2', '\"', '#', 
		'\a', '\x11', '\x2', '\x2', '#', '\x5', '\x3', '\x2', '\x2', '\x2', '$', 
		'%', '\a', '\x3', '\x2', '\x2', '%', '&', '\x5', '\b', '\x5', '\x2', '&', 
		'\'', '\x5', '\n', '\x6', '\x2', '\'', '(', '\a', '\r', '\x2', '\x2', 
		'(', ')', '\a', '\r', '\x2', '\x2', ')', '.', '\a', '\x4', '\x2', '\x2', 
		'*', '-', '\x5', '\x6', '\x4', '\x2', '+', '-', '\x5', '\f', '\a', '\x2', 
		',', '*', '\x3', '\x2', '\x2', '\x2', ',', '+', '\x3', '\x2', '\x2', '\x2', 
		'-', '\x30', '\x3', '\x2', '\x2', '\x2', '.', ',', '\x3', '\x2', '\x2', 
		'\x2', '.', '/', '\x3', '\x2', '\x2', '\x2', '/', '\x31', '\x3', '\x2', 
		'\x2', '\x2', '\x30', '.', '\x3', '\x2', '\x2', '\x2', '\x31', '\x32', 
		'\a', '\x5', '\x2', '\x2', '\x32', '\a', '\x3', '\x2', '\x2', '\x2', '\x33', 
		'\x34', '\t', '\x2', '\x2', '\x2', '\x34', '\t', '\x3', '\x2', '\x2', 
		'\x2', '\x35', ':', '\a', '\x11', '\x2', '\x2', '\x36', '\x37', '\a', 
		'\a', '\x2', '\x2', '\x37', '\x39', '\a', '\x11', '\x2', '\x2', '\x38', 
		'\x36', '\x3', '\x2', '\x2', '\x2', '\x39', '<', '\x3', '\x2', '\x2', 
		'\x2', ':', '\x38', '\x3', '\x2', '\x2', '\x2', ':', ';', '\x3', '\x2', 
		'\x2', '\x2', ';', '@', '\x3', '\x2', '\x2', '\x2', '<', ':', '\x3', '\x2', 
		'\x2', '\x2', '=', '@', '\a', '\b', '\x2', '\x2', '>', '@', '\a', '\x6', 
		'\x2', '\x2', '?', '\x35', '\x3', '\x2', '\x2', '\x2', '?', '=', '\x3', 
		'\x2', '\x2', '\x2', '?', '>', '\x3', '\x2', '\x2', '\x2', '@', '\v', 
		'\x3', '\x2', '\x2', '\x2', '\x41', '\x42', '\a', '\x11', '\x2', '\x2', 
		'\x42', '\x43', '\a', '\t', '\x2', '\x2', '\x43', '\x44', '\a', '\x10', 
		'\x2', '\x2', '\x44', '\r', '\x3', '\x2', '\x2', '\x2', '\x6', ',', '.', 
		':', '?',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
