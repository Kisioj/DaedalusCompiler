﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using DaedalusCompiler.Dat;
using Microsoft.CSharp.RuntimeBinder;

namespace DaedalusCompiler.Compilation
{
    public class DaedalusParserListener : DaedalusBaseListener
    {
        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly int _sourceFileNumber;

        public DaedalusParserListener(AssemblyBuilder assemblyBuilder, int sourceFileNumber)
        {
            _assemblyBuilder = assemblyBuilder;
            _sourceFileNumber = sourceFileNumber;
        }

        public override void ExitParameterList(DaedalusParser.ParameterListContext context)
        {
            var parametrDeclContexts = context.parameterDecl();
            foreach (var parameterDeclContext in parametrDeclContexts.Reverse())
            {
                ExecBlock execBlock = _assemblyBuilder.ExecBlocks.Last();
                string execBlockName = execBlock.Symbol.Name;
                string parameterLocalName = parameterDeclContext.nameNode().GetText();
                string parameterName = $"{execBlockName}.{parameterLocalName}";
                DatSymbol parameterSymbol = _assemblyBuilder.ResolveSymbol(parameterName);

                var assignInstruction =
                    AssemblyBuilderHelpers.GetAssignInstructionForDatSymbolType(parameterSymbol.Type);

                if (parameterSymbol.Type is DatSymbolType.Class)
                {
                    _assemblyBuilder.AddInstruction(new PushInstance(parameterSymbol));
                }
                else
                {
                    _assemblyBuilder.AddInstruction(new PushVar(parameterSymbol));
                }

                _assemblyBuilder.AddInstruction(assignInstruction);
            }
        }

        public override void EnterParameterDecl(DaedalusParser.ParameterDeclContext context)
        {
            ExecBlock execBlock = _assemblyBuilder.ExecBlocks.Last();
            string execBlockName = execBlock.Symbol.Name;
            string parameterLocalName = context.nameNode().GetText();
            string parameterName = $"{execBlockName}.{parameterLocalName}";

            int parentId = -1;

            string parameterTypeName = context.typeReference().GetText();
            DatSymbolType? parameterType = DatSymbolTypeFromString(parameterTypeName);
            if (parameterType is DatSymbolType.Class)
            {
                var parentSymbol = _assemblyBuilder.ResolveSymbol(parameterTypeName);
                parentId = parentSymbol.Index;
            }

            DatSymbol symbol;
            var location = GetLocation(context);

            var simpleValueContext = context.simpleValue();

            if (simpleValueContext != null)
            {
                if (!uint.TryParse(simpleValueContext.GetText(), out var arrIndex))
                {
                    var constSymbol = _assemblyBuilder.ResolveSymbol(simpleValueContext.GetText());
                    if (constSymbol.Flags != DatSymbolFlag.Const || constSymbol.Type != DatSymbolType.Int)
                    {
                        throw new Exception($"Expected integer constant: {simpleValueContext.GetText()}");
                    }

                    arrIndex = (uint) (int) constSymbol.Content[0];
                }

                symbol = SymbolBuilder.BuildArrOfVariables(parameterName, parameterType.Value, arrIndex, location);
            }
            else
            {
                symbol = SymbolBuilder.BuildVariable(parameterName, parameterType.Value, location, parentId);
            }

            _assemblyBuilder.AddSymbol(symbol);
        }

        public override void EnterConstDef([NotNull] DaedalusParser.ConstDefContext context)
        {
            _assemblyBuilder.IsInsideEvalableStatement = true;
            var typeName = context.typeReference().GetText();
            var type = DatSymbolTypeFromString(typeName);
            if (type == DatSymbolType.Func)
            {
                type = DatSymbolType.Int;
            }

            for (int i = 0; i < context.ChildCount; i++)
            {
                var constContext = context.GetChild(i);

                if (constContext is TerminalNodeImpl)
                    continue; // skips ',' 

                if (constContext is DaedalusParser.ConstValueDefContext constValueContext)
                {
                    var name = constValueContext.nameNode().GetText();
                    if (_assemblyBuilder.IsContextInsideExecBlock())
                    {
                        ExecBlock execBlock = _assemblyBuilder.ExecBlocks.Last();
                        string execBlockName = execBlock.Symbol.Name;
                        name = $"{execBlockName}.{name}";
                    }

                    var location = GetLocation(context);
                    var assignmentExpression = constValueContext.constValueAssignment().expressionBlock().expression();
                    var value = EvaluatorHelper.EvaluateConst(assignmentExpression, _assemblyBuilder, type);

                    var symbol = SymbolBuilder.BuildConst(name, type, value, location); // TODO : Validate params
                    _assemblyBuilder.AddSymbol(symbol);

                    continue;
                }

                if (constContext is DaedalusParser.ConstArrayDefContext constArrayContext)
                {
                    var name = constArrayContext.nameNode().GetText();
                    var location = GetLocation(context);
                    var size = EvaluatorHelper.EvaluteArraySize(constArrayContext.simpleValue(), _assemblyBuilder);
                    var content = constArrayContext.constArrayAssignment().expressionBlock()
                        .Select(expr => EvaluatorHelper.EvaluateConst(expr.expression(), _assemblyBuilder, type))
                        .ToArray();

                    if (size != content.Length)
                        throw new Exception(
                            $"Invalid const array definition '{constArrayContext.GetText()}'. Invalid items count: expected = {size}, readed = {content.Length}");

                    var symbol =
                        SymbolBuilder.BuildArrOfConst(name, type, content, location); // TODO : Validate params
                    _assemblyBuilder.AddSymbol(symbol);
                }
            }
        }

