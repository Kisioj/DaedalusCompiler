using System;
using System.Collections.Generic;
using System.Linq;
using Common.SemanticAnalysis;
using Common;


namespace DaedalusCompiler.Compilation
{
    enum IfLabelType
    {
        ElseIfBlockStart = 0,
        ElseBlockStart = 1,
        EndOfIfStatement = 2,
    }

    enum WhileLabelType
    {
        Start = 0,
        End = 1,
    }
    class LabelManager
    {
        private int _nextIfLabelIndex;
        private int _nextWhileLabelIndex;
        private int _nextElseIfLabelSubIndex;

        public LabelManager()
        {
            _nextIfLabelIndex = 0;
            _nextWhileLabelIndex = 0;
        }

        public void StartNextIfStatement()
        {
            _nextIfLabelIndex++;
            _nextElseIfLabelSubIndex = 0;
        }
        
        public string GenerateIfLabel(IfLabelType ifLabelType)
        {
            string labelId = _nextIfLabelIndex.ToString("0000");
            string label = "";
            switch (ifLabelType)
            {
                case IfLabelType.ElseIfBlockStart:
                    label = $"else_if_{++_nextElseIfLabelSubIndex}";
                    break;
                case IfLabelType.ElseBlockStart:
                    label = "else";
                    break;
                case IfLabelType.EndOfIfStatement:
                    label = "endif";
                    break;
            }

            return $"#{labelId}_{label}";
        }
        
        public void StartNextWhileStatement()
        {
            _nextWhileLabelIndex++;
        }
        
        public string GenerateWhileLabel(WhileLabelType whileLabelType)
        {
            string labelId = _nextWhileLabelIndex.ToString("0000");
            string label = "";
            switch (whileLabelType)
            {
                case WhileLabelType.Start:
                    label = "while";
                    break;
                case WhileLabelType.End:
                    label = "endwhile";
                    break;
            }

            return $"#{labelId}_{label}";
        }
    }
    
    public class AssemblyBuildingVisitor : AbstractSyntaxTreeBaseGenericVisitor<List<AssemblyElement>>
    {
        private readonly Dictionary<string, Symbol> _symbolTable;
        private readonly LabelManager _labelManager;
        private bool _nestedAttributesFound;
        private readonly InstanceSymbol _helperInstance;
        private static string HELPER_INSTANCE = ".HELPER_INSTANCE";

        public AssemblyBuildingVisitor(Dictionary<string, Symbol> symbolTable)
        {
            _symbolTable = symbolTable;
            _labelManager = new LabelManager();

            
            _nestedAttributesFound = false;
            _helperInstance = new InstanceSymbol(HELPER_INSTANCE, null);
            if (_symbolTable.Count > 0)
            {
                _helperInstance.Index = _symbolTable.Values.Last().Index + 1;
            }
            
        }

        public new Dictionary<string, Symbol> VisitTree(AbstractSyntaxTree tree)
        {
            base.VisitTree(tree);
            if (_nestedAttributesFound)
            {
                _symbolTable[HELPER_INSTANCE] = _helperInstance;
            }
            return _symbolTable;
        }
        

        private AssemblyElement GetParameterPush(Symbol symbol)
        {
            switch (symbol.BuiltinType)
            {
                case SymbolType.Instance:
                    return new PushInstance(symbol);
                default:
                    return new PushVar(symbol);
            }
        }
        
        private AssemblyElement GetAssignInstruction(Symbol symbol)
        {
            switch (symbol.BuiltinType)
            {
                case SymbolType.Int:
                    return new Assign();
                case SymbolType.String:
                    return new AssignString();
                case SymbolType.Func:
                    return new AssignFunc();
                case SymbolType.Float:
                    return new AssignFloat();
                case SymbolType.Instance:
                case SymbolType.Class:
                    return new AssignInstance();
            }
            throw new Exception();
        }


