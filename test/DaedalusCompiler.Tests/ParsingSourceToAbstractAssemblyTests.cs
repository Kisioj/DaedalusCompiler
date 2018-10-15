﻿using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DaedalusCompiler.Compilation;
using DaedalusCompiler.Dat;
using Xunit;
using AssemblyBuilder = DaedalusCompiler.Compilation.AssemblyBuilder;

namespace DaedalusCompiler.Tests
{
    public class ParsingSourceToAbstractAssemblyTests
    {
        private readonly AssemblyBuilder _assemblyBuilder;
        private string _code;
        private string _externalCode;
        private List<AssemblyElement> _instructions;
        private List<AssemblyElement> _expectedInstructions;
        private List<DatSymbol> _expectedSymbols;
        private bool _parsed;

        public ParsingSourceToAbstractAssemblyTests()
        {
            _assemblyBuilder = new AssemblyBuilder();
            _instructions = new List<AssemblyElement>();
            _expectedInstructions = new List<AssemblyElement>();
            _expectedSymbols = new List<DatSymbol>();
            _parsed = false;
            _externalCode = String.Empty;
        }

        private int RefIndex(string symbolName)
        {
            DatSymbol symbol = _assemblyBuilder.ResolveSymbol(symbolName);
            return symbol.Index;
        }
        
        private DatSymbol Ref(string symbolName)
        {
            return _assemblyBuilder.ResolveSymbol(symbolName);
        }

        private List<AssemblyElement> GetExecBlockInstructions(string execBlockName)
        {
            if (!_parsed)
            {
                ParseData();
            }

            return _assemblyBuilder.ExecBlocks
                .Find(execBlock => execBlock.Symbol.Name.ToUpper() == execBlockName.ToUpper()).Body;
        }

        private void ParseData()
        {
            _parsed = true;

            if (_externalCode != string.Empty)
            {
                _assemblyBuilder.IsCurrentlyParsingExternals = true;
                Utils.WalkSourceCode(_externalCode, _assemblyBuilder);
                _assemblyBuilder.IsCurrentlyParsingExternals = false;
            }
            Utils.WalkSourceCode(_code, _assemblyBuilder);
            _assemblyBuilder.Finish();
        }

        private void AssertRefContentEqual(string symbolName, object expectedValue)
        {
            if (expectedValue == null) throw new ArgumentNullException(nameof(expectedValue));
            Assert.Equal(expectedValue, Ref(symbolName).Content[0]);
        }

        private void AssertInstructionsMatch()
        {
            for (var index = 0; index < _expectedInstructions.Count; index++)
            {
                var instruction = _instructions[index];
                var expectedInstruction = _expectedInstructions[index];
                CompareInstructions(expectedInstruction, instruction);
            }
        }

        private void AssertSymbolsMatch()
        {
            Assert.Equal(_expectedSymbols.Count, _assemblyBuilder.Symbols.Count);
            for (var index = 0; index < _expectedSymbols.Count; index++)
            {
                var symbol = _assemblyBuilder.Symbols[index];
                var expectedSymbol = _expectedSymbols[index];
                Assert.Equal(index, symbol.Index);
                Assert.Equal(expectedSymbol, symbol);
            }
        }


