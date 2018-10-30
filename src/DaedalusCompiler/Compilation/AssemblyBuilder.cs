﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DaedalusCompiler.Dat;

namespace DaedalusCompiler.Compilation
{
    public enum ExecutebleBlockType
    {
        Function,
        InstanceConstructor,
        PrototypeConstructor
    }

    public class AssemblyElement
    {
    }

    public class AssemblyInstruction : AssemblyElement
    {
    }

    public class AssemblyLabel : AssemblyElement
    {
        public readonly string Label;

        public AssemblyLabel(string label)
        {
            Label = label;
        }
    }

    public class IfBlock
    {
        public List<AssemblyElement> Body;
        public List<AssemblyElement> Condition;

        public IfBlock()
        {
            Body = new List<AssemblyElement>();
            Condition = new List<AssemblyElement>();
        }
    }

    public enum IfBlockType
    {
        If,
        ElseIf,
        Else
    }

    public class AssemblyIfStatement : AssemblyElement
    {
        public IfBlock IfBlock;
        public List<AssemblyElement> ElseBody;
        public bool HasElseBody;
        public readonly List<IfBlock> ElseIfBlock;
        public IfBlockType CurrentBlockType;
        public List<AssemblyElement> ConditionInstructionStack;


        public AssemblyIfStatement()
        {
            IfBlock = new IfBlock();
            ElseBody = new List<AssemblyElement>();
            HasElseBody = false;
            ElseIfBlock = new List<IfBlock>();
        }
    }

    public abstract class BaseExecBlock : AssemblyElement
    {
        public List<AssemblyElement> Body;
        public abstract DatSymbol GetSymbol();
    }
    
    public class ExecBlock : BaseExecBlock
    {
        public DatSymbol Symbol;

        public override DatSymbol GetSymbol()
        {
            return Symbol;
        }
    }
    
    public class SharedExecBlock : BaseExecBlock
    {
        public List<DatSymbol> Symbols;
        public override DatSymbol GetSymbol()
        {
            return Symbols.FirstOrDefault();
        }
    }
    
    public class FunctionBlock : ExecBlock
    {
    }

    public class InstanceConstructorBlock : ExecBlock
    {
    }

    public class PrototypeContructorBlock : ExecBlock
    {
    }

    public class SymbolInstruction : AssemblyInstruction
    {
        public readonly DatSymbol Symbol;

        protected SymbolInstruction(DatSymbol symbol)
        {
            Symbol = symbol;
        }
    }

    public class ValueInstruction : AssemblyInstruction
    {
        public readonly object Value;

        protected ValueInstruction(object value)
        {
            Value = value;
        }
    }

    public abstract class AddressInstruction : AssemblyInstruction
    {
        public readonly int Address;

        protected AddressInstruction(int address)
        {
            Address = address;
        }
    }

    public class JumpToLabel : AssemblyInstruction
    {
        public readonly string Label;

        public JumpToLabel(string label)
        {
            Label = label;
        }
    }

    public class ParamLessInstruction : AssemblyInstruction
    {
    }

    public class PushInt : ValueInstruction
    {
        public PushInt(int value) : base(value)
        {
        }
    }

    public class PushVar : SymbolInstruction
    {
        public PushVar(DatSymbol symbol) : base(symbol)
        {
        }
    }

    public class PushArrayVar : SymbolInstruction
    {
        public readonly int Index;

        public PushArrayVar(DatSymbol symbol, int index) : base(symbol)
        {
            Index = index;
        }
    }

    public class PushInstance : SymbolInstruction
    {
        public PushInstance(DatSymbol symbol) : base(symbol)
        {
        }
    }

    public class SetInstance : SymbolInstruction
    {
        public SetInstance(DatSymbol symbol) : base(symbol)
        {
        }
    }