        public override void ExitConstDef([NotNull] DaedalusParser.ConstDefContext context)
        {
            _assemblyBuilder.IsInsideEvalableStatement = false;
        }

        public override void EnterVarDecl([NotNull] DaedalusParser.VarDeclContext context)
        {
            if (context.Parent is DaedalusParser.DaedalusFileContext || _assemblyBuilder.IsContextInsideExecBlock())
            {
                var typeName = context.typeReference().GetText();
                var type = DatSymbolTypeFromString(typeName);
                if (type == DatSymbolType.Class) //TODO check if it's in all cases
                {
                    type = DatSymbolType.Instance;
                }

                for (int i = 0; i < context.ChildCount; i++)
                {
                    var varContext = context.GetChild(i);

                    if (varContext is TerminalNodeImpl)
                        continue; // skips ',' 

                    if (varContext is DaedalusParser.VarValueDeclContext varValueContext)
                    {
                        var name = varValueContext.nameNode().GetText();
                        if (_assemblyBuilder.IsContextInsideExecBlock())
                        {
                            // TODO consider making assemblyBuilder.active public and using it here
                            ExecBlock execBlock = _assemblyBuilder.ExecBlocks.Last();
                            string execBlockName = execBlock.Symbol.Name;
                            name = $"{execBlockName}.{name}";
                        }

                        var location = GetLocation(context);

                        int parentId = -1;
                        string parameterTypeName = context.typeReference().GetText();
                        DatSymbolType? parameterType = DatSymbolTypeFromString(parameterTypeName);
                        if (parameterType is DatSymbolType.Class)
                        {
                            var parentSymbol = _assemblyBuilder.ResolveSymbol(parameterTypeName);
                            parentId = parentSymbol.Index;
                        }

                        var symbol = SymbolBuilder.BuildVariable(name, type, location, parentId); // TODO : Validate params
                        _assemblyBuilder.AddSymbol(symbol);
                    }

                    if (varContext is DaedalusParser.VarArrayDeclContext varArrayContext)
                    {
                        var name = varArrayContext.nameNode().GetText();
                        var location = GetLocation(context);
                        var size = EvaluatorHelper.EvaluteArraySize(varArrayContext.simpleValue(), _assemblyBuilder);

                        var symbol =
                            SymbolBuilder.BuildArrOfVariables(name, type, (uint) size,
                                location); // TODO : Validate params
                        _assemblyBuilder.AddSymbol(symbol);
                    }
                }
            }
        }

        public override void EnterClassDef([NotNull] DaedalusParser.ClassDefContext context)
        {
            var className = context.nameNode().GetText();
            var classSymbol = SymbolBuilder.BuildClass(className, 0, 0, GetLocation(context));
            _assemblyBuilder.AddSymbol(classSymbol);

            var classId = classSymbol.Index;
            int classVarOffset = 0;
            uint classLength = 0;

            // TODO: refactor later
            foreach (var varDeclContext in context.varDecl())
            {
                var typeName = varDeclContext.typeReference().GetText();
                var type = DatSymbolTypeFromString(typeName);

                for (int i = 0; i < varDeclContext.ChildCount; i++)
                {
                    var varContext = varDeclContext.GetChild(i);

                    if (varContext is TerminalNodeImpl)
                        continue; // skips ',' 

                    if (varContext is DaedalusParser.VarValueDeclContext varValueContext)
                    {
                        var name = varValueContext.nameNode().GetText();
                        var location = GetLocation(context);

                        var symbol = SymbolBuilder.BuildClassVar(name, type, 1, className, classId,
                            classVarOffset, location); // TODO : Validate params
                        _assemblyBuilder.AddSymbol(symbol);

                        classVarOffset += (type == DatSymbolType.String ? 20 : 4);
                        classLength++;
                    }

                    if (varContext is DaedalusParser.VarArrayDeclContext varArrayContext)
                    {
                        var name = varArrayContext.nameNode().GetText();
                        var location = GetLocation(context);
                        var size = EvaluatorHelper.EvaluteArraySize(varArrayContext.simpleValue(), _assemblyBuilder);

                        var symbol = SymbolBuilder.BuildClassVar(name, type, (uint) size, className, classId,
                            classVarOffset, location); // TODO : Validate params
                        _assemblyBuilder.AddSymbol(symbol);

                        classVarOffset += (type == DatSymbolType.String ? 20 : 4) * size;
                        classLength++;
                    }
                }
            }

            classSymbol.ArrayLength = classLength;
            classSymbol.ClassSize = classVarOffset;
        }

