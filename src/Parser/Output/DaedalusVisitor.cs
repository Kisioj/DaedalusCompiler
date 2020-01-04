//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.7.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Daedalus.g4 by ANTLR 4.7.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="DaedalusParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.7.2")]
[System.CLSCompliant(false)]
public interface IDaedalusVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.daedalusFile"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDaedalusFile([NotNull] DaedalusParser.DaedalusFileContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.blockDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlockDef([NotNull] DaedalusParser.BlockDefContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.inlineDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInlineDef([NotNull] DaedalusParser.InlineDefContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.externFunctionDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExternFunctionDecl([NotNull] DaedalusParser.ExternFunctionDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.functionDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionDef([NotNull] DaedalusParser.FunctionDefContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.constDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConstDef([NotNull] DaedalusParser.ConstDefContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.classDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassDef([NotNull] DaedalusParser.ClassDefContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.prototypeDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrototypeDef([NotNull] DaedalusParser.PrototypeDefContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.instanceDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInstanceDef([NotNull] DaedalusParser.InstanceDefContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.instanceDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInstanceDecl([NotNull] DaedalusParser.InstanceDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.varDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVarDecl([NotNull] DaedalusParser.VarDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.constArrayDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConstArrayDef([NotNull] DaedalusParser.ConstArrayDefContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.constArrayAssignment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConstArrayAssignment([NotNull] DaedalusParser.ConstArrayAssignmentContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.constValueDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConstValueDef([NotNull] DaedalusParser.ConstValueDefContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.constValueAssignment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConstValueAssignment([NotNull] DaedalusParser.ConstValueAssignmentContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.varArrayDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVarArrayDecl([NotNull] DaedalusParser.VarArrayDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.varArrayAssignment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVarArrayAssignment([NotNull] DaedalusParser.VarArrayAssignmentContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.varValueDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVarValueDecl([NotNull] DaedalusParser.VarValueDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.varValueAssignment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVarValueAssignment([NotNull] DaedalusParser.VarValueAssignmentContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.parameterList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameterList([NotNull] DaedalusParser.ParameterListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.parameterDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameterDecl([NotNull] DaedalusParser.ParameterDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.statementBlock"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStatementBlock([NotNull] DaedalusParser.StatementBlockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStatement([NotNull] DaedalusParser.StatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.functionCall"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionCall([NotNull] DaedalusParser.FunctionCallContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.assignment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignment([NotNull] DaedalusParser.AssignmentContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.elseBlock"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitElseBlock([NotNull] DaedalusParser.ElseBlockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.elseIfBlock"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitElseIfBlock([NotNull] DaedalusParser.ElseIfBlockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.ifBlock"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfBlock([NotNull] DaedalusParser.IfBlockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.ifBlockStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfBlockStatement([NotNull] DaedalusParser.IfBlockStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.returnStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReturnStatement([NotNull] DaedalusParser.ReturnStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.whileStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWhileStatement([NotNull] DaedalusParser.WhileStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.breakStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBreakStatement([NotNull] DaedalusParser.BreakStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.continueStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitContinueStatement([NotNull] DaedalusParser.ContinueStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>bitMoveExpression</c>
	/// labeled alternative in <see cref="DaedalusParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBitMoveExpression([NotNull] DaedalusParser.BitMoveExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>valueExpression</c>
	/// labeled alternative in <see cref="DaedalusParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitValueExpression([NotNull] DaedalusParser.ValueExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>eqExpression</c>
	/// labeled alternative in <see cref="DaedalusParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEqExpression([NotNull] DaedalusParser.EqExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>addExpression</c>
	/// labeled alternative in <see cref="DaedalusParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAddExpression([NotNull] DaedalusParser.AddExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>compExpression</c>
	/// labeled alternative in <see cref="DaedalusParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCompExpression([NotNull] DaedalusParser.CompExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>logOrExpression</c>
	/// labeled alternative in <see cref="DaedalusParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogOrExpression([NotNull] DaedalusParser.LogOrExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>binAndExpression</c>
	/// labeled alternative in <see cref="DaedalusParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBinAndExpression([NotNull] DaedalusParser.BinAndExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>binOrExpression</c>
	/// labeled alternative in <see cref="DaedalusParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBinOrExpression([NotNull] DaedalusParser.BinOrExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>multExpression</c>
	/// labeled alternative in <see cref="DaedalusParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMultExpression([NotNull] DaedalusParser.MultExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>bracketExpression</c>
	/// labeled alternative in <see cref="DaedalusParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBracketExpression([NotNull] DaedalusParser.BracketExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>unaryExpression</c>
	/// labeled alternative in <see cref="DaedalusParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnaryExpression([NotNull] DaedalusParser.UnaryExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>logAndExpression</c>
	/// labeled alternative in <see cref="DaedalusParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogAndExpression([NotNull] DaedalusParser.LogAndExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.arrayIndex"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArrayIndex([NotNull] DaedalusParser.ArrayIndexContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.arraySize"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArraySize([NotNull] DaedalusParser.ArraySizeContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>integerLiteralValue</c>
	/// labeled alternative in <see cref="DaedalusParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIntegerLiteralValue([NotNull] DaedalusParser.IntegerLiteralValueContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>floatLiteralValue</c>
	/// labeled alternative in <see cref="DaedalusParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFloatLiteralValue([NotNull] DaedalusParser.FloatLiteralValueContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>stringLiteralValue</c>
	/// labeled alternative in <see cref="DaedalusParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStringLiteralValue([NotNull] DaedalusParser.StringLiteralValueContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>nullLiteralValue</c>
	/// labeled alternative in <see cref="DaedalusParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNullLiteralValue([NotNull] DaedalusParser.NullLiteralValueContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>noFuncLiteralValue</c>
	/// labeled alternative in <see cref="DaedalusParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNoFuncLiteralValue([NotNull] DaedalusParser.NoFuncLiteralValueContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>functionCallValue</c>
	/// labeled alternative in <see cref="DaedalusParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionCallValue([NotNull] DaedalusParser.FunctionCallValueContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>referenceValue</c>
	/// labeled alternative in <see cref="DaedalusParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReferenceValue([NotNull] DaedalusParser.ReferenceValueContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.referenceAtom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReferenceAtom([NotNull] DaedalusParser.ReferenceAtomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.reference"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReference([NotNull] DaedalusParser.ReferenceContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.dataType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDataType([NotNull] DaedalusParser.DataTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.nameNode"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNameNode([NotNull] DaedalusParser.NameNodeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.parentReference"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParentReference([NotNull] DaedalusParser.ParentReferenceContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.assignmentOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignmentOperator([NotNull] DaedalusParser.AssignmentOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.addOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAddOperator([NotNull] DaedalusParser.AddOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.bitMoveOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBitMoveOperator([NotNull] DaedalusParser.BitMoveOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.compOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCompOperator([NotNull] DaedalusParser.CompOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.eqOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEqOperator([NotNull] DaedalusParser.EqOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.unaryOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnaryOperator([NotNull] DaedalusParser.UnaryOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.multOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMultOperator([NotNull] DaedalusParser.MultOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.binAndOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBinAndOperator([NotNull] DaedalusParser.BinAndOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.binOrOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBinOrOperator([NotNull] DaedalusParser.BinOrOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.logAndOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogAndOperator([NotNull] DaedalusParser.LogAndOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DaedalusParser.logOrOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogOrOperator([NotNull] DaedalusParser.LogOrOperatorContext context);
}
