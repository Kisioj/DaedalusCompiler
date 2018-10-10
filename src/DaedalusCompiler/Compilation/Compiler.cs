﻿using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime.Tree;

namespace DaedalusCompiler.Compilation
{
    public class Compiler
    {
        public void CompileFromSrc(string srcFilePath, bool compileToAssembly)
        {
            try
            {
                /*
                List<string> pathsList = SrcFileHelper.LoadScriptsFilePaths(srcFilePath).ToList();
                string runtimePath = Path.Combine("DaedalusBuiltins", Path.GetFileNameWithoutExtension(srcFilePath).ToLower() + ".d");
                pathsList.Insert(0, runtimePath);
                string[] paths = pathsList.ToArray();
                */

                
                var assemblyBuilder = new AssemblyBuilder();
                string[] paths = SrcFileHelper.LoadScriptsFilePaths(srcFilePath).ToArray();
                
                
                string runtimePath = Path.Combine("DaedalusBuiltins", Path.GetFileNameWithoutExtension(srcFilePath).ToLower() + ".d");
                if (File.Exists(runtimePath))
                {
                    assemblyBuilder.IsCurrentlyParsingExternals = true;
                    Console.WriteLine($"[0/{paths.Length}]Compiling runtime: {runtimePath}");
                    var parser = GetParser(runtimePath);
                    ParseTreeWalker.Default.Walk(new DaedalusParserListener(assemblyBuilder, -1), parser.daedalusFile());
                    assemblyBuilder.IsCurrentlyParsingExternals = false;
                }
                
                
                for (int i = 0; i < paths.Length; i++)
                {
                    Console.WriteLine($"[{i + 1}/{paths.Length}]Compiling: {paths[i]}");

                    // create parser for specific file
                    var parser = GetParser(paths[i]);

                    ParseTreeWalker.Default.Walk(new DaedalusParserListener(assemblyBuilder, i), parser.daedalusFile());
                }

                if (compileToAssembly)
                {
                    Console.WriteLine(assemblyBuilder.GetAssembler());
                }
                else
                {
                    assemblyBuilder.SaveToDat();
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("SRC compilation failed");
                Console.WriteLine($"{exc}");
            }
        }

        private DaedalusParser GetParser(string scriptFilePath)
        {
            AntlrFileStream inputStream = new AntlrFileStream(scriptFilePath);
            DaedalusLexer lexer = new DaedalusLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
            return new DaedalusParser(commonTokenStream);
        }
    }
}