        public override void EnterPrototypeDef([NotNull] DaedalusParser.PrototypeDefContext context)
        {
            var prototypeName = context.nameNode().GetText();
            var referenceName = context.referenceNode().GetText();
            var refSymbol = _assemblyBuilder.GetSymbolByName(referenceName);
            var referenceSymbolId = refSymbol.Index;
            var location = GetLocation(context);

            var prototypeSymbol = SymbolBuilder.BuildPrototype(prototypeName, referenceSymbolId, location); // TODO: Validate params
            _assemblyBuilder.AddSymbol(prototypeSymbol);

            _assemblyBuilder.ExecBlockStart(prototypeSymbol, ExecutebleBlockType.PrototypeConstructor);
        }

        public override void ExitPrototypeDef(DaedalusParser.PrototypeDefContext context)
        {
            // we invoke execBlockEnd, thanks that ab will assign all instructions
            // to currently exited prototype constructor
            _assemblyBuilder.AddInstruction(new Ret());
            _assemblyBuilder.ExecBlockEnd();
        }

        public override void EnterInstanceDef(DaedalusParser.InstanceDefContext context)
        {
            var instanceName = context.nameNode().GetText();
            var referenceName = context.referenceNode().GetText();
            var refSymbol = _assemblyBuilder.GetSymbolByName(referenceName);
            var referenceSymbolId = refSymbol.Index;
            var location = GetLocation(context);

            var instanceSymbol = SymbolBuilder.BuildInstance(instanceName, referenceSymbolId, location); // TODO: Validate params
            _assemblyBuilder.AddSymbol(instanceSymbol);

            _assemblyBuilder.ExecBlockStart(instanceSymbol, ExecutebleBlockType.InstanceConstructor);

            if (refSymbol.Type == DatSymbolType.Prototype)
            {
                _assemblyBuilder.AddInstruction(new Call(refSymbol));
            }
        }

        public override void ExitInstanceDef(DaedalusParser.InstanceDefContext context)
        {
            // we invoke execBlockEnd, thanks that ab will assign all instructions
            // to currently exited instance constructor
            _assemblyBuilder.AddInstruction(new Ret());
            _assemblyBuilder.ExecBlockEnd();
        }

        public override void EnterInstanceDecl(DaedalusParser.InstanceDeclContext context)
        {
            var referenceName = context.referenceNode().GetText();
            var refSymbol = _assemblyBuilder.GetSymbolByName(referenceName);
            var referenceSymbolId = refSymbol.Index;
            var location = GetLocation(context);
            
            for (int i = 0; i < context.nameNode().Length; ++i)
            {
                string instanceName = context.nameNode()[i].GetText();
                DatSymbol instanceSymbol = SymbolBuilder.BuildInstance(instanceName, referenceSymbolId, location); // TODO: Validate params
                _assemblyBuilder.AddSymbol(instanceSymbol);
            }
        }

        public override void EnterFunctionDef([NotNull] DaedalusParser.FunctionDefContext context)
        {
            var name = context.nameNode().GetText();
            var typeName = context.typeReference().GetText();
            var type = DatSymbolTypeFromString(typeName);
            uint parametersCount = (uint)context.parameterList().parameterDecl().Length;
            
            var symbol = SymbolBuilder.BuildFunc(name, parametersCount, type); // TODO : Validate params
            _assemblyBuilder.AddSymbol(symbol);
            _assemblyBuilder.ExecBlockStart(symbol, ExecutebleBlockType.Function);
        }

        public override void ExitFunctionDef([NotNull] DaedalusParser.FunctionDefContext context)
        {
            // we invoke execBlockEnd, thanks that ab will assign all instructions
            // to currently exited function
            _assemblyBuilder.GetCurrentSymbol().Location = GetLocation(context);
            _assemblyBuilder.AddInstruction(new Ret());
            _assemblyBuilder.ExecBlockEnd();
        }

        public override void EnterReturnStatement([NotNull] DaedalusParser.ReturnStatementContext context)
        {
            _assemblyBuilder.IsInsideReturnStatement = true;
        }
        