    public class Equal : ParamLessInstruction {}
    public class NotEqual : ParamLessInstruction {}
    public class Less : ParamLessInstruction {}
    public class LessOrEqual : ParamLessInstruction {}
    public class Greater : ParamLessInstruction {}
    public class GreaterOrEqual : ParamLessInstruction {}
    public class Assign : ParamLessInstruction {}
    public class AssignAdd : ParamLessInstruction {}
    public class AssignSubtract : ParamLessInstruction {}
    public class AssignMultiply : ParamLessInstruction {}
    public class AssignDivide : ParamLessInstruction {}
    public class AssignString : ParamLessInstruction {}
    // public class AssignStringRef : ParamLessInstruction {}
    public class AssignFunc : ParamLessInstruction {}
    public class AssignFloat : ParamLessInstruction {}
    public class AssignInstance : ParamLessInstruction {}
    public class Ret : ParamLessInstruction {}
    public class Add : ParamLessInstruction {}
    public class Multiply : ParamLessInstruction {}
    public class Divide : ParamLessInstruction {}
    public class Subtract : ParamLessInstruction {}
    public class Modulo : ParamLessInstruction {}
    public class Not: ParamLessInstruction {}
    public class Minus: ParamLessInstruction {}
    public class Plus: ParamLessInstruction {}
    public class Negate: ParamLessInstruction {}
    public class ShiftLeft: ParamLessInstruction {}
    public class ShiftRight: ParamLessInstruction {}
    public class BitAnd: ParamLessInstruction {}
    public class BitOr: ParamLessInstruction {}
    public class LogAnd: ParamLessInstruction {}
    public class LogOr: ParamLessInstruction {}
    
    public class JumpIfToLabel : JumpToLabel
    {
        public JumpIfToLabel(string name) : base(name)
        {
        }
    }
    //public class Call : LabelJumpInstruction {}

    public class Call : SymbolInstruction
    {
        public Call(DatSymbol symbol) : base(symbol)
        {
        }
    }

    public class CallExternal : SymbolInstruction
    {
        public CallExternal(DatSymbol symbol) : base(symbol)
        {
        }
    }

    public class LazyReferenceAtomInstructions : AssemblyInstruction
    {
        public readonly DaedalusParser.ReferenceAtomContext[] ReferenceAtoms;
        public readonly AssemblyBuilderSnapshot AssemblyBuilderSnapshot;
        
        public LazyReferenceAtomInstructions(
            AssemblyBuilderSnapshot assemblyBuilderSnapshot,
            DaedalusParser.ReferenceAtomContext[] referenceAtoms)
        {
            AssemblyBuilderSnapshot = assemblyBuilderSnapshot;
            ReferenceAtoms = referenceAtoms;
        }
    }

    public class AssemblyBuilderSnapshot
    {
        public readonly BaseExecBlock ActiveExecBlock;
        
        public readonly bool IsInsideArgList;
        public readonly bool IsInsideAssignment;
        public readonly bool IsInsideIfCondition;
        public readonly bool IsInsideReturnStatement;
        public readonly DatSymbolType AssignmentType;
        public readonly FunctionCallContext FuncCallCtx;

        public AssemblyBuilderSnapshot(AssemblyBuilder assemblyBuilder)
        {
            ActiveExecBlock = assemblyBuilder.ActiveExecBlock;
           
            IsInsideArgList = assemblyBuilder.IsInsideArgList;
            IsInsideAssignment = assemblyBuilder.IsInsideAssignment;
            IsInsideIfCondition = assemblyBuilder.IsInsideIfCondition;
            IsInsideReturnStatement = assemblyBuilder.IsInsideReturnStatement;
            AssignmentType = assemblyBuilder.AssignmentType;

            if (assemblyBuilder.FuncCallCtx == null)
            {
                FuncCallCtx = null;
            }
            else
            {
                FuncCallCtx = new FunctionCallContext(assemblyBuilder.FuncCallCtx);
            }
            
        }
    }
    
    public class AssemblyBuildContext
    {
        public AssemblyIfStatement CurrentConditionStatement;
        public List<AssemblyElement> Body;
        public AssemblyBuildContext Parent;
    }


    public class BaseExpressionContext
    {
        public readonly BaseExpressionContext Parent;

        protected BaseExpressionContext(BaseExpressionContext parent)
        {
            Parent = parent;
        }

        public virtual void SetEndInstruction(AssemblyElement element)
        {
            
        }
        public virtual List<AssemblyElement> GetInstructions()
        {
            return null;
            
        }
        public virtual void AddInstruction(AssemblyElement element)
        {
            
        }
        public virtual void AddInstructions(List<AssemblyElement> elements) {
    
        }
    }

    
    
    public class StackBasedExpressionContext : BaseExpressionContext
    {
        private readonly Stack<List<AssemblyElement>> _instructionsStack;

        
        public StackBasedExpressionContext(BaseExpressionContext parent) : base(parent)
        {
            _instructionsStack = new Stack<List<AssemblyElement>>();
        }
        
