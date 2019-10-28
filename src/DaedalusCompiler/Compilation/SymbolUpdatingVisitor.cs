using System;
using DaedalusCompiler.Compilation.SemanticAnalysis;

namespace DaedalusCompiler.Compilation
{
    public class SymbolUpdatingVisitor : AbstractSyntaxTreeBaseVisitor
    {
        private object GetValue(NodeValue value)
        {
            switch (value)
            {
                case IntValue intValue:
                    return intValue.Value;
                case FloatValue floatValue:
                    return floatValue.Value;
                case StringValue stringValue:
                    return stringValue.Value;
                case FunctionValue functionValue:
                    return functionValue.Value;
            }
            throw new Exception();
        }
        
        protected override void VisitConstDefinition(ConstDefinitionNode node)
        {
            node.Symbol.Content = new[] { GetValue(node.RightSideValue) };
        }
    }
}