using System;
using System.Collections.Generic;
using DaedalusCompiler.Compilation;
using System.Diagnostics;
using DaedalusCompiler.Compilation.SemanticAnalysis;
using DaedalusTranspiler.Transpilation;

namespace DaedalusCompiler
{
    class Program
    {
        private const string Version = "0.7.0";
        private const string CompilerName = "daedalus-compiler";

        static void ShowHelp()
        {
            Console.WriteLine("Daedalus Compiler Version {0}", Version);
            Console.WriteLine(
                "usage: {0} src_file_path [<args>]", CompilerName
            );
            Console.WriteLine(
                "Args description:\n" +
                "--suppress              suppress warnings globally\n" +
                "-g|--gen-ou             generate output units files (ou.cls and ou.bin)\n" +
                "-s|--strict             use more strict syntax version\n" +
                "-d|--detect-unused      enables unused symbol warnings\n" +
                "-c|--case-sensitive     enables case sensitive mode\n" +
                "-v|--version            displays version of compiler\n" +
                "-r|--runtime <path>     (optional) custom externals file\n" +
                "-o|--output <path>      (optional) output path (compiler: .DAT file, transpiler: directory)\n" +
                "-t|--transpile          run transpilation instead of compilation\n" +
                "--verbose"
            );
        }
        
        static void HandleOptionsParser(string[] args)
        {
            var loadHelp = false;
            var generateOutputUnits = false;
            var verbose = false;
            var strict = false;
            var getVersion = false;
            bool suppressModeOn = false;
            bool detectUnused = false;
            bool caseSensitiveCode = false;
            bool transpile = false;
            string filePath = String.Empty;
            string runtimePath = String.Empty;
            string outputPath = String.Empty;
            HashSet<string> suppressCodes = new HashSet<string>();

            var optionSet = new NDesk.Options.OptionSet {
                { "h|?|help",   v => loadHelp = true },
                { "suppress", v => suppressModeOn = true },
                { "g|gen-ou", v => generateOutputUnits = true },
                { "s|strict", v => strict = true },
                { "d|detect-unused", v => detectUnused = true },
                { "c|case-sensitive", v => caseSensitiveCode = true },
                { "v|version", v => getVersion = true  },
                { "r|runtime=", v => runtimePath = v},
                { "o|output=", v => outputPath = v},
                { "t|transpile", v => transpile = true},
                { "verbose", v => verbose = true },
                { "<>", v =>
                    {
                        if (suppressModeOn)
                        {
                            suppressCodes.Add(v);
                        }
                        else
                        {
                            filePath = v;
                        }
                    }
                },
            };
            
            try {
                optionSet.Parse (args);
            }
            catch (NDesk.Options.OptionException e) {
                Console.WriteLine (e.Message);
                return;
            }
            
            if (!caseSensitiveCode)
            {
                suppressCodes.Add(NamesNotMatchingCaseWiseWarning.WCode);
            }

            
            if (!detectUnused)
            {
                suppressCodes.Add(UnusedSymbolWarning.WCode);
            }

            if (getVersion)
            {
                Console.WriteLine($"v{Version}");
                return;
            }

            if ( loadHelp || filePath == String.Empty )
            {
                ShowHelp();
            }
            else
            {
                if (transpile)
                {
                    TranspileDaedalus(filePath, runtimePath, outputPath, verbose, generateOutputUnits, strict, suppressCodes);
                }
                else
                {
                    CompileDaedalus(filePath, runtimePath, outputPath, verbose, generateOutputUnits, strict, suppressCodes);
                }
                
            }
        }
        
        static void CompileDaedalus(string path, string runtimePath, string outputPath, bool verbose, bool generateOutputUnits, bool strictSyntax, HashSet<string> suppressCodes)
        {
            var compiler = new Compiler("output", verbose, strictSyntax, suppressCodes);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            bool compiledSuccessfully = compiler.CompileFromSrc(path, runtimePath, outputPath, verbose, generateOutputUnits);
            if (compiledSuccessfully)
            {
                Console.WriteLine($"Compilation completed successfully. Total time: {stopwatch.Elapsed}");
            }
            else
            {
                Console.WriteLine($"Compilation FAILED. Total time: {stopwatch.Elapsed}");
                Environment.Exit(1);
            }
        }

        static void TranspileDaedalus(string path, string runtimePath, string outputPath, bool verbose, bool generateOutputUnits, bool strictSyntax, HashSet<string> suppressCodes)
        {
            var compiler = new Transpiler("output", strictSyntax, suppressCodes);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            bool transpiledSuccessfully = compiler.TranspileFromSrc(path, runtimePath, outputPath, verbose, generateOutputUnits);
            if (transpiledSuccessfully)
            {
                Console.WriteLine($"Transpilation completed successfully. Total time: {stopwatch.Elapsed}");
            }
            else
            {
                Console.WriteLine($"Transpilation FAILED. Total time: {stopwatch.Elapsed}");
                Environment.Exit(1);
            }
        }
        
        static void Main(string[] args)
        {
            HandleOptionsParser(args);
        }
    }
}