        protected override List<AssemblyElement> VisitFunctionDefinition(FunctionDefinitionNode node)
        {
            List<AssemblyElement> instructions = ((BlockSymbol)node.Symbol).Instructions;
            if (node.IsExternal)
            {
                return instructions;
            }
            
            for (int i = node.ParameterNodes.Count - 1; i >= 0; i--)
            {
                ParameterDeclarationNode parameterNode = node.ParameterNodes[i];
                instructions.AddRange(Visit(parameterNode));
            }
            foreach (StatementNode bodyNode in node.BodyNodes)
            {
                instructions.AddRange(Visit(bodyNode));
            }
            instructions.Add(new Ret());
            return instructions;
        }

        protected override List<AssemblyElement> VisitInstanceDefinition(InstanceDefinitionNode node)
        {
            List<AssemblyElement> instructions = ((BlockSymbol)node.Symbol).Instructions;

            if (node.InheritanceParentReferenceNode.Symbol is PrototypeSymbol prototypeSymbol)
            {
                instructions.Add(new Call(prototypeSymbol));
            }
            
            foreach (StatementNode bodyNode in node.BodyNodes)
            {
                instructions.AddRange(Visit(bodyNode));
            }
            instructions.Add(new Ret());
            return instructions;
        }

        protected override List<AssemblyElement> VisitPrototypeDefinition(PrototypeDefinitionNode node)
        {
            List<AssemblyElement> instructions = ((BlockSymbol)node.Symbol).Instructions;
            foreach (StatementNode bodyNode in node.BodyNodes)
            {
                instructions.AddRange(Visit(bodyNode));
            }
            instructions.Add(new Ret());
            return instructions;
        }

        protected override List<AssemblyElement> VisitIfStatement(IfStatementNode node)
        {
            _labelManager.StartNextIfStatement();
            
            string statementEndLabel = _labelManager.GenerateIfLabel(IfLabelType.EndOfIfStatement);
            string elseStartLabel = _labelManager.GenerateIfLabel(IfLabelType.ElseBlockStart);
            
            List<AssemblyElement> instructions = new List<AssemblyElement>();
            
            List<ConditionalNode> conditionalNodes = new List<ConditionalNode>();
            conditionalNodes.Add(node.IfNode);
            conditionalNodes.AddRange(node.ElseIfNodes);
            foreach (ConditionalNode conditionalNode in conditionalNodes)
            {
                instructions.AddRange(Visit(conditionalNode.ConditionNode));
                bool isLastIteration = conditionalNode == conditionalNodes.Last();
                if (isLastIteration)
                {
                    if (node.ElseNodeBodyNodes!=null)
                    {
                        
                        instructions.Add(new JumpIfToLabel(elseStartLabel));
                        foreach (StatementNode bodyNode in conditionalNode.BodyNodes)
                        {
                            instructions.AddRange(Visit(bodyNode));
                        }
                        instructions.Add(new JumpToLabel(statementEndLabel));
                    }
                    else
                    {
                        instructions.Add(new JumpIfToLabel(statementEndLabel));
                        foreach (StatementNode bodyNode in conditionalNode.BodyNodes)
                        {
                            instructions.AddRange(Visit(bodyNode));
                        }
                    }
                }
                else
                {
                    string nextJumpLabel = _labelManager.GenerateIfLabel(IfLabelType.ElseIfBlockStart);
                    instructions.Add(new JumpIfToLabel(nextJumpLabel));
                    foreach (StatementNode bodyNode in conditionalNode.BodyNodes)
                    {
                        instructions.AddRange(Visit(bodyNode));
                    }
                    instructions.Add(new JumpToLabel(statementEndLabel));
                    instructions.Add(new AssemblyLabel(nextJumpLabel));
                }
            }

            if (node.ElseNodeBodyNodes != null)
            {
                instructions.Add(new AssemblyLabel(elseStartLabel));
                foreach (StatementNode bodyNode in node.ElseNodeBodyNodes)
                {
                    instructions.AddRange(Visit(bodyNode));
                }
            }
            instructions.Add(new AssemblyLabel(statementEndLabel));

            return instructions;

        }

