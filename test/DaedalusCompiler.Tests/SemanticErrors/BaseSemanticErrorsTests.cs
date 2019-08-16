using System;
using DaedalusCompiler.Compilation.SemanticAnalysis;
using Xunit;


namespace DaedalusCompiler.Tests.SemanticErrors
{
    public class BaseSemanticErrorsTests
    {
        protected string Code;
        protected string ExpectedCompilationOutput;
        
        private void ParseData()
        {
            string[] codeLines = Code.Trim().Split(Environment.NewLine);
            for (int i = 1; i < codeLines.Length; ++i)
            {
                codeLines[i] = codeLines[i].Substring(16);
            }
            Code = string.Join(Environment.NewLine, codeLines);

            string[] compilationOutputLines = ExpectedCompilationOutput.Trim().Split(Environment.NewLine);
            for (int i = 1; i < compilationOutputLines.Length; ++i)
            {
                compilationOutputLines[i] = compilationOutputLines[i].Substring(16);
            }
            ExpectedCompilationOutput = string.Join(Environment.NewLine, compilationOutputLines);
        }
        
        protected void AssertCompilationOutputMatch(bool strictSyntax=false)
        {
            ParseData();
            StringBufforErrorLogger logger = new StringBufforErrorLogger();
            TestsHelper testsHelper = new TestsHelper(logger, strictSyntax);
            testsHelper.RunCode(Code);
            Assert.Equal(ExpectedCompilationOutput, logger.GetBuffor().Trim());
        }
    }
}