        public override void ExitReturnStatement([NotNull] DaedalusParser.ReturnStatementContext context)
        {
            _assemblyBuilder.AddInstruction(new Ret());
            _assemblyBuilder.IsInsideReturnStatement = false;
        }

        public override void EnterIfBlockStatement(DaedalusParser.IfBlockStatementContext context)
        {
            _assemblyBuilder.ConditionalStart();
        }

        public override void ExitIfBlockStatement(DaedalusParser.IfBlockStatementContext context)
        {
            _assemblyBuilder.ConditionalEnd();
            //assemblyBuilder.ifStatementEnd();
        }


        public override void EnterIfBlock(DaedalusParser.IfBlockContext context)
        {
            _assemblyBuilder.ConditionalBlockConditionStart(IfBlockType.If);
        }

        public override void ExitIfBlock(DaedalusParser.IfBlockContext context)
        {
            _assemblyBuilder.ConditionalBlockBodyEnd();
        }

        public override void EnterElseIfBlock(DaedalusParser.ElseIfBlockContext context)
        {
            _assemblyBuilder.ConditionalBlockConditionStart(IfBlockType.ElseIf);
        }

        public override void ExitElseIfBlock(DaedalusParser.ElseIfBlockContext context)
        {
            _assemblyBuilder.ConditionalBlockBodyEnd();
        }

        public override void EnterElseBlock(DaedalusParser.ElseBlockContext context)
        {
            _assemblyBuilder.ConditionalBlockConditionStart(IfBlockType.Else);
            // else does't have condition so we call here end of condition
            _assemblyBuilder.ConditionalBlockConditionEnd();
        }

        public override void ExitElseBlock(DaedalusParser.ElseBlockContext context)
        {
            _assemblyBuilder.ConditionalBlockBodyEnd();
        }

        public override void EnterIfCondition(DaedalusParser.IfConditionContext context)
        {
            _assemblyBuilder.IsInsideIfCondition = true;
        }
        
        public override void ExitIfCondition(DaedalusParser.IfConditionContext context)
        {
            _assemblyBuilder.ConditionalBlockConditionEnd();
            _assemblyBuilder.IsInsideIfCondition = false;
        }

        public override void EnterBitMoveOperator(DaedalusParser.BitMoveOperatorContext context)
        {
            _assemblyBuilder.ExpressionRightSideStart();
        }

        public override void EnterBitMoveExpression(DaedalusParser.BitMoveExpressionContext context)
        {
            _assemblyBuilder.ExpressionLeftSideStart();
        }

        public override void ExitBitMoveExpression(DaedalusParser.BitMoveExpressionContext context)
        {
            var exprOperator = context.bitMoveOperator().GetText();
            var instruction = AssemblyBuilderHelpers.GetInstructionForOperator(exprOperator);
            _assemblyBuilder.ExpressionEnd(instruction);
        }

        public override void EnterCompExpression(DaedalusParser.CompExpressionContext context)
        {
            _assemblyBuilder.ExpressionLeftSideStart();
        }

        public override void ExitCompExpression(DaedalusParser.CompExpressionContext context)
        {
            var exprOperator = context.compOperator().GetText();
            var instruction = AssemblyBuilderHelpers.GetInstructionForOperator(exprOperator);
            _assemblyBuilder.ExpressionEnd(instruction);
        }

        public override void EnterCompOperator(DaedalusParser.CompOperatorContext context)
        {
            _assemblyBuilder.ExpressionRightSideStart();
        }

        public override void EnterEqExpression(DaedalusParser.EqExpressionContext context)
        {
            _assemblyBuilder.ExpressionLeftSideStart();
        }

        public override void ExitEqExpression(DaedalusParser.EqExpressionContext context)
        {
            var exprOperator = context.eqOperator().GetText();
            var instruction = AssemblyBuilderHelpers.GetInstructionForOperator(exprOperator);
            _assemblyBuilder.ExpressionEnd(instruction);
        }

        public override void EnterEqOperator(DaedalusParser.EqOperatorContext context)
        {
            _assemblyBuilder.ExpressionRightSideStart();
        }