        public override List<AssemblyElement> GetInstructions()
        {
            List<AssemblyElement> totalInstructions = new List<AssemblyElement>();

            foreach (var instructions in _instructionsStack)
            {
                totalInstructions.AddRange(instructions);
            }
            
            return totalInstructions;
        }
        
        public override void AddInstruction(AssemblyElement element)
        {
            AddInstructions(new List<AssemblyElement>() {element});
        }

        public override void AddInstructions(List<AssemblyElement> elements)
        {
            _instructionsStack.Push(elements);
        }
        
    }

    public class OperatorExpressionContext : StackBasedExpressionContext
    {
        
        private AssemblyElement _operatorInstruction;
        
        
        public OperatorExpressionContext(BaseExpressionContext parent) : base(parent)
        {
            _operatorInstruction = null;
        }
        
        public override void SetEndInstruction(AssemblyElement element)
        {
            _operatorInstruction = element;
        }
        
        public override List<AssemblyElement> GetInstructions()
        {
            List<AssemblyElement> instructions = base.GetInstructions();  // TODO check which one this calls

            if (_operatorInstruction != null)
            {
                instructions.Add(_operatorInstruction);
            }

            return instructions;
        }
    }
    
    
    public class FunctionCallContext : BaseExpressionContext
    {
        private readonly List<AssemblyElement> _instructions;
        private AssemblyElement _callInstruction;

        private readonly List<DatSymbolType> _parametersTypes;
        public int ArgIndex;
        public FunctionCallContext OuterCall;
        
        public FunctionCallContext(BaseExpressionContext parent, FunctionCallContext outerCall, List<DatSymbolType> parametersTypes) : base(parent)
        {
            _parametersTypes = parametersTypes;
            ArgIndex = -1;
            OuterCall = outerCall;
            _instructions = new List<AssemblyElement>();
        }
        
        public FunctionCallContext(FunctionCallContext ctx) : base(ctx.Parent)
        {
            _parametersTypes = ctx._parametersTypes;
            ArgIndex = ctx.ArgIndex;
            OuterCall = ctx.OuterCall;
            _instructions = ctx._instructions;
        }
        

        public override void SetEndInstruction(AssemblyElement element)
        {
            _callInstruction = element;
        }

        public override List<AssemblyElement> GetInstructions()
        {
            List<AssemblyElement> instructions = _instructions;
            instructions.Add(_callInstruction);
            return instructions;
        }
        
        public override void AddInstruction(AssemblyElement element)
        {
            AddInstructions(new List<AssemblyElement>() {element});
        }

        public override void AddInstructions(List<AssemblyElement> elements)
        {
            _instructions.AddRange(elements);
        }
        
        public DatSymbolType GetParameterType()
        {
            return _parametersTypes[ArgIndex];
        }
    }


    public class AssemblyBuilder
    {
        public readonly List<BaseExecBlock> ExecBlocks;
        public List<DatSymbol> Symbols;
        public Dictionary<string, DatSymbol> SymbolsDict;
        public BaseExecBlock ActiveExecBlock;
        private AssemblyBuildContext _currentBuildCtx;
        public BaseExpressionContext CurrentExprCtx;
        private List<SymbolInstruction> _assignmentLeftSide;
        public FunctionCallContext FuncCallCtx;
        private int _labelIndexGenerator;
        private int _nextStringSymbolNumber;
        public bool IsInsideEvalableStatement;
        public bool IsCurrentlyParsingExternals;

        public bool IsInsideArgList;
        public bool IsInsideAssignment;
        public bool IsInsideIfCondition;
        public bool IsInsideReturnStatement;
        public DatSymbolType AssignmentType;
        private int _nextSymbolIndex;
        
        public AssemblyBuilder()
        {
            ExecBlocks = new List<BaseExecBlock>();
            Symbols = new List<DatSymbol>();
            SymbolsDict = new Dictionary<string, DatSymbol>();
            _currentBuildCtx = GetEmptyBuildContext();
            CurrentExprCtx = null;
            ActiveExecBlock = null;
            _assignmentLeftSide = new List<SymbolInstruction>();
            FuncCallCtx = null;
            _labelIndexGenerator = 0;
            _nextStringSymbolNumber = 10000;
            IsInsideEvalableStatement = false;
            IsCurrentlyParsingExternals = false;
            
            IsInsideArgList = false;
            IsInsideAssignment = false;
            IsInsideReturnStatement = false;
            AssignmentType = DatSymbolType.Void;
            _nextSymbolIndex = 0;
        }
        