        protected override List<AssemblyElement> VisitWhileStatement(WhileStatementNode node)
        {
            _labelManager.StartNextWhileStatement();
            
            string startLabel = _labelManager.GenerateWhileLabel(WhileLabelType.Start);
            string endLabel = _labelManager.GenerateWhileLabel(WhileLabelType.End);

            List<AssemblyElement> instructions = new List<AssemblyElement>();
            instructions.Add(new AssemblyLabel(startLabel));
            instructions.AddRange(Visit(node.ConditionNode));
            instructions.Add(new JumpIfToLabel(endLabel));
            
            
            foreach (StatementNode bodyNode in node.BodyNodes)
            {
                List<AssemblyElement> bodyInstructions = Visit(bodyNode);
                foreach (AssemblyElement instruction in bodyInstructions)
                {
                    if (instruction is JumpToLoopStart)
                    {
                        instructions.Add(new JumpToLabel(startLabel));
                    }
                    else if (instruction is JumpToLoopEnd)
                    {
                        instructions.Add(new JumpToLabel(endLabel));
                    }
                    else
                    {
                        instructions.Add(instruction);
                    }
                }
            }

            instructions.Add(new JumpToLabel(startLabel));
            instructions.Add(new AssemblyLabel(endLabel));
            return instructions;
        }

        protected override List<AssemblyElement> VisitParameterDeclaration(ParameterDeclarationNode node)
        {
            return new List<AssemblyElement>
            {
                GetParameterPush(node.Symbol),
                GetAssignInstruction(node.Symbol),
            };
        }

        protected override List<AssemblyElement> VisitParameterArrayDeclaration(ParameterArrayDeclarationNode node)
        {
            return new List<AssemblyElement>
            {
                GetParameterPush(node.Symbol),
                GetAssignInstruction(node.Symbol),
            };
        }

        protected override List<AssemblyElement> VisitReference(ReferenceNode referenceNode)
        {
            int index;
            List<AssemblyElement> instructions = new List<AssemblyElement>();

            if (referenceNode.DoesHaveNestedAttributes)
            {
                //TODO check in game if it does work correctly
                _nestedAttributesFound = true;

                instructions.Add(new PushVar(_helperInstance));
                instructions.Add(new PushVar(referenceNode.BaseSymbol));
                instructions.Add(new Assign());
                
                for (int i = 0; i < referenceNode.PartNodes.Count - 1; ++i)
                {
                    ReferencePartNode partNode = referenceNode.PartNodes[i];
                    switch (partNode)
                    {
                        case AttributeNode attributeNode:

                            if (attributeNode.Symbol is InstanceSymbol ||
                                (attributeNode.Symbol is VarSymbol varSymbol &&
                                 varSymbol.BuiltinType == SymbolType.Instance))
                            {
                                
                                instructions.Add(new PushVar(_helperInstance));
                                instructions.Add(new SetInstance(_helperInstance));
                                if (attributeNode.ArrayIndexNode == null)
                                {
                                    instructions.Add(new PushVar(attributeNode.Symbol));
                                }
                                else
                                {
                                    index = (int) ((IntValue) attributeNode.ArrayIndexNode.Value).Value;
                                    instructions.Add(new PushArrayVar(attributeNode.Symbol, index));
                                }
                                instructions.Add(new Assign());
                            }
                            break;
                    }
                }
            }

            index = -1;
            if (referenceNode.IndexNode != null)
            {
                index = (int) ((IntValue) referenceNode.IndexNode.Value).Value;
            }

            if (referenceNode.BaseSymbol != referenceNode.Symbol)
            {
                if (referenceNode.DoesHaveNestedAttributes)
                {
                    instructions.Add(new SetInstance(_helperInstance));
                }
                else
                {
                    instructions.Add(new SetInstance(referenceNode.BaseSymbol));
                }
            }
        
            if (index > 0)
            {
                instructions.Add(new PushArrayVar(referenceNode.Symbol, index));
            }
            else if (referenceNode.DoCastToInt)
            {
                instructions.Add(new PushInt(referenceNode.Symbol.Index));
            }
            else if (referenceNode.Symbol.BuiltinType == SymbolType.Instance)
            {
                instructions.Add(new PushInstance(referenceNode.Symbol));
            }
            else
            {
                instructions.Add(new PushVar(referenceNode.Symbol));
            }
            
            return instructions;
        }