        public List<AssemblyInstruction> GetComplexReferenceNodeInstructions(DaedalusParser.ComplexReferenceNodeContext[] complexReferenceNodes)
        {
            
            ExecBlock activeBlock = _assemblyBuilder.ActiveExecBlock;
            bool isInsideArgList = _assemblyBuilder.IsInsideArgList;
            bool isInsideAssignment = _assemblyBuilder.IsInsideAssignment;
            bool isInsideIfCondition = _assemblyBuilder.IsInsideIfCondition;
            bool isInsideReturnStatement = _assemblyBuilder.IsInsideReturnStatement;

            var symbolPart = complexReferenceNodes[0];
            string symbolName = symbolPart.referenceNode().GetText();

            
            DatSymbol symbol;
            
            if (isInsideArgList)
            {
                if (symbolName.ToLower() == "nofunc")
                {
                    return new List<AssemblyInstruction> { new PushInt(-1) };
                }

                if (symbolName.ToLower() == "null")
                {
                    symbol = _assemblyBuilder.ResolveSymbol($"{(char)255}instance_help");
                    return new List<AssemblyInstruction>
                    {
                        new PushInstance(symbol),
                    };
                }
            }
            
           
            if (activeBlock != null && (activeBlock.Symbol.Type == DatSymbolType.Instance || activeBlock.Symbol.Type == DatSymbolType.Prototype) && (symbolName == "slf" || symbolName == "self"))
            {
                symbol = activeBlock.Symbol;
            }
            else
            {
                symbol = GetComplexReferenceNodeSymbol(complexReferenceNodes[0]);
            }

            if (complexReferenceNodes.Length == 1)
            {
                var simpleValueContext = symbolPart.simpleValue();
                int arrIndex = 0;
                if (simpleValueContext != null)
                {
                    if (!int.TryParse(simpleValueContext.GetText(), out arrIndex))
                    {
                        var constSymbol = _assemblyBuilder.ResolveSymbol(simpleValueContext.GetText());
                        if (!constSymbol.Flags.HasFlag(DatSymbolFlag.Const) || constSymbol.Type != DatSymbolType.Int)
                        {
                            throw new Exception($"Expected integer constant: {simpleValueContext.GetText()}");
                        }

                        arrIndex = (int) constSymbol.Content[0];
                    }
                }

                List<AssemblyInstruction> instructions = new List<AssemblyInstruction>();
                
                if (arrIndex > 0)
                {
                    instructions.Add(new PushArrayVar(symbol, arrIndex));
                }
                else
                {
                    if (isInsideArgList)
                    {
                        DatSymbolType parameterType = _assemblyBuilder.GetParameterType();
                        
                        if (symbol.Type == DatSymbolType.Func || parameterType == DatSymbolType.Func) // TODO check this
                        {
                            instructions.Add(new PushInt(symbol.Index));
                        }
                        else if (symbol.Type == DatSymbolType.Instance && parameterType == DatSymbolType.Int)
                        {
                            instructions.Add(new PushInt(symbol.Index));
                        }
                        else if ( (symbol.Type == DatSymbolType.Instance || symbol.Type == DatSymbolType.Class) && (parameterType == DatSymbolType.Class || parameterType == DatSymbolType.Instance))
                        {
                            instructions.Add(new PushInstance(symbol));
                        }
                        else
                        {
                            instructions.Add(new PushVar(symbol));
                        }
                    }
                    else if (isInsideReturnStatement && activeBlock != null)
                    {
                        if (symbol.Type == DatSymbolType.Instance && activeBlock.Symbol.ReturnType == DatSymbolType.Int)
                        {
                            instructions.Add(new PushInt(symbol.Index));
                        }
                        else
                        {
                            instructions.Add(new PushVar(symbol));
                        }
                    }
                    /*
                    PushInstance jest wtedy kiedy instancja jest po lewej stronie przypisania, np.
                    person = ...
                    oraz kiedy przekazywana jest jako argument do funkcji, gdy odpowiadający parametr to tez instancja, a w zasadzie klasa
                    */
                    else if ((isInsideIfCondition || isInsideAssignment) && symbol.Type == DatSymbolType.Instance) // TODO I think this may be wrong
                    {
                        instructions.Add(new PushInt(symbol.Index));
                    }
                    else if (symbol.Type == DatSymbolType.Instance)
                    {
                        instructions.Add(new PushInstance(symbol));
                    }
                    
                    else
                    {
                        instructions.Add(new PushVar(symbol));
                    }
                    
                }

                return instructions;

            }
            else if (complexReferenceNodes.Length > 1)
            {
                
                var attributePart = complexReferenceNodes[1];
                string attributeName = attributePart.referenceNode().GetText();
                DatSymbol attribute = _assemblyBuilder.ResolveAttribute(symbol, attributeName);              
                
                var simpleValueContext = attributePart.simpleValue();
                int arrIndex = 0;
                if (simpleValueContext != null)
                {
                    if (!int.TryParse(simpleValueContext.GetText(), out arrIndex))
                    {
                        var constSymbol = _assemblyBuilder.ResolveSymbol(simpleValueContext.GetText());
                        if (!constSymbol.Flags.HasFlag(DatSymbolFlag.Const) || constSymbol.Type != DatSymbolType.Int)
                        {
                            throw new Exception($"Expected integer constant: {simpleValueContext.GetText()}");
                        }

                        arrIndex = (int) constSymbol.Content[0];
                    }
                }

                List<AssemblyInstruction> instructions = new List<AssemblyInstruction>();
                
                
                if (activeBlock != null && symbol != activeBlock.Symbol)
                {
                    instructions.Add(new SetInstance(symbol));
                }

                if (arrIndex > 0)
                {
                    instructions.Add(new PushArrayVar(attribute, arrIndex));
                }
                else
                {
                    instructions.Add(new PushVar(attribute));
                }

                return instructions;
            }
            else
            {
                throw new Exception("Unexpected error.");
            }


        }

        
        