        public AssemblyBuilderSnapshot MakeSnapshot()
        {
            return new AssemblyBuilderSnapshot(this);
        }

        public void LoadStateFromSnapshot(AssemblyBuilderSnapshot snapshot)
        {
            ActiveExecBlock = snapshot.ActiveExecBlock;
            
            IsInsideArgList = snapshot.IsInsideArgList;
            IsInsideAssignment = snapshot.IsInsideAssignment;
            IsInsideIfCondition = snapshot.IsInsideIfCondition;
            IsInsideReturnStatement = snapshot.IsInsideReturnStatement;
            AssignmentType = snapshot.AssignmentType;
            FuncCallCtx = snapshot.FuncCallCtx;
        }

        public string NewStringSymbolName()
        {
            return $"{(char) 255}{_nextStringSymbolNumber++}";
        }

        public DatSymbol GetCurrentSymbol()
        {
            return ActiveExecBlock.GetSymbol();
        }
        
        public bool IsArgListKeyword(string symbolName)
        {
            return symbolName == "nofunc" || symbolName == "null";
        }

        public List<AssemblyElement> GetKeywordInstructions(string symbolName)
        {
            if (symbolName == "nofunc")
            {
                return new List<AssemblyElement> { new PushInt(-1) };
            }

            if (symbolName == "null")
            {
                DatSymbol symbol = ResolveSymbol($"{(char)255}instance_help");
                return new List<AssemblyElement> { new PushInstance(symbol) };
            }
            
            return new List<AssemblyElement>();
        }
        
        private DatSymbol GetReferenceAtomSymbol(DaedalusParser.ReferenceAtomContext[] referenceAtoms)
        {
            var referenceAtom = referenceAtoms[0];
            string symbolNameLower = referenceAtom.Identifier().GetText().ToLower();
            bool isSymbolNameSelfKeyword = symbolNameLower == "slf" || symbolNameLower == "self";


            if (ActiveExecBlock != null && isSymbolNameSelfKeyword)
            {

                bool isDottedReference = IsDottedReference(referenceAtoms);
                DatSymbolType activeSymbolType = ActiveExecBlock.GetSymbol().Type;

                if (
                    activeSymbolType == DatSymbolType.Instance
                    || (activeSymbolType == DatSymbolType.Prototype && isDottedReference)
                )
                {
                    return ActiveExecBlock.GetSymbol();
                }
            }

            return ResolveSymbol(referenceAtom.Identifier().GetText());
        }
        
        public DatSymbolType GetReferenceType(DaedalusParser.ReferenceAtomContext[] referenceAtoms)
        {
            DatSymbol symbol = GetReferenceAtomSymbol(referenceAtoms);
   
            if (IsDottedReference(referenceAtoms))
            {
                string rightPart = referenceAtoms[1].Identifier().GetText();
                DatSymbol attribute = ResolveAttribute(symbol, rightPart);
                return attribute.Type;
            }
            return symbol.Type;
        }
        
        public int GetArrayIndex(DaedalusParser.ReferenceAtomContext context)
        {
            var indexContext = context.arrayIndex();
            
            
            int arrIndex = 0;
            if (indexContext != null)
            {
                if (!int.TryParse(indexContext.GetText(), out arrIndex))
                {
                    var constSymbol = ResolveSymbol(indexContext.GetText());
                    if (!constSymbol.Flags.HasFlag(DatSymbolFlag.Const) || constSymbol.Type != DatSymbolType.Int)
                    {
                        throw new Exception($"Expected integer constant: {indexContext.GetText()}");
                    }

                    arrIndex = (int) constSymbol.Content[0];
                }
            }

            return arrIndex;
        }
        
        public AssemblyInstruction PushSymbol(DatSymbol symbol, DatSymbolType? asType=null)
        {
            if (asType == DatSymbolType.Func || (asType == DatSymbolType.Int && symbol.Type != DatSymbolType.Int))
            {
                return new PushInt(symbol.Index);
            }

            if (symbol.Type == DatSymbolType.Instance || asType == DatSymbolType.Instance)  /* DatSymbolType.Class isn't possible */
            {
                return new PushInstance(symbol);
            }
            return new PushVar(symbol);
        }
        
