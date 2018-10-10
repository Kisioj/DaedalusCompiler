/*
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DaedalusCompiler.Compilation;


namespace DaedalusCompiler.Dat
{
    public class DatSaver
    {
        private string _code;
        private AssemblyBuilder _assemblyBuilder;

        public DatSaver()
        {
            _assemblyBuilder = new AssemblyBuilder();
        }

            
        public static void WalkSourceCode(string code, AssemblyBuilder assemblyBuilder)
        {
            var inputStream = new AntlrInputStream(code);
            var lexer = new DaedalusLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new DaedalusParser(commonTokenStream);

            ParseTreeWalker.Default.Walk(new DaedalusParserListener(assemblyBuilder, 0), parser.daedalusFile());
        }
        
        private DatFile compileCodeAndGetDATFileOfThat()
        {
            WalkSourceCode(_code, _assemblyBuilder);
            var datBuilder = new DatBuilder(_assemblyBuilder);
            var datFile = datBuilder.GetDatFile();
            var datToReturn = new DatFile();
            
            datToReturn.Load(datFile.getBinary());

            return datToReturn;
        }
        
    }
}
*/