        public override void ExitComplexReference(DaedalusParser.ComplexReferenceContext context)
        {
            var complexReferenceNodes = context.complexReferenceNode();
            
            List<AssemblyInstruction> instructions = new List<AssemblyInstruction>();
            if (_assemblyBuilder.IsInsideArgList || _assemblyBuilder.IsInsideAssignment || _assemblyBuilder.IsInsideIfCondition || _assemblyBuilder.IsInsideReturnStatement)
            {
                instructions.Add(new LazyComplexReferenceNodeInstructions(_assemblyBuilder, this, complexReferenceNodes));
            }
            else
            {
                instructions = GetComplexReferenceNodeInstructions(complexReferenceNodes);
            }
            

            if (!_assemblyBuilder.IsInsideEvalableStatement)
            {
                _assemblyBuilder.AddInstructions(instructions.ToArray());   
            }
        }

        private DatSymbol GetComplexReferenceNodeSymbol(DaedalusParser.ComplexReferenceNodeContext complexReferenceNode)
        {
            return _assemblyBuilder.ResolveSymbol(complexReferenceNode.referenceNode().GetText());
        }
        /*
        private DatSymbol GetComplexReferenceNodeSymbol(DaedalusParser.ComplexReferenceNodeContext[] complexReferenceNodes)
        {
            
            
            ExecBlock activeBlock = _assemblyBuilder.ActiveExecBlock;
            var leftPart = complexReferenceNodes[0];
            string leftPartSymbolName = leftPart.referenceNode().GetText();

            
            DatSymbol leftPartSymbol;

            if (activeBlock != null && (activeBlock.Symbol.Type == DatSymbolType.Instance || activeBlock.Symbol.Type == DatSymbolType.Prototype) && (symbolName == "slf" || symbolName == "self"))
            {
                leftPartSymbol = activeBlock.Symbol;
            }
            else
            {
                leftPartSymbol =_assemblyBuilder.ResolveSymbol(leftPartSymbolName);
            }

            var simpleValueContext = symbolPart.simpleValue();
            int leftPartArrayIndex = 0;
            if (simpleValueContext != null)
            {
                if (!int.TryParse(simpleValueContext.GetText(), out leftPartArrayIndex))
                {
                    var constSymbol = _assemblyBuilder.ResolveSymbol(simpleValueContext.GetText());
                    if (!constSymbol.Flags.HasFlag(DatSymbolFlag.Const) || constSymbol.Type != DatSymbolType.Int)
                    {
                        throw new Exception($"Expected integer constant: {simpleValueContext.GetText()}");
                    }

                    leftPartArrayIndex = (int) constSymbol.Content[0];
                }
            }
            
            return leftPartSymbol;
            //return _assemblyBuilder.ResolveSymbol(complexReferenceNode.referenceNode().GetText());
        }
        */

        
        public DatSymbolType GetComplexReferenceType(DaedalusParser.ComplexReferenceNodeContext[] complexReferenceNodes)
        {
            string leftPart = complexReferenceNodes[0].referenceNode().GetText();

            DatSymbol symbol = null;

            DatSymbol activeSymbol = _assemblyBuilder.ActiveExecBlock.Symbol;
            if ((activeSymbol.Type == DatSymbolType.Instance || activeSymbol.Type == DatSymbolType.Prototype) && (leftPart == "slf" || leftPart == "self"))
            {
                symbol = activeSymbol;
            }
            else
            {
                symbol = _assemblyBuilder.ResolveSymbol(leftPart);
            }

            
            
            
            
            if (complexReferenceNodes.Length == 1)
            {
                return symbol.Type;

            }
            if (complexReferenceNodes.Length == 2)
            {
                string rightPart = complexReferenceNodes[1].referenceNode().GetText();
                DatSymbol attribute = _assemblyBuilder.ResolveAttribute(symbol, rightPart);
                return attribute.Type;
            }

            return DatSymbolType.Void;
        }
        
        
        public override void EnterAssignment(DaedalusParser.AssignmentContext context)
        {
            
            var complexReferenceNodes = context.complexReferenceLeftSide().complexReferenceNode();
            List<AssemblyInstruction> instructions = GetComplexReferenceNodeInstructions(complexReferenceNodes);
            _assemblyBuilder.AssigmentStart(Array.ConvertAll(instructions.ToArray(), item => (SymbolInstruction) item));
            _assemblyBuilder.IsInsideAssignment = true;
            
            if (GetComplexReferenceType(complexReferenceNodes) == DatSymbolType.Float)
            {
                _assemblyBuilder.IsInsideFloatAssignment = true;
            }

        }
 
