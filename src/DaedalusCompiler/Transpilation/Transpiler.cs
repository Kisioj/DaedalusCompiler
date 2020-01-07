using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DaedalusCompiler.Compilation;
using DaedalusCompiler.Compilation.SemanticAnalysis;

namespace DaedalusTranspiler.Transpilation
{
    public class Transpiler
    {
        private readonly string _outputDirPath;
        private readonly bool _strictSyntax;
        private readonly HashSet<string> _globallySuppressedCodes;
        
        public Transpiler(string outputDirPath, bool strictSyntax, HashSet<string> globallySuppressedCodes)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _outputDirPath = outputDirPath;
            _strictSyntax = strictSyntax;
            _globallySuppressedCodes = globallySuppressedCodes;
        }
        
        public static HashSet<string> GetWarningCodesToSuppress(string line)
        {
            string ws = @"(?:[ \t])*";
            string newline = @"(?:\r\n?|\n)";
            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline;
            string suppressWarningsPattern = $@"//!{ws}suppress{ws}:((?:{ws}[a-zA-Z0-9]+)+){ws}{newline}?$";
            MatchCollection matches = Regex.Matches(line, suppressWarningsPattern, options);
            foreach (Match match in matches)
            {
                return match.Groups[1].Value.Split(" ").Where(s => !s.Equals(String.Empty)).ToHashSet();
            }
            return new HashSet<string>();
        }

        private string GetBuiltinsPath()
        {
            string programStartPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            return Path.Combine(Path.GetDirectoryName(programStartPath), "LegacyDaedalusBuiltins");
        }

        public bool TranspileFromSrc(
            string srcFilePath,
            string runtimePath,
            string outputPath,
            bool verbose = true,
            bool generateOutputUnits = true
        )
        {
            bool isRunTimePathSpecified = runtimePath != String.Empty;
            
            var absoluteSrcFilePath = Path.GetFullPath(srcFilePath);

            string[] paths;
            try
            {
                paths = SrcFileHelper.LoadScriptsFilePaths(absoluteSrcFilePath).ToArray();
            }
            catch (Exception ex)
            {
                if (ex is DirectoryNotFoundException || ex is FileNotFoundException)
                {
                    Console.WriteLine($"Could not find '{srcFilePath}'");
                    return false;
                }
                throw;
            }
            
            string srcFileName = Path.GetFileNameWithoutExtension(absoluteSrcFilePath).ToLower();
            List<IParseTree> parseTrees = new List<IParseTree>();
            
            List<string> filesPaths = new List<string>();
            List<string[]> filesContentsLines = new List<string[]>();
            List<string> filesContents = new List<string>();
            List<CommonTokenStream> tokenStreams = new List<CommonTokenStream>();
            List<HashSet<string>> suppressedWarningCodes = new List<HashSet<string>>();

            int syntaxErrorsCount = 0;
            
            if (File.Exists(runtimePath))
            {
                if (verbose) Console.WriteLine($"[0/{paths.Length}]Parsing runtime: {runtimePath}");
                
                string fileContent = GetFileContent(runtimePath);
                LegacyDaedalusParser parser = GetParserForText(fileContent);
                tokenStreams.Add((CommonTokenStream) parser.TokenStream);

                SyntaxErrorListener syntaxErrorListener = new SyntaxErrorListener();
                parser.AddErrorListener(syntaxErrorListener);
                parseTrees.Add(parser.daedalusFile());
                
                string[] fileContentLines = fileContent.Split(Environment.NewLine);
                filesPaths.Add(runtimePath);
                filesContentsLines.Add(fileContentLines);
                suppressedWarningCodes.Add(GetWarningCodesToSuppress(fileContentLines[0]));
                
                syntaxErrorsCount += syntaxErrorListener.ErrorsCount;
            }
            else if(isRunTimePathSpecified)
            {
                if (verbose) Console.WriteLine($"Specified runtime {runtimePath} doesn't exist.");
            }

            
            for (int i = 0; i < paths.Length; i++)
            {
                if (verbose) Console.WriteLine($"[{i + 1}/{paths.Length}]Parsing: {paths[i]}");
                
                string fileContent = GetFileContent(paths[i]);
                LegacyDaedalusParser parser = GetParserForText(fileContent);
                tokenStreams.Add((CommonTokenStream) parser.TokenStream);
                
                SyntaxErrorListener syntaxErrorListener = new SyntaxErrorListener();
                parser.AddErrorListener(syntaxErrorListener);
                parseTrees.Add(parser.daedalusFile());

                string[] fileContentLines = fileContent.Split(Environment.NewLine);
                filesPaths.Add(paths[i]);
                filesContentsLines.Add(fileContentLines);
                filesContents.Add(fileContent);
                suppressedWarningCodes.Add(GetWarningCodesToSuppress(fileContentLines[0]));
                
                syntaxErrorsCount += syntaxErrorListener.ErrorsCount;
            }
            
            
            StdErrorLogger logger = new StdErrorLogger();
            
            if (syntaxErrorsCount > 0)
            {
                logger.LogLine($"{syntaxErrorsCount} syntax {(syntaxErrorsCount == 1 ? "error" : "errors")} generated.");
                return false;
            }
            
            if (verbose) Console.WriteLine("parseTrees created");

            SemanticAnalyzer semanticAnalyzer = new SemanticAnalyzer(parseTrees, tokenStreams, filesPaths, filesContentsLines, suppressedWarningCodes);
            semanticAnalyzer.Run();

            SemanticErrorsCollectingVisitor semanticErrorsCollectingVisitor = new SemanticErrorsCollectingVisitor(
                new StdErrorLogger(),
                _strictSyntax,
                _globallySuppressedCodes);
                
            semanticErrorsCollectingVisitor.VisitTree(semanticAnalyzer.AbstractSyntaxTree);

            int errorsCount = semanticErrorsCollectingVisitor.ErrorsCount;
            int warningsCount = semanticErrorsCollectingVisitor.WarningsCount;
            string error = errorsCount == 1 ? "error" : "errors";
            string warning = warningsCount == 1 ? "warning" : "warnings";

            if (errorsCount > 0)
            {
                logger.LogLine(warningsCount > 0
                    ? $"{errorsCount} {error}, {warningsCount} {warning} generated."
                    : $"{errorsCount} {error} generated.");
                return false;
            }

            if (warningsCount > 0)
            {
                logger.LogLine($"{warningsCount} {warning} generated.");
            }
            
            if (verbose) Console.WriteLine($"parseTrees.Count: {parseTrees.Count}");
            
            
            /*
             DaedalusBuildingVisitor daedalusBuildingVisitor = new DaedalusBuildingVisitor();
             assemblyBuildingVisitor.VisitTree(semanticAnalyzer.AbstractSyntaxTree);
            */

            return true;
        }
        
        private string GetFileContent(string filePath)
        {
            return File.ReadAllText(filePath, Encoding.GetEncoding(1250));
        }
        
        public static LegacyDaedalusParser GetParserForText(string input)
        {
            AntlrInputStream inputStream = new AntlrInputStream(input);
            LegacyDaedalusLexer lexer = new LegacyDaedalusLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
            return new LegacyDaedalusParser(commonTokenStream);
        }

    }
}