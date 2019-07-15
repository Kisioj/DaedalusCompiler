using System.Collections.Generic;
using DaedalusCompiler.Compilation;
using DaedalusCompiler.Dat;
using Xunit;


namespace DaedalusCompiler.Tests
{
    public class ParsingExtendedSyntaxToAbstractAssemblyTests : ParsingSourceToAbstractAssemblyTestsBase
    {
       
        [Fact]
        public void TestWhileLoop()
        {
            _code = @"
                func void firstFunc() {
                    var int x;
                    x = 0;
                    while(x < 5) {
                        x += 1;
                    }
                };
           ";
            
            _instructions = GetExecBlockInstructions("firstFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // x = 0;
                new PushInt(0),
                new PushVar(Ref("firstFunc.x")),
                new Assign(),
                
                // while(x < 5) {
                new AssemblyLabel("label_while_0"),
                new PushInt(5),
                new PushVar(Ref("firstFunc.x")),
                new Less(),
                new JumpIfToLabel("label_while_1"),
                
                //     x += 1;
                new PushInt(1),
                new PushVar(Ref("firstFunc.x")),
                new AssignAdd(),
                
                // }
                new JumpToLabel("label_while_0"),
                new AssemblyLabel("label_while_1"),

                new Ret(),
            };
            AssertInstructionsMatch();

            _expectedSymbols = new List<DatSymbol>
            {
                Ref("firstFunc"),
                Ref("firstFunc.x"),
            };
            AssertSymbolsMatch();
            
        }
        
        
        [Fact]
        public void TestWhileLoopBreakContinue()
        {
            _code = @"                
                func void secondFunc() {
                    var int x;
                    x = 0;
                    while(x < 5) {
                        x += 1;
                        if (x == 3) {
                            break;
                        } else if (x == 4) {
                            continue;
                        }
                    }
                };
           ";

            _instructions = GetExecBlockInstructions("secondFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // x = 0;
                new PushInt(0),
                new PushVar(Ref("secondFunc.x")),
                new Assign(),
                
                // while(x < 5) {
                new AssemblyLabel("label_while_0"),
                new PushInt(5),
                new PushVar(Ref("secondFunc.x")),
                new Less(),
                new JumpIfToLabel("label_while_1"),
                
                //     x += 1;
                new PushInt(1),
                new PushVar(Ref("secondFunc.x")),
                new AssignAdd(),
                
                //     if (x == 3) {
                new PushInt(3),
                new PushVar(Ref("secondFunc.x")),
                new Equal(),
                new JumpIfToLabel("label_1"),
                
                //         break;
                new JumpToLabel("label_while_1"),
                
                //     } else if (x == 4) {
                new JumpToLabel("label_0"),
                new AssemblyLabel("label_1"),
                new PushInt(4),
                new PushVar(Ref("secondFunc.x")),
                new Equal(),
                new JumpIfToLabel("label_0"),
                
                //         continue;
                new JumpToLabel("label_while_0"),
                
                //     }
                new AssemblyLabel("label_0"),
                
                // }
                new JumpToLabel("label_while_0"),
                new AssemblyLabel("label_while_1"),


                new Ret(),
            };
            AssertInstructionsMatch();
            
            
            _expectedSymbols = new List<DatSymbol>
            {
                Ref("secondFunc"),
                Ref("secondFunc.x"),
            };
            AssertSymbolsMatch();
            
        }
        
        
        [Fact]
        public void TestVarDef()
        {
            _code = @"                
                var int x;
                //x = 5;
                
                func void firstFunc() {
                    var int tab[3];
                    tab[0] = 5;
                    tab[1] = 10;
                    tab[2] = 15;

                    var int x;
                    x = 5;
                }
                
                func void secondFunc() {
                    var int tab[3] = {5, 10, 15};
                    var int x = 5;
                }
           ";

            _instructions = GetExecBlockInstructions("firstFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // tab[0] = 5;
                new PushInt(5),
                new PushVar(Ref("firstFunc.tab")),
                new Assign(),
                
                // tab[1] = 10;
                new PushInt(10),
                new PushArrayVar(Ref("firstFunc.tab"), 1),
                new Assign(),
                
                // tab[2] = 15;
                new PushInt(15),
                new PushArrayVar(Ref("firstFunc.tab"), 2),
                new Assign(),
                
                // x = 5;
                new PushInt(5),
                new PushVar(Ref("firstFunc.x")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();
            
            _instructions = GetExecBlockInstructions("secondFunc");
            _expectedInstructions = new List<AssemblyElement>
            {
                // var int tab[3] = {5, 10, 15};
                new PushInt(5),
                new PushVar(Ref("secondFunc.tab")),
                new Assign(),
                
                new PushInt(10),
                new PushArrayVar(Ref("secondFunc.tab"), 1),
                new Assign(),
                
                new PushInt(15),
                new PushArrayVar(Ref("secondFunc.tab"), 2),
                new Assign(),
                
                // var int x = 5;
                new PushInt(5),
                new PushVar(Ref("secondFunc.x")),
                new Assign(),

                new Ret(),
            };
            AssertInstructionsMatch();
            
            _expectedSymbols = new List<DatSymbol>
            {
                Ref("firstFunc"),
                Ref("firstFunc.tab"),
                Ref("firstFunc.x"),
                
                Ref("secondFunc"),
                Ref("secondFunc.tab"),
                Ref("secondFunc.x"),
            };
            AssertSymbolsMatch();
            
        }
    }
}