        public AssemblyElement GetProperPushInstruction(DatSymbol symbol, int arrIndex)
        {
            BaseExecBlock activeBlock = ActiveExecBlock;
            
            if (arrIndex > 0)
            {
                return new PushArrayVar(symbol, arrIndex);
            }
            
            if (IsInsideArgList)
            {
                return PushSymbol(symbol, FuncCallCtx.GetParameterType());
            }
            
            if (IsInsideReturnStatement && activeBlock != null)
            {
                return PushSymbol(symbol, activeBlock.GetSymbol().ReturnType);
            }
            
            if (IsInsideAssignment)
            {
                return PushSymbol(symbol, AssignmentType);
            }
            
            if (IsInsideIfCondition)
            {
                return PushSymbol(symbol, DatSymbolType.Int);
            }

            return PushSymbol(symbol);
        }

        public bool IsDottedReference(DaedalusParser.ReferenceAtomContext[] nodes)
        {
            if (nodes.Length > 2)
            {
                throw new Exception("Too many nodes in reference.");
            }
            return nodes.Length == 2;
        }
        
        public List<AssemblyElement> GetReferenceAtomInstructions(
            DaedalusParser.ReferenceAtomContext[] referenceAtoms)
        {
            var symbolPart = referenceAtoms[0];
            string symbolName = symbolPart.Identifier().GetText().ToLower();


            if (IsInsideArgList && IsArgListKeyword(symbolName))
            {
                return GetKeywordInstructions(symbolName);
            }

            DatSymbol symbol = GetReferenceAtomSymbol(referenceAtoms);
            List<AssemblyElement> instructions = new List<AssemblyElement>();
            
            
            if (IsDottedReference(referenceAtoms))
            {
                var attributePart = referenceAtoms[1];
                string attributeName = attributePart.Identifier().GetText();
                DatSymbol attribute = ResolveAttribute(symbol, attributeName);

                bool isInsideExecBlock = ActiveExecBlock != null;
                bool isSymbolSelf = symbol == ActiveExecBlock?.GetSymbol(); // self.attribute, slf.attribute cases
                bool isSymbolPassedToInstanceParameter = IsInsideArgList && FuncCallCtx.GetParameterType() == DatSymbolType.Instance;
                bool isSymbolPassedToFuncParameter = IsInsideArgList && FuncCallCtx.GetParameterType() == DatSymbolType.Func;
                bool isInsideFuncAssignment = IsInsideAssignment && AssignmentType == DatSymbolType.Func;
                
                if (isInsideExecBlock
                    && !isSymbolSelf
                    && !isInsideFuncAssignment
                    && !(isSymbolPassedToInstanceParameter || isSymbolPassedToFuncParameter)
                )
                {
                    instructions.Add(new SetInstance(symbol));
                }
                
                int arrIndex = GetArrayIndex(attributePart);
                instructions.Add(GetProperPushInstruction(attribute, arrIndex));
                return instructions;
            }
            else
            {
                int arrIndex = GetArrayIndex(symbolPart);
                instructions.Add(GetProperPushInstruction(symbol, arrIndex));
                return instructions;
            }

        }

        private AssemblyBuildContext GetEmptyBuildContext(bool isOperatorContext = false)
        {
            return new AssemblyBuildContext
            {
                Body = new List<AssemblyElement>(),
                Parent = _currentBuildCtx,
                CurrentConditionStatement = new AssemblyIfStatement(),
            };
        }

        public bool IsContextInsideExecBlock()
        {
            return ActiveExecBlock != null;
        }

        public void AddInstruction(AssemblyInstruction instruction)
        {
            AddInstructions(new List<AssemblyElement>() {instruction});
        }

        public void AddInstructions(List<AssemblyElement> instructions)
        {
            if (CurrentExprCtx != null)
            {
                CurrentExprCtx.AddInstructions(instructions);
            }
            else
            {
                _currentBuildCtx.Body.AddRange(instructions);
            }
        }

        public void SharedBlockStart(List<DatSymbol> symbols)
        {
            ActiveExecBlock = new SharedExecBlock {Symbols = symbols};
            ExecBlocks.Add(ActiveExecBlock);
            _currentBuildCtx = GetEmptyBuildContext();
        }
        
