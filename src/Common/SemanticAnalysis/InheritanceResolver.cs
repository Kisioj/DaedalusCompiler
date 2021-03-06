using System.Collections.Generic;
using Common;


namespace Common.SemanticAnalysis
{
    public class InheritanceResolver
    {
        private readonly HashSet<Symbol> _resolvedSymbols;
        private HashSet<Symbol> _resolvedSymbolsCurrentIteration;

        private readonly Dictionary <string, Symbol> _symbolTable;

        public InheritanceResolver(Dictionary<string, Symbol> symbolTable)
        {
            _resolvedSymbols = new HashSet<Symbol>();
            _symbolTable = symbolTable;
        }
        
        public void Resolve(List<SubclassSymbol> subclassSymbols)
        {
            foreach (var subclassSymbol in subclassSymbols)
            {
                _resolvedSymbolsCurrentIteration = new HashSet<Symbol>();
                Resolve(subclassSymbol);    
            }
        }
        
        private Symbol GetSymbol(string symbolName)
        {
            symbolName = symbolName.ToUpper();
            if (!_symbolTable.ContainsKey(symbolName))
            {
                return null;
            }

            return _symbolTable[symbolName];
        }

        private Symbol Resolve(SubclassSymbol subclassSymbol)
        {
            SubclassNode symbolNode = (SubclassNode) subclassSymbol.Node;
            InheritanceParentReferenceNode parentReferenceNode = symbolNode.InheritanceParentReferenceNode;
            
            if (_resolvedSymbolsCurrentIteration.Contains(subclassSymbol))
            {
                parentReferenceNode.Annotations.Add(new InfiniteInheritanceReferenceLoopError());
                return null;
            }

            if (_resolvedSymbols.Contains(subclassSymbol))
            {
                return subclassSymbol.BaseClassSymbol;
            }
            
            _resolvedSymbolsCurrentIteration.Add(subclassSymbol);
            _resolvedSymbols.Add(subclassSymbol);
            
            if (parentReferenceNode.PartNodes.Count > 0)
            {
                // TODO it's impossible scenario for current grammar
                parentReferenceNode.Annotations.Add(new NotClassOrPrototypeReferenceError());
                return null;
            }
            
            parentReferenceNode.Symbol = GetSymbol(parentReferenceNode.Name);
            
            switch (parentReferenceNode.Symbol)
            {
                case null:
                    parentReferenceNode.Annotations.Add(new UndeclaredIdentifierError(parentReferenceNode.Name));
                    break;
                case SubclassSymbol parentSubclassSymbol:
                    subclassSymbol.InheritanceParentSymbol = parentSubclassSymbol;
                    subclassSymbol.BaseClassSymbol = (ClassSymbol) Resolve(parentSubclassSymbol);
                    break;
                case ClassSymbol classSymbol:
                    subclassSymbol.InheritanceParentSymbol = classSymbol;
                    subclassSymbol.BaseClassSymbol = classSymbol;
                    break;
                default:
                    parentReferenceNode.Annotations.Add(new NotClassOrPrototypeReferenceError());
                    break;
            }

            if (parentReferenceNode.Symbol != null)
            {
                DeclarationNode declarationNode = (DeclarationNode) parentReferenceNode.Symbol.Node;
                declarationNode.Usages.Add(parentReferenceNode);
            }
            
            return subclassSymbol.BaseClassSymbol;
        }
    }
}