        public override void ExitAssignment(DaedalusParser.AssignmentContext context)
        {
            _assemblyBuilder.IsInsideAssignment = false;
            _assemblyBuilder.IsInsideFloatAssignment = false;
            
            string assignmentOperator = context.assigmentOperator().GetText();

            _assemblyBuilder.AssigmentEnd(assignmentOperator);
        }

        public override void EnterFuncCallValue(DaedalusParser.FuncCallValueContext context)
        {
            _assemblyBuilder.ExpressionLeftSideStart();
            _assemblyBuilder.FuncCallStart();
            
            string funcName = context.funcCall().nameNode().GetText();
            DatSymbol symbol = _assemblyBuilder.GetSymbolByName(funcName);

            /*
            if (_assemblyBuilder.ParametersTypes.Count > 0)
            {
                _assemblyBuilder.ParametersTypesStack.Push(_assemblyBuilder.ParametersTypes);
                _assemblyBuilder.ParametersTypes = new List<DatSymbolType>();
            }
            */

            try
            {
                for (int i = 1; i <= symbol.ParametersCount; ++i)
                {
                    DatSymbol parameter = _assemblyBuilder.Symbols[symbol.Index + i];
                    _assemblyBuilder.ParametersTypes.Add(parameter.Type);
                }
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine($"funcName: {funcName}");
                throw new Exception("dupa");
            }
        }

        public override void ExitFuncCallValue(DaedalusParser.FuncCallValueContext context)
        {
            var funcName = context.funcCall().nameNode().GetText();
            var symbol = _assemblyBuilder.GetSymbolByName(funcName);

            if (symbol == null)
            {
                throw new Exception($"Function '{funcName}' not defined");
            }

            if (symbol.Flags.HasFlag(DatSymbolFlag.External))
            {
                _assemblyBuilder.FuncCallEnd(new CallExternal(symbol));
            }
            else
            {
                _assemblyBuilder.FuncCallEnd(new Call(symbol));
            }
            
            
        }

        public override void EnterFuncArgExpression(DaedalusParser.FuncArgExpressionContext context)
        {
            _assemblyBuilder.FuncCallArgStart();
        }

        public override void ExitFuncArgExpression(DaedalusParser.FuncArgExpressionContext context)
        {
            _assemblyBuilder.FuncCallArgEnd();
        }

        public override void EnterIntegerLiteralValue(DaedalusParser.IntegerLiteralValueContext context)
        {
            if (!_assemblyBuilder.IsInsideEvalableStatement)
            {
                if (_assemblyBuilder.IsInsideFloatAssignment)
                {
                    int parsedFloat = EvaluatorHelper.EvaluateFloatExpression(context.Parent.Parent.GetText());
                    _assemblyBuilder.AddInstruction(new PushInt(parsedFloat));
                }
                else
                {
                    _assemblyBuilder.AddInstruction(new PushInt(int.Parse(context.GetText())));   
                }
                
            }
        }
        
        public override void EnterFloatLiteralValue(DaedalusParser.FloatLiteralValueContext context)
        {
            if (!_assemblyBuilder.IsInsideEvalableStatement)
            {
                int parsedFloat = EvaluatorHelper.EvaluateFloatExpression(context.Parent.Parent.GetText());
                _assemblyBuilder.AddInstruction(new PushInt(parsedFloat));
            }
        }
        

        public override void EnterStringLiteralValue(DaedalusParser.StringLiteralValueContext context)
        {
            if (!_assemblyBuilder.IsInsideEvalableStatement)
            {
                DatSymbolLocation location = GetLocation(context);
                string value = context.GetText().Replace("\"", "");
                string symbolName = _assemblyBuilder.NewStringSymbolName();
                DatSymbol symbol = SymbolBuilder.BuildConst(symbolName, DatSymbolType.String, value, location);
                _assemblyBuilder.AddSymbol(symbol);
                _assemblyBuilder.AddInstruction(new PushVar(symbol));
            }
        }

        public override void EnterAddExpression(DaedalusParser.AddExpressionContext context)
        {
            _assemblyBuilder.ExpressionLeftSideStart();
        }

        public override void ExitAddExpression(DaedalusParser.AddExpressionContext context)
        {
            var exprOperator = context.addOperator().GetText();
            var instruction = AssemblyBuilderHelpers.GetInstructionForOperator(exprOperator);
            _assemblyBuilder.ExpressionEnd(instruction);
        }