        public void ExecBlockStart(DatSymbol symbol, ExecutebleBlockType blockType)
        {
            switch (blockType)
            {
                case ExecutebleBlockType.Function:
                    ActiveExecBlock = new FunctionBlock {Symbol = symbol};
                    break;
                case ExecutebleBlockType.InstanceConstructor:
                    ActiveExecBlock = new InstanceConstructorBlock {Symbol = symbol};
                    break;
                case ExecutebleBlockType.PrototypeConstructor:
                    ActiveExecBlock = new PrototypeContructorBlock {Symbol = symbol};
                    break;
            }

            ExecBlocks.Add(ActiveExecBlock);
            _currentBuildCtx = GetEmptyBuildContext();
        }

        public void ExecBlockEnd()
        {
            ActiveExecBlock.Body = _currentBuildCtx.Body;
            ActiveExecBlock = null;

            _currentBuildCtx = _currentBuildCtx.Parent;
        }

        public void AssigmentStart(SymbolInstruction[] instructions)
        {
            _assignmentLeftSide.AddRange(instructions);
        }

        public void AssigmentEnd(string assignmentOperator)
        {
            //TODO check if there are any possibilities of assignmentLeftSide longer than 2 instructions?
            var operationType = _assignmentLeftSide.Last().Symbol.Type; 
            var assignmentInstruction =
                AssemblyBuilderHelpers.GetInstructionForOperator(assignmentOperator, true, operationType);

            if (!IsInsideEvalableStatement)
            {
                AddInstructions(new List<AssemblyElement>(_assignmentLeftSide));   
            }
            _assignmentLeftSide = new List<SymbolInstruction>();
            AddInstruction(assignmentInstruction);
        }

        public void ExitOperator(string operatorText, bool twoArg=true)
        {
            var instruction = AssemblyBuilderHelpers.GetInstructionForOperator(operatorText, twoArg);
            CurrentExprCtx.SetEndInstruction(instruction);
        }

       

        public void FuncCallStart(string funcName)
        {
            DatSymbol symbol = GetSymbolByName(funcName);
            
            List<DatSymbolType> parametersTypes = new List<DatSymbolType>();
            for (int i = 1; i <= symbol.ParametersCount; ++i)
            {
                DatSymbol parameter = Symbols[symbol.Index + i];
                parametersTypes.Add(parameter.Type);
            }
            
            AssemblyInstruction instruction;
            if (symbol.Flags.HasFlag(DatSymbolFlag.External))
            {
                instruction = new CallExternal(symbol);
            }
            else
            {
                instruction = new Call(symbol);
            }

            FuncCallCtx = new FunctionCallContext(CurrentExprCtx, FuncCallCtx, parametersTypes);
            CurrentExprCtx = FuncCallCtx;
            CurrentExprCtx.SetEndInstruction(instruction);
            
            IsInsideArgList = true;
        }

        public void FuncCallEnd()
        {
            List<AssemblyElement> instructions = CurrentExprCtx.GetInstructions();
            FuncCallCtx = FuncCallCtx.OuterCall;
            CurrentExprCtx = CurrentExprCtx.Parent;
            AddInstructions(instructions);
            
            if (FuncCallCtx == null)
            {
                IsInsideArgList = false;
            }
        }
        
        public void FuncCallArgStart()
        {
            FuncCallCtx.ArgIndex++;
            CurrentExprCtx = new StackBasedExpressionContext(CurrentExprCtx);
        }

        public void FuncCallArgEnd()
        {
            List<AssemblyElement> instructions = CurrentExprCtx.GetInstructions();
            CurrentExprCtx = CurrentExprCtx.Parent;
            AddInstructions(instructions);
        }

        public void OperatorExpressionStart()
        {
            CurrentExprCtx = new OperatorExpressionContext(CurrentExprCtx);
        }

        public void OperatorExpressionEnd()
        {
            List<AssemblyElement> instructions = CurrentExprCtx.GetInstructions();
            CurrentExprCtx = CurrentExprCtx.Parent;
            AddInstructions(instructions);
        }


        public void ConditionalStart()
        {
        }