        private void CompareInstructions(AssemblyElement expectedInstruction, AssemblyElement instruction)
        {
            if (expectedInstruction == null) throw new ArgumentNullException(nameof(expectedInstruction));
            Assert.Equal(expectedInstruction.GetType(), instruction.GetType());
            switch (instruction)
            {
                case PushInt pushIntInstruction:
                {
                    Assert.Equal(
                        ((PushInt) expectedInstruction).Value,
                        pushIntInstruction.Value
                    );
                    break;
                }
                case Call _:
                case CallExternal _:
                case PushVar _:
                    Assert.Equal(
                        ((SymbolInstruction) expectedInstruction).Symbol,
                        ((SymbolInstruction) instruction).Symbol
                    );
                    break;
                case PushArrayVar pushArrVarInstruction:
                {
                    Assert.Equal(
                        ((SymbolInstruction) expectedInstruction).Symbol,
                        ((SymbolInstruction) instruction).Symbol
                    );
                    Assert.Equal(
                        ((PushArrayVar) expectedInstruction).Index,
                        pushArrVarInstruction.Index
                    );
                    break;
                }
                case AssemblyLabel assemblyLabelInstruction:
                {
                    Assert.Equal(
                        ((AssemblyLabel) expectedInstruction).Label,
                        assemblyLabelInstruction.Label
                    );
                    break;
                }

                case JumpIfToLabel _:
                case JumpToLabel _:
                {
                    var jumpInstruction = (JumpToLabel) instruction;
                    Assert.Equal(
                        ((JumpToLabel) expectedInstruction).Label,
                        jumpInstruction.Label);
                    break;
                }
            }
        }
        
        
        [Fact]
        public void TestIntAddOperator()
        {
            _code = @"
                var int x;

                func void testFunc() {
                    x = 2 + 3 + 4 + 5;
                    x = 2 - 3 - 4 - 5;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // x = 2 + 3 + 4 + 5;
                new PushInt(5),
                new PushInt(4),
                new PushInt(3),
                new PushInt(2),
                new Add(),
                new Add(),
                new Add(),
                new PushVar(Ref("x")),
                new Assign(),

                // x = 2 - 3 - 4 - 5;
                new PushInt(5),
                new PushInt(4),
                new PushInt(3),
                new PushInt(2),
                new Subtract(),
                new Subtract(),
                new Subtract(),
                new PushVar(Ref("x")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("x"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestIntMultOperator()
        {
            _code = @"
                var int x;

                func void testFunc() {
                    x = 2 * 3 * 4 * 5;
                    x = 2 / 3 / 4 / 5;
                    x = 2 % 3 % 4 % 5;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // x = 2 * 3 * 4 * 5;
                new PushInt(5),
                new PushInt(4),
                new PushInt(3),
                new PushInt(2),
                new Multiply(),
                new Multiply(),
                new Multiply(),
                new PushVar(Ref("x")),
                new Assign(),

                // x = 2 / 3 / 4 / 5;
                new PushInt(5),
                new PushInt(4),
                new PushInt(3),
                new PushInt(2),
                new Divide(),
                new Divide(),
                new Divide(),
                new PushVar(Ref("x")),
                new Assign(),

                // x = 2 / 3 / 4 / 5;
                new PushInt(5),
                new PushInt(4),
                new PushInt(3),
                new PushInt(2),
                new Modulo(),
                new Modulo(),
                new Modulo(),
                new PushVar(Ref("x")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("x"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestIntAssignmentOperator()
        {
            _code = @"
                var int x;

                func void testFunc() {
                    x = 1
                    x += 2;
                    x -= 3;
                    x *= 4;
                    x /= 5;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // x = 1;
                new PushInt(1),
                new PushVar(Ref("x")),
                new Assign(),

                // x += 2;
                new PushInt(2),
                new PushVar(Ref("x")),
                new AssignAdd(),

                // x -= 3;
                new PushInt(3),
                new PushVar(Ref("x")),
                new AssignSubtract(),

                // x *= 4;
                new PushInt(4),
                new PushVar(Ref("x")),
                new AssignMultiply(),

                // x /= 5;
                new PushInt(5),
                new PushVar(Ref("x")),
                new AssignDivide(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("x"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }
        
        [Fact]
        public void TestInstanceAssignment()
        {
            _externalCode = @"
                func instance HLP_GetNpc(var int par0) {};
            ";
            _code = @"
                class C_NPC { var int data [200]; };
                var C_NPC person;
                
                func void  testFunc()
                {
                    var int test;
                    person = HLP_GetNpc (0);
                    test = person == person;
                    test = test == person;
                    test = person + person;
                    test = test + person;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // person = HLP_GetNpc (0);
                new PushInt(0),
                new CallExternal(Ref("HLP_GetNpc")),
                new PushInstance(Ref("person")),
                new AssignInstance(),
                
                // test = person == person;
                new PushInt(RefIndex("person")),
                new PushInt(RefIndex("person")),
                new Equal(),
                new PushVar(Ref("testFunc.test")),
                new Assign(),
                
                // test = test == person;
                new PushInt(RefIndex("person")),
                new PushVar(Ref("testFunc.test")),
                new Equal(),
                new PushVar(Ref("testFunc.test")),
                new Assign(),
                
                // test = person + person;
                new PushInt(RefIndex("person")),
                new PushInt(RefIndex("person")),
                new Add(),
                new PushVar(Ref("testFunc.test")),
                new Assign(),
                
                // test = test + person;
                new PushInt(RefIndex("person")),
                new PushVar(Ref("testFunc.test")),
                new Add(),
                new PushVar(Ref("testFunc.test")),
                new Assign(),


                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("HLP_GetNpc"),
                Ref("HLP_GetNpc.par0"),
                Ref("C_NPC"),
                Ref("C_NPC.data"),
                Ref("person"),
                Ref("testFunc"),
                Ref("testFunc.test"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestIntAddAndMultOperatorPrecedence()
        {
            _code = @"
                var int a;
                var int b;
                var int c;
                var int d;

                func void testFunc() {
                    a = 1 + 2 * 3;
                    a += 1 + 2 / 3;
    
                    b = 1 - 2 * 3;
                    b -= 1 - 2 / 3;
                                
                    c = 4 / (5 + 6) * 7;
                    c *= 4 * (5 + 6) / 7;
    
                    d = 4 / (5 - 6) * 7;
                    d /= 4 * (5 - 6) / 7;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // a = 1 + 2 * 3;
                new PushInt(3),
                new PushInt(2),
                new Multiply(),
                new PushInt(1),
                new Add(),
                new PushVar(Ref("a")),
                new Assign(),

                // a += 1 + 2 / 3;
                new PushInt(3),
                new PushInt(2),
                new Divide(),
                new PushInt(1),
                new Add(),
                new PushVar(Ref("a")),
                new AssignAdd(),

                // b = 1 - 2 * 3;
                new PushInt(3),
                new PushInt(2),
                new Multiply(),
                new PushInt(1),
                new Subtract(),
                new PushVar(Ref("b")),
                new Assign(),

                // b -= 1 - 2 / 3;
                new PushInt(3),
                new PushInt(2),
                new Divide(),
                new PushInt(1),
                new Subtract(),
                new PushVar(Ref("b")),
                new AssignSubtract(),

                // c = 4 / (5 + 6) * 7;
                new PushInt(7),
                new PushInt(6),
                new PushInt(5),
                new Add(),
                new PushInt(4),
                new Divide(),
                new Multiply(),
                new PushVar(Ref("c")),
                new Assign(),

                // c *= 4 * (5 + 6) / 7;
                new PushInt(7),
                new PushInt(6),
                new PushInt(5),
                new Add(),
                new PushInt(4),
                new Multiply(),
                new Divide(),
                new PushVar(Ref("c")),
                new AssignMultiply(),

                // d = 4 / (5 - 6) * 7;
                new PushInt(7),
                new PushInt(6),
                new PushInt(5),
                new Subtract(),
                new PushInt(4),
                new Divide(),
                new Multiply(),
                new PushVar(Ref("d")),
                new Assign(),

                // d /= 4 * (5 - 6) / 7;
                new PushInt(7),
                new PushInt(6),
                new PushInt(5),
                new Subtract(),
                new PushInt(4),
                new Multiply(),
                new Divide(),
                new PushVar(Ref("d")),
                new AssignDivide(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("a"),
                Ref("b"),
                Ref("c"),
                Ref("d"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestIntEqOperator()
        {
            _code = @"
                var int a;
                var int b;
                var int c;
                var int d;

                func void testFunc() {
                    a = 1 == 2 != 3;
                    a = 1 != 2 == 3;
                    a = b == c != d;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // a = 1 == 2 != 3;
                new PushInt(3),
                new PushInt(2),
                new PushInt(1),
                new Equal(),
                new NotEqual(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = 1 != 2 == 3;
                new PushInt(3),
                new PushInt(2),
                new PushInt(1),
                new NotEqual(),
                new Equal(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = b == c != d;
                new PushVar(Ref("d")),
                new PushVar(Ref("c")),
                new PushVar(Ref("b")),
                new Equal(),
                new NotEqual(),
                new PushVar(Ref("a")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("a"),
                Ref("b"),
                Ref("c"),
                Ref("d"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestIntOneArgOperator()
        {
            _code = @"
                var int a;
                var int b;
                var int c;
                var int d;

                func void testFunc() {
                    a = -1;
                    b = !2;
                    c = ~3;
                    d = +4;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // a = -1;
                new PushInt(1),
                new Minus(),
                new PushVar(Ref("a")),
                new Assign(),

                // b = !2;
                new PushInt(2),
                new Not(),
                new PushVar(Ref("b")),
                new Assign(),

                // c = ~3;
                new PushInt(3),
                new Negate(),
                new PushVar(Ref("c")),
                new Assign(),

                // d = +4;
                new PushInt(4),
                new Plus(),
                new PushVar(Ref("d")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("a"),
                Ref("b"),
                Ref("c"),
                Ref("d"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }


        [Fact]
        public void TestIntLogAndBinOperator()
        {
            _code = @"
                var int a;
                var int b;
                var int c;
                var int d;

                func void testFunc() {
                    a = 1 & 2;
                    a = 1 | 2;
                    a = 1 && 2;
                    a = 1 || 2;
                    
                    a = 1 & b;
                    a = 1 | b;
                    a = 1 && b;
                    a = 1 || b;
                    
                    a = b & 2;
                    a = b | 2;
                    a = b && 2;
                    a = b || 2;
                    
                    a = c & d;
                    a = c | d;
                    a = c && d;
                    a = c || d;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // a = 1 & 2;
                new PushInt(2),
                new PushInt(1),
                new BitAnd(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = 1 | 2;
                new PushInt(2),
                new PushInt(1),
                new BitOr(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = 1 && 2;
                new PushInt(2),
                new PushInt(1),
                new LogAnd(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = 1 || 2;
                new PushInt(2),
                new PushInt(1),
                new LogOr(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = 1 & b;
                new PushVar(Ref("b")),
                new PushInt(1),
                new BitAnd(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = 1 | b;
                new PushVar(Ref("b")),
                new PushInt(1),
                new BitOr(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = 1 && b;
                new PushVar(Ref("b")),
                new PushInt(1),
                new LogAnd(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = 1 || b;
                new PushVar(Ref("b")),
                new PushInt(1),
                new LogOr(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = b & 2;
                new PushInt(2),
                new PushVar(Ref("b")),
                new BitAnd(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = b | 2;
                new PushInt(2),
                new PushVar(Ref("b")),
                new BitOr(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = b && 2;
                new PushInt(2),
                new PushVar(Ref("b")),
                new LogAnd(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = b || 2;
                new PushInt(2),
                new PushVar(Ref("b")),
                new LogOr(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = c & d;
                new PushVar(Ref("d")),
                new PushVar(Ref("c")),
                new BitAnd(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = c | d;
                new PushVar(Ref("d")),
                new PushVar(Ref("c")),
                new BitOr(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = c && d;
                new PushVar(Ref("d")),
                new PushVar(Ref("c")),
                new LogAnd(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = c || d;
                new PushVar(Ref("d")),
                new PushVar(Ref("c")),
                new LogOr(),
                new PushVar(Ref("a")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("a"),
                Ref("b"),
                Ref("c"),
                Ref("d"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }


        [Fact]
        public void TestIntLogAndBinOperatorPrecedence()
        {
            _code = @"
                var int a;
                var int b;

                func void testFunc() {
                    a = 0 || 1 && 2 | 3 & 4;
                    b = 5 & 6 | 7 && 8 || 9;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // a = 0 || 1 && 2 | 3 & 4;
                new PushInt(4),
                new PushInt(3),
                new BitAnd(),
                new PushInt(2),
                new BitOr(),
                new PushInt(1),
                new LogAnd(),
                new PushInt(0),
                new LogOr(),
                new PushVar(Ref("a")),
                new Assign(),

                // b = 5 & 6 | 7 && 8 || 9;
                new PushInt(9),
                new PushInt(8),
                new PushInt(7),
                new PushInt(6),
                new PushInt(5),
                new BitAnd(),
                new BitOr(),
                new LogAnd(),
                new LogOr(),
                new PushVar(Ref("b")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("a"),
                Ref("b"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestIntBitMoveOperator()
        {
            _code = @"
                var int a;
                var int b;
                var int c;
                var int d;

                func void testFunc() {
                    a = 1 << 2;
                    a = 1 >> 2;
                    a = 1 << b;
                    a = b >> 2;
                    a = c << d;
                    a = c >> d;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // a = 1 << 2;
                new PushInt(2),
                new PushInt(1),
                new ShiftLeft(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = 1 >> 2;
                new PushInt(2),
                new PushInt(1),
                new ShiftRight(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = 1 << b;
                new PushVar(Ref("b")),
                new PushInt(1),
                new ShiftLeft(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = b >> 2;
                new PushInt(2),
                new PushVar(Ref("b")),
                new ShiftRight(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = c << d;
                new PushVar(Ref("d")),
                new PushVar(Ref("c")),
                new ShiftLeft(),
                new PushVar(Ref("a")),
                new Assign(),

                // a = c >> d;
                new PushVar(Ref("d")),
                new PushVar(Ref("c")),
                new ShiftRight(),
                new PushVar(Ref("a")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("a"),
                Ref("b"),
                Ref("c"),
                Ref("d"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestIntCompOperator()
        {
            _code = @"
                var int a;
                var int b;
                var int c;
                var int d;

                func void testFunc() {
                    a = 1 < 2;
                    b = 1 <= 2;
                    c = 1 > 2;
                    d = 1 >= 2;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // a = 1 < 2;
                new PushInt(2),
                new PushInt(1),
                new Less(),
                new PushVar(Ref("a")),
                new Assign(),

                // b = 1 <= 2;
                new PushInt(2),
                new PushInt(1),
                new LessOrEqual(),
                new PushVar(Ref("b")),
                new Assign(),

                // c = 1 > 2;
                new PushInt(2),
                new PushInt(1),
                new Greater(),
                new PushVar(Ref("c")),
                new Assign(),

                // d = 1 >= 2;
                new PushInt(2),
                new PushInt(1),
                new GreaterOrEqual(),
                new PushVar(Ref("d")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("a"),
                Ref("b"),
                Ref("c"),
                Ref("d"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestIntComplexExpression()
        {
            _code = @"
                func int otherFunc(var int a, var int b)
                {
                    return 0;
                };

                var int x;

                func void testFunc() {
                    x = 12 + 9 * ( 2 + otherFunc(1 + 7 * 3, 4 + 5) );
                };
            ";

            _instructions = GetExecBlockInstructions("otherFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushVar(Ref("otherFunc.b")),
                new Assign(),
                new PushVar(Ref("otherFunc.a")),
                new Assign(),

                // return 0
                new PushInt(0),
                new Ret(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                new PushInt(3),
                new PushInt(7),
                new Multiply(),
                new PushInt(1),
                new Add(),
                new PushInt(5),
                new PushInt(4),
                new Add(),
                new Call(Ref("otherFunc")),
                new PushInt(2),
                new Add(),
                new PushInt(9),
                new Multiply(),
                new PushInt(12),
                new Add(),
                new PushVar(Ref("x")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("otherFunc"),
                Ref("otherFunc.a"),
                Ref("otherFunc.b"),
                Ref("x"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestIntArrElementExpression()
        {
            _code = @"
                var int x;
                var int tab[3];

                func void testFunc() {
                    x = 1;
                    tab[0] = 2;
                    tab[1] = 3;
                    tab[2] = x;
                    x = tab[0] + tab[1] * tab[2];
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // x = 1;
                new PushInt(1),
                new PushVar(Ref("x")),
                new Assign(),

                // tab[0] = 2;
                new PushInt(2),
                new PushVar(Ref("tab")),
                new Assign(),

                // tab[1] = 3;
                new PushInt(3),
                new PushArrayVar(Ref("tab"), 1),
                new Assign(),

                // tab[2] = x;
                new PushVar(Ref("x")),
                new PushArrayVar(Ref("tab"), 2),
                new Assign(),

                // x = tab[0] + tab[1] * tab[2];
                new PushArrayVar(Ref("tab"), 2),
                new PushArrayVar(Ref("tab"), 1),
                new Multiply(),
                new PushVar(Ref("tab")),
                new Add(),
                new PushVar(Ref("x")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("x"),
                Ref("tab"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }


        [Fact]
        public void TestIntArrElementWithGlobalConstIntIndexExpression()
        {
            _code = @"
                const int TAB_SIZE = 0 + 1 + 2;
                const int INDEX_ZERO = 0 - 0 + 0 * 0;
                const int INDEX_ONE = 0 + 1 - 0;
                const int INDEX_TWO = 1 + 1;
                var int x;
                var int tab[TAB_SIZE];

                func void testFunc() {
                    x = 1;
                    tab[INDEX_ZERO] = 2;
                    tab[INDEX_ONE] = 3;
                    tab[INDEX_TWO] = x;
                    x = tab[INDEX_ZERO] + tab[INDEX_ONE] * tab[INDEX_TWO];
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // x = 1;
                new PushInt(1),
                new PushVar(Ref("x")),
                new Assign(),

                // tab[INDEX_ZERO] = 2;
                new PushInt(2),
                new PushVar(Ref("tab")),
                new Assign(),

                // tab[INDEX_ONE] = 3;
                new PushInt(3),
                new PushArrayVar(Ref("tab"), 1),
                new Assign(),

                //  tab[INDEX_TWO] = x;
                new PushVar(Ref("x")),
                new PushArrayVar(Ref("tab"), 2),
                new Assign(),

                // x = tab[INDEX_ZERO] + tab[INDEX_ONE] * tab[INDEX_TWO];
                new PushArrayVar(Ref("tab"), 2),
                new PushArrayVar(Ref("tab"), 1),
                new Multiply(),
                new PushVar(Ref("tab")),
                new Add(),
                new PushVar(Ref("x")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("TAB_SIZE"),
                Ref("INDEX_ZERO"),
                Ref("INDEX_ONE"),
                Ref("INDEX_TWO"),
                Ref("x"),
                Ref("tab"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestIntArrElementWithLocalConstIntIndexExpression()
        {
            _code = @"
                const int TAB_SIZE = 3;
                var int x;
                var int tab[TAB_SIZE];

                func void testFunc() {
                    const int INDEX_ZERO = 0;
                    const int INDEX_ONE = 1;
                    const int INDEX_TWO = 2;
                    x = 1;
                    tab[INDEX_ZERO] = 2;
                    tab[INDEX_ONE] = 3;
                    tab[INDEX_TWO] = x;
                    x = tab[INDEX_ZERO] + tab[INDEX_ONE] * tab[INDEX_TWO];
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // x = 1;
                new PushInt(1),
                new PushVar(Ref("x")),
                new Assign(),

                // tab[INDEX_ZERO] = 2;
                new PushInt(2),
                new PushVar(Ref("tab")),
                new Assign(),

                // tab[INDEX_ONE] = 3;
                new PushInt(3),
                new PushArrayVar(Ref("tab"), 1),
                new Assign(),

                //  tab[INDEX_TWO] = x;
                new PushVar(Ref("x")),
                new PushArrayVar(Ref("tab"), 2),
                new Assign(),

                // x = tab[INDEX_ZERO] + tab[INDEX_ONE] * tab[INDEX_TWO];
                new PushArrayVar(Ref("tab"), 2),
                new PushArrayVar(Ref("tab"), 1),
                new Multiply(),
                new PushVar(Ref("tab")),
                new Add(),
                new PushVar(Ref("x")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("TAB_SIZE"),
                Ref("x"),
                Ref("tab"),
                Ref("testFunc"),
                Ref("testFunc.INDEX_ZERO"),
                Ref("testFunc.INDEX_ONE"),
                Ref("testFunc.INDEX_TWO"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestIntArrParameter()
        {
            _code = @"
                const int ATTRS_COUNT = 5;
                
                func void testFunc(var int x, var int tab[3], var float attrs[ATTRS_COUNT]) {
                    x = 1;
                    tab[0] = 2;
                    tab[1] = 3;
                    tab[2] = x;
                    x = tab[0] + tab[1] * tab[2];
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushVar(Ref("testFunc.attrs")),
                new AssignFloat(),
                new PushVar(Ref("testFunc.tab")),
                new Assign(),
                new PushVar(Ref("testFunc.x")),
                new Assign(),

                // x = 1;
                new PushInt(1),
                new PushVar(Ref("testFunc.x")),
                new Assign(),

                // tab[0] = 2;
                new PushInt(2),
                new PushVar(Ref("testFunc.tab")),
                new Assign(),

                // tab[1] = 3;
                new PushInt(3),
                new PushArrayVar(Ref("testFunc.tab"), 1),
                new Assign(),

                // tab[2] = x;
                new PushVar(Ref("testFunc.x")),
                new PushArrayVar(Ref("testFunc.tab"), 2),
                new Assign(),

                // x = tab[0] + tab[1] * tab[2];
                new PushArrayVar(Ref("testFunc.tab"), 2),
                new PushArrayVar(Ref("testFunc.tab"), 1),
                new Multiply(),
                new PushVar(Ref("testFunc.tab")),
                new Add(),
                new PushVar(Ref("testFunc.x")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("attrs_count"),
                Ref("testFunc"),
                Ref("testFunc.x"),
                Ref("testFunc.tab"),
                Ref("testFunc.attrs"),
            };
            AssertSymbolsMatch();
        }


        [Fact]
        public void TestMostOperatorsPrecedence()
        {
            _code = @"
                var int x;

                func void testFunc() {
                    x = +1 * -2 / !3 % ~4 + 5 - 6 << 7 >> 8 < 9 > 10 <= 11 >= 12 & 13 | 14 && 15 || 16;
                    x = 16 || 15 && 14 | 13 & 12 >= 11 <= 10 > 9 < 8 >> 7 << 6 - 5 + ~4 % !3 / -2 * +1;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // x = +1 * -2 / !3 % ~4 + 5 - 6 << 7 >> 8 < 9 > 10 <= 11 >= 12 & 13 | 14 && 15 || 16;
                new PushInt(16),
                new PushInt(15),
                new PushInt(14),
                new PushInt(13),
                new PushInt(12),
                new PushInt(11),
                new PushInt(10),
                new PushInt(9),
                new PushInt(8),
                new PushInt(7),
                new PushInt(6),
                new PushInt(5),
                new PushInt(4),
                new Negate(),
                new PushInt(3),
                new Not(),
                new PushInt(2),
                new Minus(),
                new PushInt(1),
                new Plus(),
                new Multiply(),
                new Divide(),
                new Modulo(),
                new Add(),
                new Subtract(),
                new ShiftLeft(),
                new ShiftRight(),
                new Less(),
                new Greater(),
                new LessOrEqual(),
                new GreaterOrEqual(),
                new BitAnd(),
                new BitOr(),
                new LogAnd(),
                new LogOr(),
                new PushVar(Ref("x")),
                new Assign(),

                // x = 16 || 15 && 14 | 13 & 12 >= 11 <= 10 > 9 < 8 >> 7 << 6 - 5 + ~4 % !3 / -2 * +1;
                new PushInt(1),
                new Plus(),
                new PushInt(2),
                new Minus(),
                new PushInt(3),
                new Not(),
                new PushInt(4),
                new Negate(),
                new Modulo(),
                new Divide(),
                new Multiply(),
                new PushInt(5),
                new PushInt(6),
                new Subtract(),
                new Add(),
                new PushInt(7),
                new PushInt(8),
                new ShiftRight(),
                new ShiftLeft(),
                new PushInt(9),
                new PushInt(10),
                new PushInt(11),
                new PushInt(12),
                new GreaterOrEqual(),
                new LessOrEqual(),
                new Greater(),
                new Less(),
                new PushInt(13),
                new BitAnd(),
                new PushInt(14),
                new BitOr(),
                new PushInt(15),
                new LogAnd(),
                new PushInt(16),
                new LogOr(),
                new PushVar(Ref("x")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("x"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestFuncClassParameterAndAttributesInSimpleExpressions()
        {
            _code = @"
                class person {
                    var int age;
                };
                
                var int a;

                func void testFunc(var person d) {
                    d.age = 5;
                    a = d.age;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushInstance(Ref("testFunc.d")),
                new AssignInstance(),

                // d.age = 5;
                new PushInt(5),
                new SetInstance(Ref("testFunc.d")),
                new PushVar(Ref("person.age")),
                new Assign(),

                // a = d.age;
                new SetInstance(Ref("testFunc.d")),
                new PushVar(Ref("person.age")),
                new PushVar(Ref("a")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("person"),
                Ref("person.age"),
                Ref("a"),
                Ref("testFunc"),
                Ref("testFunc.d"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestGlobalVarClassAndAttributesInSimpleExpressions()
        {
            _code = @"
                class person {
                    var int age;
                };
                
                var int a;
                var person d;

                func void testFunc() {
                    d.age = 5;
                    a = d.age;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // d.age = 5;
                new PushInt(5),
                new SetInstance(Ref("d")),
                new PushVar(Ref("person.age")),
                new Assign(),

                // a = d.age;
                new SetInstance(Ref("d")),
                new PushVar(Ref("person.age")),
                new PushVar(Ref("a")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("person"),
                Ref("person.age"),
                Ref("a"),
                Ref("d"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestLocalVarClassAndAttributesInSimpleExpressions()
        {
            _code = @"
                class person {
                    var int age;
                };
                
                var int a;

                func void testFunc() {
                    var person d;
                    d.age = 5;
                    a = d.age;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // d.age = 5;
                new PushInt(5),
                new SetInstance(Ref("testFunc.d")),
                new PushVar(Ref("person.age")),
                new Assign(),

                // a = d.age;
                new SetInstance(Ref("testFunc.d")),
                new PushVar(Ref("person.age")),
                new PushVar(Ref("a")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("person"),
                Ref("person.age"),
                Ref("a"),
                Ref("testFunc"),
                Ref("testFunc.d"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestInlineVarAndConstDeclarations()
        {
            _code = @"
                const int a = 1, b = 2, c = 3;
                var int d, e, f;
                
                func void testFunc(var int g) {
                    const int h = 4;
                    var int k;
                    d = 7;
                    e = 8;
                    f = 9;
                    k = 10;
                    g = 11;
                };
            ";

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushVar(Ref("testFunc.g")),
                new Assign(),

                // d = 7;
                new PushInt(7),
                new PushVar(Ref("d")),
                new Assign(),

                // e = 8;
                new PushInt(8),
                new PushVar(Ref("e")),
                new Assign(),

                // f = 9;
                new PushInt(9),
                new PushVar(Ref("f")),
                new Assign(),

                // k = 10;
                new PushInt(10),
                new PushVar(Ref("testFunc.k")),
                new Assign(),

                // g = 11;
                new PushInt(11),
                new PushVar(Ref("testFunc.g")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("a"),
                Ref("b"),
                Ref("c"),
                Ref("d"),
                Ref("e"),
                Ref("f"),
                Ref("testFunc"),
                Ref("testFunc.g"),
                Ref("testFunc.h"),
                Ref("testFunc.k"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestClassPrototypeInstanceInheritanceAndVarAndConstDeclarationsInsidePrototypeAndInstance()
        {
            _code = @"
                class Person {
                    var int age;
                };
                
                prototype Orc(Person) {
                    age = 10;
                    var int weight;
                    weight = 20;
                    const int HEIGHT = 100;
                };
                
                instance OrcShaman(Orc) {
                    age = 10;
                    var int mana;
                    mana = 30;
                    const int HP = 200;
                };
                
                var int a;
                
                func void testFunc()
                {
                    var person d;
                    d.age = 5;
                    a = d.age;
                };
            ";

            _instructions = GetExecBlockInstructions("Orc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // age = 10;
                new PushInt(10),
                new PushVar(Ref("Person.age")),
                new Assign(),

                // weight = 20;
                new PushInt(20),
                new PushVar(Ref("Orc.weight")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _instructions = GetExecBlockInstructions("OrcShaman");
            _expectedInstructions = new List<AssemblyElement>
            {
                new Call(Ref("Orc")), // only when parent is prototype

                // age = 10;
                new PushInt(10),
                new PushVar(Ref("Person.age")),
                new Assign(),

                // mana = 30;
                new PushInt(30),
                new PushVar(Ref("OrcShaman.mana")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // d.age = 5;
                new PushInt(5),
                new SetInstance(Ref("testFunc.d")),
                new PushVar(Ref("Person.age")),
                new Assign(),

                // a = d.age;
                new SetInstance(Ref("testFunc.d")),
                new PushVar(Ref("Person.age")),
                new PushVar(Ref("a")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("Person"),
                Ref("Person.age"),
                Ref("Orc"),
                Ref("Orc.weight"),
                Ref("Orc.height"),
                Ref("OrcShaman"),
                Ref("OrcShaman.mana"),
                Ref("OrcShaman.hp"),
                Ref("a"),
                Ref("testFunc"),
                Ref("testFunc.d"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestClassInstanceInheritanceAndVarAndConstDeclarationsInsidePrototypeAndInstance()
        {
            _code = @"
                class Person {
                    var int age;
                };

                instance OrcShaman(Person) {
                    age = 10;
                    var int mana;
                    mana = 30;
                    const int HP = 200;
                };
                
                var int a;
                
                func void testFunc()
                {
                    var person d;
                    d.age = 5;
                    a = d.age;
                };
            ";

            _instructions = GetExecBlockInstructions("OrcShaman");
            _expectedInstructions = new List<AssemblyElement>
            {
                // age = 10;
                new PushInt(10),
                new PushVar(Ref("Person.age")),
                new Assign(),

                // mana = 30;
                new PushInt(30),
                new PushVar(Ref("OrcShaman.mana")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // d.age = 5;
                new PushInt(5),
                new SetInstance(Ref("testFunc.d")),
                new PushVar(Ref("Person.age")),
                new Assign(),

                // a = d.age;
                new SetInstance(Ref("testFunc.d")),
                new PushVar(Ref("Person.age")),
                new PushVar(Ref("a")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("Person"),
                Ref("Person.age"),
                Ref("OrcShaman"),
                Ref("OrcShaman.mana"),
                Ref("OrcShaman.hp"),
                Ref("a"),
                Ref("testFunc"),
                Ref("testFunc.d"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestFloatExpressions()
        {
            _code = @"
                var float x;

                func float otherFunc() {};                
                
                const float a = -10;
                const float b = +20;

                func void testFunc(var float y) {
                    const float c = -12.5;
                    y = 5.5;
                    x = -1.5;
                    x = -1;
                    x = 0;
                    x = 1;
                    x = 1.5;
                    x = +2.5;
                    x = 3.5;
                };
            ";

            _instructions = GetExecBlockInstructions("otherFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                new Ret(),
            };
            AssertInstructionsMatch();

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushVar(Ref("testFunc.y")),
                new AssignFloat(),

                // y = 5.5;
                new PushInt(1085276160),
                new PushVar(Ref("testFunc.y")),
                new AssignFloat(),

                // x = -1.5;
                new PushInt(-1077936128),
                new PushVar(Ref("x")),
                new AssignFloat(),

                // x = -1;
                new PushInt(-1082130432),
                new PushVar(Ref("x")),
                new AssignFloat(),

                // x = 0;
                new PushInt(0),
                new PushVar(Ref("x")),
                new AssignFloat(),

                // x = 1;
                new PushInt(1065353216),
                new PushVar(Ref("x")),
                new AssignFloat(),

                // x = 1.5;
                new PushInt(1069547520),
                new PushVar(Ref("x")),
                new AssignFloat(),

                // x = 2.5;
                new PushInt(1075838976),
                new PushVar(Ref("x")),
                new AssignFloat(),

                // x = 3.5;
                new PushInt(1080033280),
                new PushVar(Ref("x")),
                new AssignFloat(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("x"),
                Ref("otherFunc"),
                Ref("a"),
                Ref("b"),
                Ref("testFunc"),
                Ref("testFunc.y"),
                Ref("testFunc.c"),
            };
            AssertSymbolsMatch();

            AssertRefContentEqual("a", -10.0f);
            AssertRefContentEqual("b", 20.0f);
            AssertRefContentEqual("testFunc.c", -12.5f);
        }

        [Fact]
        public void TestStringExpressions()
        {
            _code = @"
                const string hyzio = ""Hyzio"";
                const string dyzio = ""Dyzio"";
                const string zyzio = ""Zyzio"";
    
                var string lech;
    
                func string someFunc() {
                    return lech;
                };
    
                func string otherFunc(var string john) {
                    return ""Dyzio"";
                };
    
                func string anotherFunc() {
                    return zyzio;
                };
    
                func void testFunc(var string czech) {
                    var string rus;
                    lech = ""Lech"";
        
                    czech = ""Czech"";
                    czech = ""Dyzio"";
        
                    rus = ""Rus"";
        
                    const string hyzio_clone = ""Hyzio"";
                    hyzio_clone = hyzio;
                    hyzio_clone = ""Hyzio"";
                    hyzio_clone = ""Hyzio"";
        
                    var string lech_clone;
                    lech_clone = ""Lech"";
                    lech_clone = otherFunc(""John"");
                };
            ";

            char prefix = (char) 255;

            _instructions = GetExecBlockInstructions("someFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // return lech;
                new PushVar(Ref("lech")),
                new Ret(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _instructions = GetExecBlockInstructions("otherFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushVar(Ref("otherFunc.john")),
                new AssignString(),

                // return "Dyzio";
                new PushVar(Ref($"{prefix}10000")),
                new Ret(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _instructions = GetExecBlockInstructions("anotherFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // return zyzio;
                new PushVar(Ref("zyzio")),
                new Ret(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushVar(Ref("testFunc.czech")),
                new AssignString(),

                // lech = "Lech";
                new PushVar(Ref($"{prefix}10001")),
                new PushVar(Ref("lech")),
                new AssignString(),

                // czech = "Czech";
                new PushVar(Ref($"{prefix}10002")),
                new PushVar(Ref("testFunc.czech")),
                new AssignString(),

                // czech = "Dyzio";
                new PushVar(Ref($"{prefix}10003")),
                new PushVar(Ref("testFunc.czech")),
                new AssignString(),

                // rus = "Rus";
                new PushVar(Ref($"{prefix}10004")),
                new PushVar(Ref("testFunc.rus")),
                new AssignString(),

                // hyzio_clone = hyzio;
                new PushVar(Ref("hyzio")),
                new PushVar(Ref("testFunc.hyzio_clone")),
                new AssignString(),

                // hyzio_clone = "Hyzio";
                new PushVar(Ref($"{prefix}10005")),
                new PushVar(Ref("testFunc.hyzio_clone")),
                new AssignString(),

                // hyzio_clone = "Hyzio";
                new PushVar(Ref($"{prefix}10006")),
                new PushVar(Ref("testFunc.hyzio_clone")),
                new AssignString(),

                // lech_clone = "Lech";
                new PushVar(Ref($"{prefix}10007")),
                new PushVar(Ref("testFunc.lech_clone")),
                new AssignString(),

                // lech_clone = otherFunc("John");
                new PushVar(Ref($"{prefix}10008")),
                new Call(Ref("otherFunc")),
                new PushVar(Ref("testFunc.lech_clone")),
                new AssignString(),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("hyzio"),
                Ref("dyzio"),
                Ref("zyzio"),
                Ref("lech"),
                Ref("someFunc"),
                Ref("otherFunc"),
                Ref("otherFunc.john"),
                Ref("anotherFunc"),
                Ref("testFunc"),
                Ref("testFunc.czech"),
                Ref("testFunc.rus"),
                Ref("testFunc.hyzio_clone"),
                Ref("testFunc.lech_clone"),
                Ref($"{prefix}10000"),
                Ref($"{prefix}10001"),
                Ref($"{prefix}10002"),
                Ref($"{prefix}10003"),
                Ref($"{prefix}10004"),
                Ref($"{prefix}10005"),
                Ref($"{prefix}10006"),
                Ref($"{prefix}10007"),
                Ref($"{prefix}10008"),
            };
            AssertSymbolsMatch();

            AssertRefContentEqual("hyzio", "Hyzio");
            AssertRefContentEqual("dyzio", "Dyzio");
            AssertRefContentEqual("zyzio", "Zyzio");
            AssertRefContentEqual($"{prefix}10000", "Dyzio");
            AssertRefContentEqual($"{prefix}10001", "Lech");
            AssertRefContentEqual($"{prefix}10002", "Czech");
            AssertRefContentEqual($"{prefix}10003", "Dyzio");
            AssertRefContentEqual($"{prefix}10004", "Rus");
            AssertRefContentEqual($"{prefix}10005", "Hyzio");
            AssertRefContentEqual($"{prefix}10006", "Hyzio");
            AssertRefContentEqual($"{prefix}10007", "Lech");
            AssertRefContentEqual($"{prefix}10008", "John");
        }

        [Fact]
        public void TestIfInstruction()
        {
            _code = @"
                var int a;

                func void testFunc()
                {
                    if ( 1 < 2 ) {
                        a = 5;
                        a = 2 * 2;
                    };
                };
            ";
            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // if ( 1 < 2 )
                new PushInt(2),
                new PushInt(1),
                new Less(),
                new JumpIfToLabel("label_0"),
                // a = 5;
                new PushInt(5),
                new PushVar(Ref("a")),
                new Assign(),
                // a = 2 * 2;
                new PushInt(2),
                new PushInt(2),
                new Multiply(),
                new PushVar(Ref("a")),
                new Assign(),
                // if end
                new AssemblyLabel("label_0"),
                new Ret()
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("a"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestIfAndElseInstruction()
        {
            _code = @"
                var int a;

                func void testFunc()
                {
                    if ( 1 < 2 ) {
                        a = 3;
                    } else {
                        a = 4;
                    };
                };
            ";
            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // if ( 1 < 2 )
                new PushInt(2),
                new PushInt(1),
                new Less(),
                new JumpIfToLabel("label_1"),
                // a = 3;
                new PushInt(3),
                new PushVar(Ref("a")),
                new Assign(),
                new JumpToLabel("label_0"),
                // if end
                // else start
                new AssemblyLabel("label_1"),
                // a = 4;
                new PushInt(4),
                new PushVar(Ref("a")),
                new Assign(),
                // else and if end
                new AssemblyLabel("label_0"),
                new Ret()
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("a"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }

        [Fact]
        public void TestIfElseInstruction()
        {
            _code = @"
                var int a;

                func void testFunc()
                {
                        if ( 1 < 2 ) {
                            a = 3;
                        } else if ( 4 < 5 ) {
                            a = 6;
                        } else {
                            a = 7;
                        };
                };
            ";
            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // if ( 1 < 2 )
                new PushInt(2),
                new PushInt(1),
                new Less(),
                new JumpIfToLabel("label_1"),
                // a = 3;
                new PushInt(3),
                new PushVar(Ref("a")),
                new Assign(),
                new JumpToLabel("label_0"),
                // if end
                new AssemblyLabel("label_1"),
                // else if ( 4 < 5 )
                new PushInt(5),
                new PushInt(4),
                new Less(),
                new JumpIfToLabel("label_2"),
                // a = 6;
                new PushInt(6),
                new PushVar(Ref("a")),
                new Assign(),
                new JumpToLabel("label_0"),
                // else if end
                // else start
                new AssemblyLabel("label_2"),
                //a = 7;
                new PushInt(7),
                new PushVar(Ref("a")),
                new Assign(),
                // else end if end
                new AssemblyLabel("label_0"),
                new Ret()
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("a"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch();
        }
        [Fact]
        public void TestFuncCalls()
        {
            _code = @"
                class person {
                    var int age;
                };
                
                func void firstFunc (var person par) {};
                func void secondFunc (var int par) {};
                func void thirdFunc (var float par) {};
                func void fourthFunc (var string par) {};
                
                var person a;
                var int b;
                var float c;
                var string d;
                
                func void testFunc () {
                    var person e;
                    var int f;
                    var float g;
                    var string h;
                    
                    firstFunc(a);
                    //firstFunc(b);
                    //firstFunc(c);
                    //firstFunc(d);
                    firstFunc(e);
                    //firstFunc(f);
                    //firstFunc(g);
                    //firstFunc(h);
                        
                    secondFunc(a);
                    secondFunc(b);
                    //secondFunc(c);
                    //secondFunc(d);
                    secondFunc(e);
                    secondFunc(f);
                    //secondFunc(g);
                    //secondFunc(h);
                    
                    
                    //thirdFunc(a);
                    //thirdFunc(b);
                    thirdFunc(c);
                    //thirdFunc(d);
                    //thirdFunc(e);
                    //thirdFunc(f);
                    thirdFunc(g);
                    //thirdFunc(h);
                    
                    //fourthFunc(a);
                    //fourthFunc(b);
                    //fourthFunc(c);
                    fourthFunc(d);
                    //fourthFunc(e);
                    //fourthFunc(f);
                    //fourthFunc(g);
                    fourthFunc(h);
                    
                };
            ";
            
            _instructions = GetExecBlockInstructions("firstFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushInstance(Ref("firstFunc.par")),
                new AssignInstance(),
                new Ret(),
            };
            AssertInstructionsMatch();
            
            _instructions = GetExecBlockInstructions("secondFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushVar(Ref("secondFunc.par")),
                new Assign(),
                new Ret(),
            };
            AssertInstructionsMatch();
            
            _instructions = GetExecBlockInstructions("thirdFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushVar(Ref("thirdFunc.par")),
                new AssignFloat(),
                new Ret(),
            };
            AssertInstructionsMatch();
            
            _instructions = GetExecBlockInstructions("fourthFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushVar(Ref("fourthFunc.par")),
                new AssignString(),
                new Ret(),
            };
            AssertInstructionsMatch();
            
            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // firstFunc(a);
                new PushInstance(Ref("a")),
                new Call(Ref("firstFunc")),
                
                // firstFunc(e);
                new PushInstance(Ref("testFunc.e")),
                new Call(Ref("firstFunc")),
                
                // secondFunc(a);
                new PushInt(RefIndex("a")),
                new Call(Ref("secondFunc")),
                
                // secondFunc(b);
                new PushVar(Ref("b")),
                new Call(Ref("secondFunc")),
                
                // secondFunc(e);
                new PushInt(RefIndex("testFunc.e")),
                new Call(Ref("secondFunc")),
                
                // secondFunc(f);
                new PushVar(Ref("testFunc.f")),
                new Call(Ref("secondFunc")),
                
                // thirdFunc(c);
                new PushVar(Ref("c")),
                new Call(Ref("thirdFunc")),
                
                // thirdFunc(c);
                new PushVar(Ref("testFunc.g")),
                new Call(Ref("thirdFunc")),
                
                // fourthFunc(d);
                new PushVar(Ref("d")),
                new Call(Ref("fourthFunc")),
                
                // fourthFunc(h);
                new PushVar(Ref("testFunc.h")),
                new Call(Ref("fourthFunc")),
                
                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("person"),
                Ref("person.age"),
                Ref("firstFunc"),
                Ref("firstFunc.par"),
                Ref("secondFunc"),
                Ref("secondFunc.par"),
                Ref("thirdFunc"),
                Ref("thirdFunc.par"),
                Ref("fourthFunc"),
                Ref("fourthFunc.par"),
                Ref("a"),
                Ref("b"),
                Ref("c"),
                Ref("d"),
                Ref("testFunc"),
                Ref("testFunc.e"),
                Ref("testFunc.f"),
                Ref("testFunc.g"),
                Ref("testFunc.h"),
            };
            AssertSymbolsMatch();
        }
        
        [Fact]
        public void TestMultiparameterFuncCall()
        {
            _code = @"
                class person {
                    var int age;
                };
                
                var person a;
                var int b;
                var float c;
                var string d;
                
                
                func void firstFunc (var person par0, var int par1, var float par2, var string par3, var person par4) {};
                
                func void testFunc () {
                    firstFunc(a, a, c, d, a);
                    firstFunc(a, b, c, d, a);
                };
            ";
            
            _instructions = GetExecBlockInstructions("firstFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushInstance(Ref("firstFunc.par4")),
                new AssignInstance(),
                
                new PushVar(Ref("firstFunc.par3")),
                new AssignString(),
                
                new PushVar(Ref("firstFunc.par2")),
                new AssignFloat(),
                
                new PushVar(Ref("firstFunc.par1")),
                new Assign(),
                
                new PushInstance(Ref("firstFunc.par0")),
                new AssignInstance(),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            
            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // firstFunc(a, a, c, d, a);
                new PushInstance(Ref("a")),
                new PushInt(RefIndex("a")),
                new PushVar(Ref("c")),
                new PushVar(Ref("d")),
                new PushInstance(Ref("a")),
                new Call(Ref("firstFunc")),
                
                // firstFunc(a, b, c, d, a);
                new PushInstance(Ref("a")),
                new PushVar(Ref("b")),
                new PushVar(Ref("c")),
                new PushVar(Ref("d")),
                new PushInstance(Ref("a")),
                new Call(Ref("firstFunc")),
                
                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("person"),
                Ref("person.age"),
                Ref("a"),
                Ref("b"),
                Ref("c"),
                Ref("d"),
                Ref("firstFunc"),
                Ref("firstFunc.par0"),
                Ref("firstFunc.par1"),
                Ref("firstFunc.par2"),
                Ref("firstFunc.par3"),
                Ref("firstFunc.par4"),
                Ref("testFunc"),
            };
            AssertSymbolsMatch(); 
        }
        
        [Fact]
        public void TestLazyReferenceFunctionCall()
        {
            _code = @"
                class person {
                    var int age;
                };
                
                func void firstFunc (var int par) {};
                func void secondFunc (var person par) {};
    
                func void testFunc () {
                    firstFunc(a);
                    firstFunc(b);
                    firstFunc(8);

                    secondFunc(a);
                };
                
                var person a;
                var int b;
            ";
            
            _instructions = GetExecBlockInstructions("firstFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushVar(Ref("firstFunc.par")),
                new Assign(),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            
            _instructions = GetExecBlockInstructions("secondFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushInstance(Ref("secondFunc.par")),
                new AssignInstance(),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            
            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // firstFunc(a);
                new PushInt(RefIndex("a")),
                new Call(Ref("firstFunc")),
                
                // firstFunc(b);
                new PushVar(Ref("b")),
                new Call(Ref("firstFunc")),
                
                // firstFunc(8);
                new PushInt(8),
                new Call(Ref("firstFunc")),
                
                // secondFunc(a);
                new PushInstance(Ref("a")),
                new Call(Ref("secondFunc")),
                
                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("person"),
                Ref("person.age"),
                Ref("firstFunc"),
                Ref("firstFunc.par"),
                Ref("secondFunc"),
                Ref("secondFunc.par"),
                Ref("testFunc"),
                Ref("a"),
                Ref("b"),
            };
            AssertSymbolsMatch(); 
        }
        
        
        
        [Fact]
        public void TestLazyReferenceExternalFunctionCall()
        {
            _externalCode = @"
                func int NPC_HasNews(var instance par0, var int par1, var instance par2, var instance par3) {};
            ";
            _code = @"
                class C_NPC { var int data [200]; };

                func int firstFunc(var C_NPC par0, var C_NPC par1, var C_NPC par2) {};

                func int testFunc()
                {
                    firstFunc(person, person, person);
                    NPC_HasNews(person, person, person, person);
                };
                
                instance person(C_NPC);
            ";
            
            _instructions = GetExecBlockInstructions("firstFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushInstance(Ref("firstFunc.par2")),
                new AssignInstance(),
                
                new PushInstance(Ref("firstFunc.par1")),
                new AssignInstance(),
                
                new PushInstance(Ref("firstFunc.par0")),
                new AssignInstance(),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            
            
            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // firstFunc(person, person, person);
                new PushInstance(Ref("person")),
                new PushInstance(Ref("person")),
                new PushInstance(Ref("person")),
                new Call(Ref("firstFunc")),
                
                // NPC_HasNews(person, person, person, person);
                new PushInstance(Ref("person")),
                new PushInt(RefIndex("person")),
                new PushInstance(Ref("person")),
                new PushInstance(Ref("person")),
                new CallExternal(Ref("NPC_HasNews")),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("NPC_HasNews"),
                Ref("NPC_HasNews.par0"),
                Ref("NPC_HasNews.par1"),
                Ref("NPC_HasNews.par2"),
                Ref("NPC_HasNews.par3"),
                
                Ref("C_NPC"),
                Ref("C_NPC.data"),
                Ref("firstFunc"),
                Ref("firstFunc.par0"),
                Ref("firstFunc.par1"),
                Ref("firstFunc.par2"),
                Ref("testFunc"),
                Ref("person"),
            };
            AssertSymbolsMatch(); 
        }
        
        
        [Fact]
        public void TestLazyReferenceAssignFunctionCall()
        {
            _code = @"
                class person {
                    var int age;
                };
                func int firstFunc (var int par) {
                    return par;
                };
                func int secondFunc (var person par) {
                    return par;
                };
                
                func void testFunc () {
                    var int c;
                
                    c = firstFunc(a);
                    c = firstFunc(b);
                    c = firstFunc(8);
                    
                    c = secondFunc(a);
                };
                
                var person a;
                var int b;
            ";
            
            _instructions = GetExecBlockInstructions("firstFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushVar(Ref("firstFunc.par")),
                new Assign(),
                
                // return par;
                new PushVar(Ref("firstFunc.par")),
                new Ret(),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            
            /* TODO not working but probably never happens in Gothic code
            _instructions = GetExecBlockInstructions("secondFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushInstance(Ref("secondFunc.par")),
                new AssignInstance(),
                
                // return par;
                new PushInt(RefIndex("secondFunc.par")),
                new Ret(),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            */
            
            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // c = firstFunc(a);
                new PushInt(RefIndex("a")),
                new Call(Ref("firstFunc")),
                new PushVar(Ref("testFunc.c")),
                new Assign(),
                
                // c = firstFunc(b);
                new PushVar(Ref("b")),
                new Call(Ref("firstFunc")),
                new PushVar(Ref("testFunc.c")),
                new Assign(),
                
                // c = firstFunc(8);
                new PushInt(8),
                new Call(Ref("firstFunc")),
                new PushVar(Ref("testFunc.c")),
                new Assign(),
                
                // c = secondFunc(a);
                new PushInstance(Ref("a")),
                new Call(Ref("secondFunc")),
                new PushVar(Ref("testFunc.c")),
                new Assign(),
                
                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("person"),
                Ref("person.age"),
                Ref("firstFunc"),
                Ref("firstFunc.par"),
                Ref("secondFunc"),
                Ref("secondFunc.par"),
                Ref("testFunc"),
                Ref("testFunc.c"),
                Ref("a"),
                Ref("b"),
            };
            AssertSymbolsMatch(); 
        }
        [Fact]
        public void TestLazyReferenceAssignIntString()
        {
            _code = @"
                func void testFunc () {
                    var int e;
                    var string f;
                    
                    e = a;
                    f = b;
                    
                    e = c;
                    f = d;
                };
                
                const int a = 1;
                const string b = ""super"";
                
                var int c;
                var string d;
            ";
            
           
            
            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // e = a;
                new PushVar(Ref("a")),
                new PushVar(Ref("testFunc.e")),
                new Assign(),
                
                // f = b;
                new PushVar(Ref("b")),
                new PushVar(Ref("testFunc.f")),
                new AssignString(),
                
                // e = c;
                new PushVar(Ref("c")),
                new PushVar(Ref("testFunc.e")),
                new Assign(),
                
                // f = d;
                new PushVar(Ref("d")),
                new PushVar(Ref("testFunc.f")),
                new AssignString(),
                
                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("testFunc"),
                Ref("testFunc.e"),
                Ref("testFunc.f"),
                Ref("a"),
                Ref("b"),
                Ref("c"),
                Ref("d"),
            };
            AssertSymbolsMatch(); 
        }
        
        [Fact]
        public void TestLazyReferenceInsideIfCondition()
        {
            _code = @"
                func int intFunc(var int par) {
                    return 0;
                };
                
                func void testFunc () {
                    var int c;
                    
                    if (intFunc(d)) {
                        c = 0;
                    }else if (d == a) {
                        c = 1;
                    } else if (d == b) {
                        c = 2;
                    } else if (d == 100) {
                        c = 3;
                    } else {
                        c = d;
                    };
                
                    var int d;
                };
                
                const int a = 1;
                var int b;
            ";
            
            
            _instructions = GetExecBlockInstructions("intFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushVar(Ref("intFunc.par")),
                new Assign(),
                
                // return 0;
                new PushInt(0),
                new Ret(),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            
            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // if(intFunc(d))
                new PushVar(Ref("testFunc.d")),
                new Call(Ref("intFunc")),
                new JumpIfToLabel("label_1"),

                // c = 0;
                new PushInt(0),
                new PushVar(Ref("testFunc.c")),
                new Assign(),
                new JumpToLabel("label_0"),
                
                // else if(d == a)
                new AssemblyLabel("label_1"),
                new PushVar(Ref("a")),
                new PushVar(Ref("testFunc.d")),
                new Equal(),
                new JumpIfToLabel("label_2"),
                
                // c = 1;
                new PushInt(1),
                new PushVar(Ref("testFunc.c")),
                new Assign(),
                new JumpToLabel("label_0"),
                
                // else if(d == b)
                new AssemblyLabel("label_2"),
                new PushVar(Ref("b")),
                new PushVar(Ref("testFunc.d")),
                new Equal(),
                new JumpIfToLabel("label_3"),
                
                // c = 2;
                new PushInt(2),
                new PushVar(Ref("testFunc.c")),
                new Assign(),
                new JumpToLabel("label_0"),
                
                // else if(d == 100)
                new AssemblyLabel("label_3"),
                new PushInt(100),
                new PushVar(Ref("testFunc.d")),
                new Equal(),
                new JumpIfToLabel("label_4"),
                
                // c = 3;
                new PushInt(3),
                new PushVar(Ref("testFunc.c")),
                new Assign(),
                new JumpToLabel("label_0"),
                
                // else
                new AssemblyLabel("label_4"),
                
                // c = d;
                new PushVar(Ref("testFunc.d")),
                new PushVar(Ref("testFunc.c")),
                new Assign(),
                
                new AssemblyLabel("label_0"),
                
                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("intFunc"),
                Ref("intFunc.par"),
                Ref("testFunc"),
                Ref("testFunc.c"),
                Ref("testFunc.d"),
                Ref("a"),
                Ref("b"),
            };
            AssertSymbolsMatch(); 
        }
        
        
        [Fact]
        public void TestLazyReferenceReturn()
        {
            _code = @"
                class person {
                    var int age;
                };
                
                func int firstFunc() {
                    return a;
                };
                
                func string secondFunc() {
                    return b;
                };
                
                func int thirdFunc() {
                    return c;
                };
                
                var int a;
                var string b;
                instance c(person);
            ";
            
            _instructions = GetExecBlockInstructions("firstFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // return a;
                new PushVar(Ref("a")),
                new Ret(),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            
            _instructions = GetExecBlockInstructions("secondFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // return b;
                new PushVar(Ref("b")),
                new Ret(),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            
            _instructions = GetExecBlockInstructions("thirdFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // return c;
                new PushInt(RefIndex("c")),
                new Ret(),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            
            
            _expectedSymbols = new List<DatSymbol>
            {
                Ref("person"),
                Ref("person.age"),
                Ref("firstFunc"),
                Ref("secondFunc"),
                Ref("thirdFunc"),
                Ref("a"),
                Ref("b"),
                Ref("c"),
            };
            AssertSymbolsMatch(); 
        }
        
        [Fact]
        public void TestLazyReferenceInsideComplexIfCondition()
        {
            _externalCode = @"
                func int NPC_HasItems(var instance par0, var int par1) {};
            ";
            _code = @"
                class C_NPC { var int data [200]; };

                func int testFunc()
                {
                    var int newWeapon;
                    
                    if (NPC_HasItems(person, sword) >= 1)
                    {
                        return sword;
                    };
                    if ( (oldWeapon == axe) || (oldWeapon == sword) )
                    {
                        newWeapon = 0;
                    };
                    var int oldWeapon;
                };
                instance person(C_NPC);
                instance sword (C_NPC){};
                instance axe (C_NPC){};
            ";
            
            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // if (NPC_HasItems(person, sword) >= 1)
                new PushInt(1),
                new PushInstance(Ref("person")),
                new PushInt(RefIndex("sword")),
                new CallExternal(Ref("NPC_HasItems")),
                new GreaterOrEqual(),
                new JumpIfToLabel("label_0"),
                
                // return sword;
                new PushInt(RefIndex("sword")),
                new Ret(),
                
                // endif
                new AssemblyLabel("label_0"),
                
                // if ( (oldWeapon == axe) || (oldWeapon == sword) )
                new PushInt(RefIndex("sword")),
                new PushVar(Ref("testFunc.oldWeapon")),
                new Equal(),
                new PushInt(RefIndex("axe")),
                new PushVar(Ref("testFunc.oldWeapon")),
                new Equal(),
                new LogOr(),
                new JumpIfToLabel("label_1"),
                
                // newWeapon = 0;
                new PushInt(0),
                new PushVar(Ref("testFunc.newWeapon")),
                new Assign(),
                
                //endif
                new AssemblyLabel("label_1"),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            
            
            _expectedSymbols = new List<DatSymbol>
            {
                Ref("NPC_HasItems"),
                Ref("NPC_HasItems.par0"),
                Ref("NPC_HasItems.par1"),
                
                Ref("C_NPC"),
                Ref("C_NPC.data"),
                Ref("testFunc"),
                Ref("testFunc.newWeapon"),
                Ref("testFunc.oldWeapon"),
                Ref("person"),
                Ref("sword"),
                Ref("axe"),

            };
            AssertSymbolsMatch(); 
        }
        
        
        [Fact]
        public void TestConstFunc()
        {
            _code = @"
                func int intFunc(var int par) {
                    return 0;
                };
                func void voidFunc() {};
                
                const func constIntFunc = intFunc; 
                const func constVoidFunc = voidFunc;
            ";
            ParseData();
            
            _expectedSymbols = new List<DatSymbol>
            {
                Ref("intFunc"),
                Ref("intFunc.par"),
                Ref("voidFunc"),
                Ref("constIntFunc"),
                Ref("constVoidFunc"),
            };
            AssertSymbolsMatch(); 
        }
        
        [Fact]
        public void TestExternalFunc()
        {
            _externalCode = @"
                func float IntToFloat(var int par0) {};
            ";
            _code = @"
                func float floatFunc() {};

                func void testFunc() {
                    var int wait;
                    var float waitTime;
                    waitTime = 2.5;
                    waitTime = floatFunc();
                    waitTime = IntToFloat(wait);
                };
            ";
            
            _instructions = GetExecBlockInstructions("testFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // waitTime = 2.5;
                new PushInt(1075838976),
                new PushVar(Ref("testFunc.waitTime")),
                new AssignFloat(),
                
                // waitTime = floatFunc();
                new Call(Ref("floatFunc")),
                new PushVar(Ref("testFunc.waitTime")),
                new AssignFloat(),
                
                // waitTime = IntToFloat(wait);
                new PushVar(Ref("testFunc.wait")),
                new CallExternal(Ref("IntToFloat")),
                new PushVar(Ref("testFunc.waitTime")),
                new AssignFloat(),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            
            
            _expectedSymbols = new List<DatSymbol>
            {
                Ref("IntToFloat"),
                Ref("IntToFloat.par0"),
                Ref("floatFunc"),
                Ref("testFunc"),
                Ref("testFunc.wait"),
                Ref("testFunc.waitTime"),
            };
            AssertSymbolsMatch(); 
        }
        
        
        [Fact]
        public void TestKeywordsSelfAndSlf()
        {
            _externalCode = @"
                func int NPC_IsPlayer(var instance par0) {};
                func void WLD_PlayEffect(var string par0, var instance par1, var instance par2, var int par3, var int par4, var int par5, var int par6) {};
                func void NPC_ChangeAttribute(var instance par0, var int par1, var int par2) {};
                func void CreateInvItems(var instance par0, var int par1, var int par2) {};
                
            ";
            _code = @"
                const int ATR_STRENGTH =  4;
                const int ATR_DEXTERITY =  5;
                const int ATR_INDEX_MAX	=  8;
                
                class C_NPC 
                {	
                    var int attribute[ATR_INDEX_MAX];
                };		
                
                prototype NPC_Default (C_NPC)
                {
                    attribute[ATR_STRENGTH] = 10;
                    attribute[ATR_DEXTERITY] = 20;
                };
                
                instance self(C_NPC);
                instance sword(C_NPC);
                
                func void useJoint()
                {
                    if (NPC_IsPlayer (self))
                    {
                        WLD_PlayEffect(""SLOW_TIME"", self, self, 0, 0, 0, 0);
                    };
                };
                
                func void gainStrength(var C_NPC slf, var int spell, var int mana)
                {
                    if (slf.attribute[ATR_STRENGTH] < 10)
                    {
                        NPC_ChangeAttribute(slf, ATR_STRENGTH, 10);
                    };
                    NPC_ChangeAttribute(slf, ATR_STRENGTH, ATR_STRENGTH + 1);
                };
                
                instance Geralt (NPC_Default)
                {
                    slf.attribute[ATR_STRENGTH] = 10;
                    self.attribute[ATR_DEXTERITY] = 10;
                                                                
                    // CreateInvItems (slf, sword, 1); // cannot use slf alone
                    CreateInvItems(self, sword, 2);
                    gainStrength(self, slf.attribute[ATR_STRENGTH], self.attribute[ATR_DEXTERITY]);
                };
            ";
            char prefix = (char) 255;
            
            _instructions = GetExecBlockInstructions("NPC_Default");
            _expectedInstructions = new List<AssemblyElement>
            {
                // attribute[ATR_STRENGTH] = 10;
                new PushInt(10),
                new PushArrayVar(Ref("C_NPC.attribute"), 4),
                new Assign(),
                
                // attribute[ATR_DEXTERITY] = 20;
                new PushInt(20),
                new PushArrayVar(Ref("C_NPC.attribute"), 5),
                new Assign(),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            
            _instructions = GetExecBlockInstructions("useJoint");
            _expectedInstructions = new List<AssemblyElement>
            {
                // if (NPC_IsPlayer (self))
                new PushInstance(Ref("self")),
                new CallExternal(Ref("NPC_IsPlayer")),
                new JumpIfToLabel("label_0"),
                
                // WLD_PlayEffect(""SLOW_TIME"", self, self, 0, 0, 0, 0);
                new PushVar(Ref($"{prefix}10000")),
                new PushInstance(Ref("self")),
                new PushInstance(Ref("self")),
                new PushInt(0),
                new PushInt(0),
                new PushInt(0),
                new PushInt(0),
                new CallExternal(Ref("WLD_PlayEffect")),
                
                // endif
                new AssemblyLabel("label_0"),
                new Ret(),
            };
            AssertInstructionsMatch();
            
            _instructions = GetExecBlockInstructions("gainStrength");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new PushVar(Ref("gainStrength.mana")),
                new Assign(),
                new PushVar(Ref("gainStrength.spell")),
                new Assign(),
                new PushInstance(Ref("gainStrength.slf")),
                new AssignInstance(),
                
                // if (slf.attribute[ATR_STRENGTH] < 10)
                new PushInt(10),
                new SetInstance(Ref("gainStrength.slf")),
                new PushArrayVar(Ref("C_NPC.attribute"), 4),
                new Less(),
                new JumpIfToLabel("label_1"),
                
                // NPC_ChangeAttribute(slf, ATR_STRENGTH, 10);
                new PushInstance(Ref("gainStrength.slf")),
                new PushVar(Ref("ATR_STRENGTH")),
                new PushInt(10),
                new CallExternal(Ref("NPC_ChangeAttribute")),
                
                // endif
                new AssemblyLabel("label_1"),
                
                // NPC_ChangeAttribute(slf, ATR_STRENGTH, ATR_STRENGTH + 1);
                new PushInstance(Ref("gainStrength.slf")),
                new PushVar(Ref("ATR_STRENGTH")),
                new PushInt(1),
                new PushVar(Ref("ATR_STRENGTH")),
                new Add(),
                new CallExternal(Ref("NPC_ChangeAttribute")),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            
            _instructions = GetExecBlockInstructions("Geralt");
            _expectedInstructions = new List<AssemblyElement>
            {
                // parameters
                new Call(Ref("NPC_Default")),
                
                // slf.attribute[ATR_STRENGTH] = 10;
                new PushInt(10),
                new PushArrayVar(Ref("C_NPC.attribute"), 4),
                new Assign(),
                
                // self.attribute[ATR_DEXTERITY] = 10;
                new PushInt(10),
                new PushArrayVar(Ref("C_NPC.attribute"), 5),
                new Assign(),
                                                            
                // CreateInvItems(self, sword, 2);
                new PushInstance(Ref("Geralt")),
                new PushInt(RefIndex("sword")),
                new PushInt(2),
                new CallExternal(Ref("CreateInvItems")),
                
                // gainStrength(self, slf.attribute[ATR_STRENGTH], self.attribute[ATR_DEXTERITY]);
                new PushInstance(Ref("Geralt")),
                new PushArrayVar(Ref("C_NPC.attribute"), 4),
                new PushArrayVar(Ref("C_NPC.attribute"), 5),
                new Call(Ref("gainStrength")),
                
                new Ret(),
            };
            AssertInstructionsMatch();
            
            _expectedSymbols = new List<DatSymbol>
            {
                Ref("NPC_IsPlayer"),
                Ref("NPC_IsPlayer.par0"),
                Ref("WLD_PlayEffect"),
                Ref("WLD_PlayEffect.par0"),
                Ref("WLD_PlayEffect.par1"),
                Ref("WLD_PlayEffect.par2"),
                Ref("WLD_PlayEffect.par3"),
                Ref("WLD_PlayEffect.par4"),
                Ref("WLD_PlayEffect.par5"),
                Ref("WLD_PlayEffect.par6"),
                Ref("NPC_ChangeAttribute"),
                Ref("NPC_ChangeAttribute.par0"),
                Ref("NPC_ChangeAttribute.par1"),
                Ref("NPC_ChangeAttribute.par2"),
                Ref("CreateInvItems"),
                Ref("CreateInvItems.par0"),
                Ref("CreateInvItems.par1"),
                Ref("CreateInvItems.par2"),
                
                Ref("ATR_STRENGTH"),
                Ref("ATR_DEXTERITY"),
                Ref("ATR_INDEX_MAX"),
                Ref("C_NPC"),
                Ref("C_NPC.attribute"),
                Ref("NPC_Default"),
                Ref("self"),
                Ref("sword"),
                Ref("useJoint"),
                Ref("gainStrength"),
                Ref("gainStrength.slf"),
                Ref("gainStrength.spell"),
                Ref("gainStrength.mana"),
                Ref("Geralt"),
                Ref($"{prefix}10000"),
            };
            AssertSymbolsMatch(); 
        }
        
    }
}