        public override void EnterAddOperator(DaedalusParser.AddOperatorContext context)
        {
            //TODO add comment why here we invoke that
            _assemblyBuilder.ExpressionRightSideStart();
        }

        public override void EnterMultOperator(DaedalusParser.MultOperatorContext context)
        {
            //TODO add comment why here we invoke that
            _assemblyBuilder.ExpressionRightSideStart();
        }

        public override void EnterMultExpression(DaedalusParser.MultExpressionContext context)
        {
            _assemblyBuilder.ExpressionLeftSideStart();
        }

        public override void ExitMultExpression(DaedalusParser.MultExpressionContext context)
        {
            var exprOperator = context.multOperator().GetText();
            var instruction = AssemblyBuilderHelpers.GetInstructionForOperator(exprOperator);
            _assemblyBuilder.ExpressionEnd(instruction);
        }

        public override void EnterBinAndOperator(DaedalusParser.BinAndOperatorContext context)
        {
            _assemblyBuilder.ExpressionRightSideStart();
        }

        public override void EnterBinAndExpression(DaedalusParser.BinAndExpressionContext context)
        {
            _assemblyBuilder.ExpressionLeftSideStart();
        }

        public override void ExitBinAndExpression(DaedalusParser.BinAndExpressionContext context)
        {
            var exprOperator = context.binAndOperator().GetText();
            var instruction = AssemblyBuilderHelpers.GetInstructionForOperator(exprOperator);
            _assemblyBuilder.ExpressionEnd(instruction);
        }

        public override void EnterBinOrOperator(DaedalusParser.BinOrOperatorContext context)
        {
            _assemblyBuilder.ExpressionRightSideStart();
        }

        public override void EnterBinOrExpression(DaedalusParser.BinOrExpressionContext context)
        {
            _assemblyBuilder.ExpressionLeftSideStart();
        }

        public override void ExitBinOrExpression(DaedalusParser.BinOrExpressionContext context)
        {
            var exprOperator = context.binOrOperator().GetText();
            var instruction = AssemblyBuilderHelpers.GetInstructionForOperator(exprOperator);
            _assemblyBuilder.ExpressionEnd(instruction);
        }

        public override void EnterLogAndOperator(DaedalusParser.LogAndOperatorContext context)
        {
            _assemblyBuilder.ExpressionRightSideStart();
        }

        public override void EnterLogAndExpression(DaedalusParser.LogAndExpressionContext context)
        {
            _assemblyBuilder.ExpressionLeftSideStart();
        }

        public override void ExitLogAndExpression(DaedalusParser.LogAndExpressionContext context)
        {
            var exprOperator = context.logAndOperator().GetText();
            var instruction = AssemblyBuilderHelpers.GetInstructionForOperator(exprOperator);
            _assemblyBuilder.ExpressionEnd(instruction);
        }

        public override void EnterLogOrOperator(DaedalusParser.LogOrOperatorContext context)
        {
            _assemblyBuilder.ExpressionRightSideStart();
        }

        public override void EnterLogOrExpression(DaedalusParser.LogOrExpressionContext context)
        {
            _assemblyBuilder.ExpressionLeftSideStart();
        }

        public override void ExitLogOrExpression(DaedalusParser.LogOrExpressionContext context)
        {
            var exprOperator = context.logOrOperator().GetText();
            var instruction = AssemblyBuilderHelpers.GetInstructionForOperator(exprOperator);
            _assemblyBuilder.ExpressionEnd(instruction);
        }

        public override void ExitOneArgExpression(DaedalusParser.OneArgExpressionContext context)
        {
            if (!_assemblyBuilder.IsInsideEvalableStatement && !_assemblyBuilder.IsInsideFloatAssignment)
            {
                var exprOperator = context.oneArgOperator().GetText();
                var instruction = AssemblyBuilderHelpers.GetInstructionForOperator(exprOperator, false);
                _assemblyBuilder.AddInstruction(instruction);   
            }
        }
        
        private DatSymbolType DatSymbolTypeFromString(string typeName)
        {
            string capitalizedTypeName = typeName.First().ToString().ToUpper() + typeName.Substring(1).ToLower();

            if (Enum.TryParse(capitalizedTypeName, out DatSymbolType type))
            {
                return type;
            }
            else
            {
                var symbol = _assemblyBuilder.ResolveSymbol(typeName);
                return symbol.Type;
            }
        }

        private DatSymbolLocation GetLocation(ParserRuleContext context)
        {
            return new DatSymbolLocation
            {
                FileNumber = _sourceFileNumber,
                Line = context.Start.Line,
                LinesCount = context.Stop.Line - context.Start.Line + 1,
                Position = context.Start.StartIndex,
                PositionsCount = context.Stop.StopIndex - context.Start.StartIndex + 1
            };
        }
    }
}