        public void ConditionalEnd()
        {
            _currentBuildCtx.Body.AddRange(ResolveIfStatement(_currentBuildCtx.CurrentConditionStatement));

            _currentBuildCtx.CurrentConditionStatement = new AssemblyIfStatement();
        }

        public void ConditionalBlockConditionStart(IfBlockType blockType)
        {
            _currentBuildCtx.CurrentConditionStatement.CurrentBlockType = blockType;

            _currentBuildCtx = GetEmptyBuildContext();
        }

        public void ConditionalBlockConditionEnd()
        {
            var body = _currentBuildCtx.Body;
            // we need firstly get out from condition context
            _currentBuildCtx = _currentBuildCtx.Parent;
            _currentBuildCtx.CurrentConditionStatement.ConditionInstructionStack = body;

            // we need create context for statement block
            _currentBuildCtx = GetEmptyBuildContext();
        }

        public void ConditionalBlockBodyEnd()
        {
            var body = _currentBuildCtx.Body;
            _currentBuildCtx = _currentBuildCtx.Parent;

            var blocktype = _currentBuildCtx.CurrentConditionStatement.CurrentBlockType;

            if (blocktype == IfBlockType.If || blocktype == IfBlockType.ElseIf)
            {
                var ifBlock = new IfBlock
                {
                    Body = body,
                    Condition = _currentBuildCtx.CurrentConditionStatement.ConditionInstructionStack
                };

                if (blocktype == IfBlockType.If)
                {
                    _currentBuildCtx.CurrentConditionStatement.IfBlock = ifBlock;
                }
                else
                {
                    _currentBuildCtx.CurrentConditionStatement.ElseIfBlock.Add(ifBlock);
                }
            }
            else
            {
                _currentBuildCtx.CurrentConditionStatement.HasElseBody = true;
                _currentBuildCtx.CurrentConditionStatement.ElseBody = body;
            }
        }

        public void AddSymbol(DatSymbol symbol)
        {
            if (IsCurrentlyParsingExternals)
            {
                if (symbol.Type == DatSymbolType.Func && symbol.Flags.HasFlag(DatSymbolFlag.Const))
                {
                    symbol.Flags |= DatSymbolFlag.External;
                }

                if (symbol.Name == "instance_help")
                {
                    symbol.Name = $"{(char) 255}instance_help";
                }
            }
            
            SymbolsDict[symbol.Name.ToUpper()] = symbol;
            
            Symbols.Add(symbol);
            symbol.Index = _nextSymbolIndex;
            _nextSymbolIndex++;
        }

        public DatSymbol ResolveAttribute(DatSymbol symbol, string attributeName)
        {
            string attributePath = $"{symbol.Name}.{attributeName}";

            DatSymbol attributeSymbol = null;
            
            while (symbol != null)
            {
                attributeSymbol = SymbolsDict.GetValueOrDefault(attributePath.ToUpper(), null);
                
                if (attributeSymbol == null)
                {
                    if (symbol.ParentIndex == DatSymbol.NULL_INDEX)
                    {
                        break;
                    }

                    symbol = Symbols[symbol.ParentIndex];
                    attributePath = $"{symbol.Name}.{attributeName}";
                    
                    if (symbol.Type == DatSymbolType.Prototype && symbol.ParentIndex != DatSymbol.NULL_INDEX)
                    {
                        symbol = Symbols[symbol.ParentIndex];
                        attributePath = $"{symbol.Name}.{attributeName}";
                    }
                    
                }
                else
                {
                    break;
                }
            }
            
            if (attributeSymbol == null)
            {
                throw new Exception($"attributeSymbol {symbol.Name}.{attributeName} is not added");
            }

            return attributeSymbol;
            
        }
        
        public DatSymbol ResolveSymbol(string symbolName)
        {
            DatSymbol symbol;

            if (ActiveExecBlock != null && !symbolName.Contains("."))
            {
                DatSymbol currentExecBlockSymbol = ActiveExecBlock.GetSymbol();

                while (currentExecBlockSymbol != null)
                {
                    var targetSymbolName = $"{currentExecBlockSymbol.Name}.{symbolName}";
                    symbol = SymbolsDict.GetValueOrDefault(targetSymbolName.ToUpper(), null);
                    
                    
                    if (symbol == null)
                    {
                        if (currentExecBlockSymbol.ParentIndex == DatSymbol.NULL_INDEX)
                        {
                            break;
                        }

                        currentExecBlockSymbol = Symbols[currentExecBlockSymbol.ParentIndex];
                    }
                    else
                    {
                        return symbol;
                    }
                }
            }

            symbol = SymbolsDict.GetValueOrDefault(symbolName.ToUpper(), null);
            
            if (symbol == null)
            {
                throw new Exception("Symbol " + symbolName + " is not added");
            }

            return symbol;
        }