        protected override List<AssemblyElement> VisitIntegerLiteral(IntegerLiteralNode node)
        {
            int intValue = (int) node.Value;
            
            if (node.DoCastToFloat)
            {
                float floatValue = node.Value;
                intValue = BitConverter.ToInt32(BitConverter.GetBytes(floatValue), 0);
            }
            
            return new List<AssemblyElement>{ new PushInt(intValue) };
        }

        protected override List<AssemblyElement> VisitFloatLiteral(FloatLiteralNode node)
        {
            float floatValue = node.Value;
            int intValue = BitConverter.ToInt32(BitConverter.GetBytes(floatValue), 0);
            return new List<AssemblyElement>{ new PushInt(intValue) };
        }

        protected override List<AssemblyElement> VisitStringLiteral(StringLiteralNode node)
        {
            return new List<AssemblyElement> { new PushVar(node.Symbol) };
        }

        protected override List<AssemblyElement> VisitAssignment(AssignmentNode node)
        {
            List<AssemblyElement> instructions = new List<AssemblyElement>();
            instructions.AddRange(Visit(node.RightSideNode));
            instructions.AddRange(Visit(node.LeftSideNode));
            instructions.Add(GetAssignInstruction(node.LeftSideNode.Symbol));
            return instructions;
        }

        protected override List<AssemblyElement> VisitCompoundAssignment(CompoundAssignmentNode node)
        {
            List<AssemblyElement> instructions = new List<AssemblyElement>();
            instructions.AddRange(Visit(node.RightSideNode));
            instructions.AddRange(Visit(node.LeftSideNode));
            switch (node.Operator)
            {
                case CompoundAssignmentOperator.Add:
                    instructions.Add(new AssignAdd());
                    break;
                case CompoundAssignmentOperator.Sub:
                    instructions.Add(new AssignSubtract());
                    break;
                case CompoundAssignmentOperator.Mult:
                    instructions.Add(new AssignMultiply());
                    break;
                case CompoundAssignmentOperator.Div:
                    instructions.Add(new AssignDivide());
                    break;
            }
            return instructions;
        }

        protected override List<AssemblyElement> VisitBinaryExpression(BinaryExpressionNode node)
        {
            List<AssemblyElement> instructions = new List<AssemblyElement>();
            instructions.AddRange(Visit(node.RightSideNode));
            instructions.AddRange(Visit(node.LeftSideNode));

            switch (node.Operator)
            {
                case BinaryOperator.Mult:
                    instructions.Add(new Multiply());
                    break;
                case BinaryOperator.Div:
                    instructions.Add(new Divide());
                    break;
                case BinaryOperator.Modulo:
                    instructions.Add(new Modulo());
                    break;
                
                case BinaryOperator.Add:
                    instructions.Add(new Add());
                    break;
                case BinaryOperator.Sub:
                    instructions.Add(new Subtract());
                    break;
                
                case BinaryOperator.ShiftLeft:
                    instructions.Add(new ShiftLeft());
                    break;
                case BinaryOperator.ShiftRight:
                    instructions.Add(new ShiftRight());
                    break;
                
                case BinaryOperator.Less:
                    instructions.Add(new Less());
                    break;
                case BinaryOperator.Greater:
                    instructions.Add(new Greater());
                    break;
                case BinaryOperator.LessOrEqual:
                    instructions.Add(new LessOrEqual());
                    break;
                case BinaryOperator.GreaterOrEqual:
                    instructions.Add(new GreaterOrEqual());
                    break;
                
                case BinaryOperator.Equal:
                    instructions.Add(new Equal());
                    break;
                case BinaryOperator.NotEqual:
                    instructions.Add(new NotEqual());
                    break;
                
                case BinaryOperator.BinAnd:
                    instructions.Add(new BitAnd());
                    break;
                case BinaryOperator.BinOr:
                    instructions.Add(new BitOr());
                    break;
                case BinaryOperator.LogAnd:
                    instructions.Add(new LogAnd());
                    break;
                case BinaryOperator.LogOr:
                    instructions.Add(new LogOr());
                    break;
            }
            return instructions;
        }

