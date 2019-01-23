﻿using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using DaedalusCompiler.Dat;

namespace DaedalusCompiler.Compilation
{
    public class Compiler
    {

        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly OutputUnitsBuilder _ouBuilder;
        private readonly string _outputDirPath;
        

        public Compiler(string outputDirPath="output", bool verbose=true)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _assemblyBuilder = new AssemblyBuilder(verbose);
            _ouBuilder = new OutputUnitsBuilder(verbose);
            _outputDirPath = outputDirPath;
        }
        
        public string GetBuiltinsPath()
        {
            string programStartPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            return Path.Combine(Path.GetDirectoryName(programStartPath), "DaedalusBuiltins");
        }

        public bool CompileFromSrc(
            string srcFilePath, 
            bool compileToAssembly,
            bool verbose = true,
            bool generateOutputUnits = true
        )
        {
            var absoluteSrcFilePath = Path.GetFullPath(srcFilePath);

            try
            {
                string[] paths = SrcFileHelper.LoadScriptsFilePaths(absoluteSrcFilePath).ToArray();
                string srcFileName = Path.GetFileNameWithoutExtension(absoluteSrcFilePath).ToLower();
                
                string runtimePath = Path.Combine(GetBuiltinsPath(), srcFileName + ".d");
                if (File.Exists(runtimePath))
                {
                    _assemblyBuilder.IsCurrentlyParsingExternals = true;
                    if (verbose) Console.WriteLine($"[0/{paths.Length}]Compiling runtime: {runtimePath}");
                    DaedalusParser parser = GetParserForScriptsFile(runtimePath);
                    ParseTreeWalker.Default.Walk(new DaedalusListener(_assemblyBuilder, 0), parser.daedalusFile());
                    _assemblyBuilder.IsCurrentlyParsingExternals = false;
                }

                for (int i = 0; i < paths.Length; i++)
                {
                    if (verbose) Console.WriteLine($"[{i + 1}/{paths.Length}]Compiling: {paths[i]}");

                    string fileContent = GetFileContent(paths[i]);
                    DaedalusParser parser = GetParserForText(fileContent);

                    _assemblyBuilder.ErrorContext.FileContentLines = fileContent.Split(Environment.NewLine);
                    _assemblyBuilder.ErrorContext.FilePath = paths[i];
                    _assemblyBuilder.ErrorContext.FileIndex = i;
                    ParseTreeWalker.Default.Walk(new DaedalusListener(_assemblyBuilder, i), parser.daedalusFile());
                    if (generateOutputUnits)
                    {
                        _ouBuilder.ParseText(fileContent);
                    }
                }

                if (generateOutputUnits)
                {
                    _ouBuilder.SaveOutputUnits(_outputDirPath);
                }
                
                _assemblyBuilder.Finish();
                if (_assemblyBuilder.Errors.Any())
                {
                    _assemblyBuilder.Errors.Sort((x, y) => x.CompareTo(y));

                    string lastErrorFilePath = "";
                    string lastErrorBlockName = "";
                    foreach (CompilationMessage error in _assemblyBuilder.Errors)
                    {
                        if (lastErrorFilePath != error.FilePath)
                        {
                            lastErrorFilePath = error.FilePath;
                            Console.WriteLine(error.FilePath);
                        }

                        if (lastErrorBlockName != error.ExecBlockName)
                        {
                            lastErrorBlockName = error.ExecBlockName;
                            Console.WriteLine($"{error.FileName}: In {error.ExecBlockType} ‘{error.ExecBlockName}’:");
                        }

                        error.Print();
                    }
                    return false;
                }

                if (compileToAssembly)
                {
                    Console.WriteLine(_assemblyBuilder.GetAssembler());
                }
                else
                {
                    Console.WriteLine($"Compiler.cs: _outputDirPath: {_outputDirPath}");
                    Directory.CreateDirectory(_outputDirPath);
                    string datPath = Path.Combine(_outputDirPath, srcFileName + ".dat");
                    _assemblyBuilder.SaveToDat(datPath);
                }

                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine("SRC compilation failed");
                Console.WriteLine($"{exc}");
                return false;
            }
        }

        public List<DatSymbol> GetSymbols()
        {
            return _assemblyBuilder.GetSymbols();
        }

        public List<BaseExecBlockContext> GetExecBlocks()
        {
            return _assemblyBuilder.GetExecBlocks();
        }

        public void SetCompilationDateTimeText(string compilationDateTimeText)
        {
            _ouBuilder.SetGenerationDateTimeText(compilationDateTimeText);
        }
        
        public void SetCompilationUserName(string userName)
        {
            _ouBuilder.SetGenerationUserName(userName);
        }

        private string GetFileContent(string filePath)
        {
            return File.ReadAllText(filePath, Encoding.GetEncoding(1250));
        }
        
        private DaedalusParser GetParserForText(string input)
        {
            AntlrInputStream inputStream = new AntlrInputStream(input);
            DaedalusLexer lexer = new DaedalusLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
            return new DaedalusParser(commonTokenStream);
        }

        private DaedalusParser GetParserForScriptsFile(string scriptFilePath)
        {
            string fileContent = GetFileContent(scriptFilePath);
            return GetParserForText(fileContent);
        }
    }
}