        public DatSymbol GetSymbolByName(string symbolName)
        {
            return SymbolsDict[symbolName.ToUpper()];
        }

        private string GetNextLabel()
        {
            var labelVal = _labelIndexGenerator;

            _labelIndexGenerator++;

            return $"label_{labelVal}";
        }

        private List<AssemblyElement> ResolveIfStatement(AssemblyIfStatement ifStatement)
        {
            var instructions = new List<AssemblyElement>();
            var ifBlocks = new List<IfBlock>();
            var haveElse = ifStatement.HasElseBody;
            var statementEndLabel = GetNextLabel();
            var elseStartLabel = "";

            ifBlocks.Add(ifStatement.IfBlock);
            ifBlocks.AddRange(ifStatement.ElseIfBlock);

            foreach (var ifBlock in ifBlocks)
            {
                var isLastOne = ifBlock == ifBlocks.Last();

                instructions.AddRange(ifBlock.Condition);

                if (!isLastOne)
                {
                    var nextJumpLabel = GetNextLabel();

                    instructions.Add(new JumpIfToLabel(nextJumpLabel));
                    instructions.AddRange(ifBlock.Body);
                    instructions.Add(new JumpToLabel(statementEndLabel));
                    instructions.Add(new AssemblyLabel(nextJumpLabel));
                }
                else
                {
                    if (haveElse)
                    {
                        elseStartLabel = GetNextLabel();
                        instructions.Add(new JumpIfToLabel(elseStartLabel));
                        instructions.AddRange(ifBlock.Body);
                        instructions.Add(new JumpToLabel(statementEndLabel));
                    }
                    else
                    {
                        instructions.Add(new JumpIfToLabel(statementEndLabel));
                        instructions.AddRange(ifBlock.Body);
                    }
                }
            }

            if (haveElse)
            {
                instructions.Add(new AssemblyLabel(elseStartLabel));
                instructions.AddRange(ifStatement.ElseBody);
            }

            instructions.Add(new AssemblyLabel(statementEndLabel));

            return instructions;
        }

        public bool IsInsideOneArgOperatorsEvaluationMode()
        {
            bool isInsideFloatAssignment = IsInsideAssignment && AssignmentType == DatSymbolType.Float;
            bool isInsideFloatArgument = IsInsideArgList && FuncCallCtx.GetParameterType() == DatSymbolType.Float;
            return IsInsideEvalableStatement || isInsideFloatAssignment || isInsideFloatArgument;
        }


        public string GetAssembler()
        {
            return new AssemblyBuilderTraverser().GetAssembler(ExecBlocks);
        }

        public void SaveToDat(string filename)
        {
            DatBuilder datBuilder = new DatBuilder(this);
            DatFile datFile = datBuilder.GetDatFile();
            Directory.CreateDirectory("./output");
            datFile.Save($"./output/{filename}");
        }
        
        public void Finish()
        {
            int counter = 0;
            int maxCounter = ExecBlocks.Count;
            foreach (BaseExecBlock execBlock in ExecBlocks)
            {
                Console.WriteLine($"{++counter}/{maxCounter} lazy references resolved");
                for (int i = 0; i < execBlock.Body.Count; ++i)
                {
                    AssemblyElement element = execBlock.Body[i];
                    if (element is LazyReferenceAtomInstructions nodeInstructions)
                    {
                        LoadStateFromSnapshot(nodeInstructions.AssemblyBuilderSnapshot);
                        List<AssemblyElement> instructions = GetReferenceAtomInstructions(nodeInstructions.ReferenceAtoms);
                        execBlock.Body.RemoveAt(i);
                        execBlock.Body.InsertRange(i, instructions);
                    }
                    
                    if (element is PushVar pushVar&& pushVar.Symbol.IsStringLiteralSymbol())
                    {
                        pushVar.Symbol.Name = NewStringSymbolName();
                        AddSymbol(pushVar.Symbol);
                    }
                }
            }
        }
    }
}
