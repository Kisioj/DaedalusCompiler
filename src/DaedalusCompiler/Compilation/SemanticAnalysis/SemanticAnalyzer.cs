using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using Antlr4.Runtime.Tree;
using DaedalusCompiler.Compilation.SemanticAnalysis;
using DaedalusCompiler.Dat;

namespace DaedalusCompiler.Compilation
{
    public class SymbolContext
    {
        public DatSymbol Symbol;
        public DeclarationNode Node;
    }

    public class SemanticAnalyzer
    {
        private AbstractSyntaxTree _abstractSyntaxTree;
        private Dictionary<string, SymbolContext> _symbolTable;

        public SemanticAnalyzer(List<IParseTree> parseTrees, int externalFilesCount)
        {
            _symbolTable = null;
            
            Stopwatch timer = new Stopwatch();
            timer.Start();
            _abstractSyntaxTree = new AbstractSyntaxTree(parseTrees, externalFilesCount);
            timer.Stop();
            Console.WriteLine($"AbstractSyntaxTree creation time: {timer.Elapsed}");
        }


        public void CreateSymbolTable()
        {
            //Stopwatch timer = new Stopwatch();
            //timer.Start();
            //SymbolTableCreationVisitor symbolTableCreationVisitor = new SymbolTableCreationVisitor();
            //symbolTableCreationVisitor.VisitTree(_abstractSyntaxTree);
            //timer.Stop();
            //Console.WriteLine($"SymbolTable creation time: {timer.Elapsed}");
            //_symbolTable = visitor.SymbolTable;
            
            
            SymbolTableCreationVisitor symbolTableCreationVisitor = new SymbolTableCreationVisitor();
            symbolTableCreationVisitor.VisitTree(_abstractSyntaxTree);
            _symbolTable = symbolTableCreationVisitor.SymbolTable;

            ConstEvaluationVisitor constEvaluationVisitor = new ConstEvaluationVisitor(_symbolTable);
            constEvaluationVisitor.Visit(symbolTableCreationVisitor.ConstDefinitionNodes);
            constEvaluationVisitor.Visit(symbolTableCreationVisitor.ArrayDeclarationNodes);
            
            
            
            
            //constEvaluationVisitor.VisitTree(_abstractSyntaxTree);
            Console.WriteLine("---------");
            AnnotationsAdditionVisitor annotationsAdditionVisitor = new AnnotationsAdditionVisitor(constEvaluationVisitor.VisitedNodesValuesCache);
            annotationsAdditionVisitor.VisitTree(_abstractSyntaxTree);
            
        }
        
        public void EvaluateReferencesAndTypesAndArraySize()
        {
           
            
            
            /*
             Steps:
             1. Add types?
             2. Resolve references?
             
             3. Evaluate constants content
             4. Evaluate array Sizes
             5. Evaluate const arrays elements
             
             6. For every variable and parameter that is not classVar and has complex type, set ParentInde
             
             
             
             Calculate DatSymbol properties:
             
             Array: ArrayLength - must be const
             Const: Content
             * For every local and global const and var (not class attributes) if it has Class as Type,
             set ParentIndex to that Class. 
             
             Evaluate references.
             */
        }

        public void DetectErrors()
        {
            /*
             New errors:
                * typechecking
                * C_NPC size has to be 800 bytes if declared
             New warnings:
                * used name isn't exacly the same as declared (match-case)
             */
        }
        
    }
}