        protected override List<AssemblyElement> VisitUnaryExpression(UnaryExpressionNode node)
        {
            List<AssemblyElement> instructions = new List<AssemblyElement>();
            instructions.AddRange(Visit(node.ExpressionNode));
            if (node.DoGenerateOperatorInstruction)
            {
                switch (node.Operator)
                {
                    case UnaryOperator.Minus:
                        instructions.Add(new Minus());
                        break;
                    case UnaryOperator.Not:
                        instructions.Add(new Not());
                        break;
                    case UnaryOperator.Negate:
                        instructions.Add(new Negate());
                        break;
                    case UnaryOperator.Plus:
                        instructions.Add(new Plus());
                        break;
                }
            }
            return instructions;
        }

        protected override List<AssemblyElement> VisitReturnStatement(ReturnStatementNode node)
        {
            List<AssemblyElement> instructions = new List<AssemblyElement>();

            if (node.ExpressionNode != null)
            {
                instructions.AddRange(Visit(node.ExpressionNode));
            }
            
            instructions.Add(new Ret());
            
            return instructions;
        }

        protected override List<AssemblyElement> VisitBreakStatement(BreakStatementNode node)
        {
            return new List<AssemblyElement> { new JumpToLoopEnd() };
        }

        protected override List<AssemblyElement> VisitContinueStatement(ContinueStatementNode node)
        {
            return new List<AssemblyElement> { new JumpToLoopStart() };
        }
        
        protected override List<AssemblyElement> VisitFunctionCall(FunctionCallNode node)
        {
            List<AssemblyElement> instructions = new List<AssemblyElement>();
            foreach (ExpressionNode argumentNode in node.ArgumentNodes)
            {
                instructions.AddRange(Visit(argumentNode));
            }

            FunctionSymbol symbol = (FunctionSymbol) node.FunctionReferenceNode.Symbol;
            if (symbol.IsExternal)
            {
                instructions.Add(new CallExternal(symbol));
            }
            else
            {
                instructions.Add(new Call(symbol));
            }
            return instructions;
        }

        protected override List<AssemblyElement> VisitVarDeclaration(VarDeclarationNode node)
        {
            List<AssemblyElement> instructions = new List<AssemblyElement>();
            if (node.RightSideNode != null)
            {
                instructions.AddRange(Visit(node.RightSideNode));
                instructions.Add(new PushVar(node.Symbol));
                instructions.Add(GetAssignInstruction(node.Symbol));
            }

            return instructions;
        }

        protected override List<AssemblyElement> VisitVarArrayDeclaration(VarArrayDeclarationNode node)
        {
            List<AssemblyElement> instructions = new List<AssemblyElement>();
            if (node.ElementNodes != null)
            {
                int index = 0;
                foreach (ExpressionNode elementNode in node.ElementNodes)
                {
                    instructions.AddRange(Visit(elementNode));
                    if (index > 0)
                    {
                        instructions.Add(new PushArrayVar(node.Symbol, index));
                    }
                    else
                    {
                        instructions.Add(new PushVar(node.Symbol));
                    }
                    instructions.Add(GetAssignInstruction(node.Symbol));
                    index++;
                }
            }


            return instructions;
        }

        protected override List<AssemblyElement> VisitConstDefinition(ConstDefinitionNode node)
        {
            return new List<AssemblyElement>();
        }

        protected override List<AssemblyElement> VisitConstArrayDefinition(ConstArrayDefinitionNode node)
        {
            return new List<AssemblyElement>();
        }

        protected override List<AssemblyElement> VisitClassDefinition(ClassDefinitionNode node)
        {
            return new List<AssemblyElement>();
        }

        protected override List<AssemblyElement> VisitNull(NullNode node)
        {
            return new List<AssemblyElement>
            {
                new PushNullInstance(),
            };
        }

        protected override List<AssemblyElement> VisitNoFunc(NoFuncNode node)
        {
            return new List<AssemblyElement>
            {
                new PushInt(-1),
            };
        }
    }
}