using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace DaedalusCompiler.Compilation.SemanticAnalysis
{
    public class ParseTreeVisitor : DaedalusBaseVisitor<ASTNode>
    {
	    private readonly int _sourceFileNumber;
	    private readonly List<InheritanceParentReferenceNode> _inheritanceReferenceNodes;
	    public readonly List<ReferenceNode> ReferenceNodes;
	    private readonly List<ConstDefinitionNode> _constDefinitionNodes;
	    private readonly List<IArrayDeclarationNode> _arrayDeclarationNodes;
	    private readonly CommonTokenStream _tokenStream;

	    public ParseTreeVisitor(CommonTokenStream tokenStream, int sourceFileNumber)
	    {
		    _tokenStream = tokenStream;
		    _sourceFileNumber = sourceFileNumber;
		    _constDefinitionNodes = new List<ConstDefinitionNode>();
		    _arrayDeclarationNodes = new List<IArrayDeclarationNode>();
		    _inheritanceReferenceNodes = new List<InheritanceParentReferenceNode>();
		    ReferenceNodes = new List<ReferenceNode>();
	    }

	    private List<CommentNode> GetCommentNodesToRight(ParserRuleContext context)
	    {
		    List<CommentNode> commentNodes = new List<CommentNode>();
		    IList<IToken> tokens = _tokenStream.GetHiddenTokensToRight(context.Stop.TokenIndex);
		    if (tokens != null)
		    {
			    foreach (IToken token in tokens)
			    {
				    commentNodes.Add(new CommentNode(GetLocation((CommonToken)token), token.Text));
			    }
		    }
		    return commentNodes;
		    //List<CommentNode> commentNodes = GetCommentNodesToRight((ParserRuleContext) context.Parent);
	    }

	    public override ASTNode VisitDaedalusFile([NotNull] DaedalusParser.DaedalusFileContext context)
	    {
		    List<DeclarationNode> definitionNodes = new List<DeclarationNode>();
		    foreach (IParseTree childContext in context.children)
		    {
			    if (childContext is TerminalNodeImpl)
			    {
				    continue;
			    }
			    
			    if (childContext is DaedalusParser.InlineDefContext inlineDefContext)
			    {
				    ASTNode resultNode = Visit(inlineDefContext.GetChild(0));
				    if (resultNode is TemporaryNode temporaryNode)
				    {
					    definitionNodes.AddRange(temporaryNode.Nodes);
				    }
				    else
				    {
					    definitionNodes.Add((FunctionDefinitionNode)resultNode);
				    }
			    }
			    
			    else if (childContext is DaedalusParser.BlockDefContext blockDefContext)
			    {
				    definitionNodes.Add((DeclarationNode) Visit(blockDefContext.GetChild(0)));
			    }
		    }
		    return new FileNode(GetLocation(context), definitionNodes);
	    }
	    

	    public override ASTNode VisitFunctionDef([NotNull] DaedalusParser.FunctionDefContext context)
	    {
		    NameNode typeNameNode = new NameNode(GetLocation(context.dataType()),context.dataType().GetText());
		    NameNode nameNode = new NameNode(GetLocation(context.nameNode()),context.nameNode().GetText());

		    List<ParameterDeclarationNode> varDeclarationNodes = new List<ParameterDeclarationNode>();
			foreach (DaedalusParser.ParameterDeclContext parameterDeclContext in context.parameterList().parameterDecl())
			{
				varDeclarationNodes.Add((ParameterDeclarationNode) VisitParameterDecl(parameterDeclContext));
			}

			List<StatementNode> statementNodes = GetStatementNodes(context.statementBlock());
			
			return new FunctionDefinitionNode(GetLocation(context), typeNameNode, nameNode, varDeclarationNodes, statementNodes, false);
	    }

	    public override ASTNode VisitExternFunctionDecl(DaedalusParser.ExternFunctionDeclContext context)
	    {
		    NameNode typeNameNode = new NameNode(GetLocation(context.dataType()),context.dataType().GetText());
		    NameNode nameNode = new NameNode(GetLocation(context.nameNode()),context.nameNode().GetText());

		    List<ParameterDeclarationNode> varDeclarationNodes = new List<ParameterDeclarationNode>();
		    foreach (DaedalusParser.ParameterDeclContext parameterDeclContext in context.parameterList().parameterDecl())
		    {
			    varDeclarationNodes.Add((ParameterDeclarationNode) VisitParameterDecl(parameterDeclContext));
		    }

		    return new FunctionDefinitionNode(GetLocation(context), typeNameNode, nameNode, varDeclarationNodes, new List<StatementNode>(), true);
	    }

	    public override ASTNode VisitConstDef([NotNull] DaedalusParser.ConstDefContext context)
		{
			return GetConstDefinitionsTemporaryNode(context);
		}
		
		public override ASTNode VisitInstanceDecl([NotNull] DaedalusParser.InstanceDeclContext context)
		{
			return GetInstanceDeclarationsTemporaryNode(context);
		}

		public override ASTNode VisitVarDecl([NotNull] DaedalusParser.VarDeclContext context)
		{
			return GetVarDeclarationsTemporaryNode(context);
		}

		public override ASTNode VisitClassDef([NotNull] DaedalusParser.ClassDefContext context)
		{
			NameNode nameNode = new NameNode(GetLocation(context.nameNode()),context.nameNode().GetText());
			List<DeclarationNode> varDeclarationNodes = new List<DeclarationNode>();
			foreach (DaedalusParser.VarDeclContext varDeclContext in context.varDecl())
			{
				ASTNode node = VisitVarDecl(varDeclContext);
				
				
				if (node is TemporaryNode temporaryNode)
				{
					varDeclarationNodes.AddRange(temporaryNode.Nodes);
				}
				else
				{
					varDeclarationNodes.Add((VarDeclarationNode) node);
				}
			}
			return new ClassDefinitionNode(GetLocation(context), nameNode, varDeclarationNodes);
		}

		public override ASTNode VisitPrototypeDef([NotNull] DaedalusParser.PrototypeDefContext prototypeDefContext)
		{
			NameNode nameNode = new NameNode(GetLocation(prototypeDefContext.nameNode()), prototypeDefContext.nameNode().GetText());
			DaedalusParser.ParentReferenceContext parentReferenceContext = prototypeDefContext.parentReference();
			InheritanceParentReferenceNode inheritanceParentReferenceNode = new InheritanceParentReferenceNode(parentReferenceContext.GetText(), GetLocation(parentReferenceContext));
			_inheritanceReferenceNodes.Add(inheritanceParentReferenceNode);
			List<StatementNode> statementNodes = GetStatementNodes(prototypeDefContext.statementBlock());
			return new PrototypeDefinitionNode(GetLocation(prototypeDefContext), nameNode, inheritanceParentReferenceNode, statementNodes);
		}

		public override ASTNode VisitInstanceDef([NotNull] DaedalusParser.InstanceDefContext instanceDefContext)
		{
			NameNode nameNode = new NameNode(GetLocation(instanceDefContext.nameNode()), instanceDefContext.nameNode().GetText());
			DaedalusParser.ParentReferenceContext parentReferenceContext = instanceDefContext.parentReference();
			InheritanceParentReferenceNode inheritanceParentReferenceNode = new InheritanceParentReferenceNode(parentReferenceContext.GetText(), GetLocation(parentReferenceContext));
			_inheritanceReferenceNodes.Add(inheritanceParentReferenceNode);
			List<StatementNode> statementNodes = GetStatementNodes(instanceDefContext.statementBlock());
			return new InstanceDefinitionNode(GetLocation(instanceDefContext), nameNode, inheritanceParentReferenceNode, statementNodes, definedWithoutBody:false);
		}
		
		public override ASTNode VisitParameterDecl([NotNull] DaedalusParser.ParameterDeclContext context)
		{
			NodeLocation location = GetLocation(context);
			NameNode typeNameNode = new NameNode(GetLocation(context.dataType()),context.dataType().GetText());
			NameNode nameNode = new NameNode(GetLocation(context.nameNode()),context.nameNode().GetText());
			if (context.arraySize() != null)
			{
				var arraySize = (ExpressionNode) VisitArraySize(context.arraySize());
				ParameterArrayDeclarationNode parameterArrayDeclarationNode = new ParameterArrayDeclarationNode(location, typeNameNode, nameNode, arraySize);
				_arrayDeclarationNodes.Add(parameterArrayDeclarationNode);
				return parameterArrayDeclarationNode;
			}
			return new ParameterDeclarationNode(location, typeNameNode, nameNode);
		}

		public override ASTNode VisitStatement([NotNull] DaedalusParser.StatementContext context)
		{
			return Visit(context.GetChild(0));
		}

		public override ASTNode VisitFunctionCall([NotNull] DaedalusParser.FunctionCallContext context)
		{
			DaedalusParser.NameNodeContext nameNodeContext = context.nameNode();
			ReferenceNode referenceNode = new ReferenceNode(nameNodeContext.GetText(), GetLocation(nameNodeContext));
			ReferenceNodes.Add(referenceNode);
			List<ExpressionNode> expressionNodes = new List<ExpressionNode>();
			foreach (DaedalusParser.ExpressionContext expressionContext in context.expression())
			{
				expressionNodes.Add((ExpressionNode) Visit(expressionContext));
			}
			return new FunctionCallNode(GetLocation(context), referenceNode, expressionNodes);

		}

		public override ASTNode VisitAssignment([NotNull] DaedalusParser.AssignmentContext context)
		{
			string oper = context.assignmentOperator().GetText();
			ReferenceNode referenceNode = (ReferenceNode) VisitReference(context.reference());
			ExpressionNode expressionNode = (ExpressionNode) Visit(context.expression());
			
			if (oper == "=")
			{
				return new AssignmentNode(GetLocation(context), GetLocation(context.assignmentOperator()), referenceNode, expressionNode);
			}
			return new CompoundAssignmentNode(GetLocation(context), GetCompoundAssignmentOperator(oper), GetLocation(context.assignmentOperator()), referenceNode, expressionNode);
		}
		
		public override ASTNode VisitElseIfBlock([NotNull] DaedalusParser.ElseIfBlockContext context)
		{
			ExpressionNode conditionNode = (ExpressionNode) Visit(context.expression());
			List<StatementNode> statementNodes = GetStatementNodes(context.statementBlock());
			return new ConditionalNode(GetLocation(context), conditionNode, statementNodes);
		}

		public override ASTNode VisitIfBlock([NotNull] DaedalusParser.IfBlockContext context)
		{
			ExpressionNode conditionNode = (ExpressionNode) Visit(context.expression());
			List<StatementNode> statementNodes = GetStatementNodes(context.statementBlock());
			return new ConditionalNode(GetLocation(context), conditionNode, statementNodes);
		}

		public override ASTNode VisitIfBlockStatement([NotNull] DaedalusParser.IfBlockStatementContext context)
		{
			ConditionalNode conditionalNode = (ConditionalNode) VisitIfBlock(context.ifBlock());
			List<ConditionalNode> conditionalNodes = new List<ConditionalNode>();
			List<StatementNode> elseNodeBodyNodes = null;
			
			foreach (DaedalusParser.ElseIfBlockContext elseIfBlockContext in context.elseIfBlock())
			{
				conditionalNodes.Add((ConditionalNode) VisitElseIfBlock(elseIfBlockContext));
			}
			
			if (context.elseBlock() != null)
			{
				elseNodeBodyNodes = GetStatementNodes(context.elseBlock().statementBlock());
			}
			
			return new IfStatementNode(GetLocation(context), conditionalNode, conditionalNodes, elseNodeBodyNodes);
		}

		public override ASTNode VisitWhileStatement([NotNull] DaedalusParser.WhileStatementContext context)
		{
			ExpressionNode conditionNode = (ExpressionNode) Visit(context.expression());
			List<StatementNode> statementNodes = GetStatementNodes(context.statementBlock());
			return new WhileStatementNode(GetLocation(context), conditionNode, statementNodes);
		}
		
		public override ASTNode VisitReturnStatement([NotNull] DaedalusParser.ReturnStatementContext context)
		{
			ExpressionNode expressionNode = null;
			if (context.expression() != null)
			{
				expressionNode = (ExpressionNode) Visit(context.expression());
			}
			return new ReturnStatementNode(GetLocation(context), expressionNode);
		}

		public override ASTNode VisitBreakStatement([NotNull] DaedalusParser.BreakStatementContext context)
		{
			return new BreakStatementNode(GetLocation(context));
		}

		public override ASTNode VisitContinueStatement([NotNull] DaedalusParser.ContinueStatementContext context)
		{
			return new ContinueStatementNode(GetLocation(context));
		}

		public override ASTNode VisitBracketExpression([NotNull] DaedalusParser.BracketExpressionContext context)
		{
			return Visit(context.expression());
		}

		public override ASTNode VisitUnaryExpression([NotNull] DaedalusParser.UnaryExpressionContext context)
		{
			ExpressionNode expressionNode = (ExpressionNode) Visit(context.expression());
			return new UnaryExpressionNode(GetLocation(context), GetUnaryOperator(context.oper.GetText()), GetLocation(context.oper), expressionNode);
		}

		public override ASTNode VisitBitMoveExpression([NotNull] DaedalusParser.BitMoveExpressionContext context)
		{
			return CreateBinaryExpressionNode(GetLocation(context), context.oper, context.expression());
		}

		public override ASTNode VisitEqExpression([NotNull] DaedalusParser.EqExpressionContext context)
		{
			return CreateBinaryExpressionNode(GetLocation(context), context.oper, context.expression());
		}

		public override ASTNode VisitAddExpression([NotNull] DaedalusParser.AddExpressionContext context)
		{
			return CreateBinaryExpressionNode(GetLocation(context), context.oper, context.expression());
		}

		public override ASTNode VisitCompExpression([NotNull] DaedalusParser.CompExpressionContext context)
		{
			return CreateBinaryExpressionNode(GetLocation(context), context.oper, context.expression());
		}

		public override ASTNode VisitLogOrExpression([NotNull] DaedalusParser.LogOrExpressionContext context)
		{
			return CreateBinaryExpressionNode(GetLocation(context), context.oper, context.expression());
		}

		public override ASTNode VisitBinAndExpression([NotNull] DaedalusParser.BinAndExpressionContext context)
		{
			return CreateBinaryExpressionNode(GetLocation(context), context.oper, context.expression());
		}

		public override ASTNode VisitBinOrExpression([NotNull] DaedalusParser.BinOrExpressionContext context)
		{
			return CreateBinaryExpressionNode(GetLocation(context), context.oper, context.expression());
		}

		public override ASTNode VisitMultExpression([NotNull] DaedalusParser.MultExpressionContext context)
		{
			return CreateBinaryExpressionNode(GetLocation(context), context.oper, context.expression());
		}


		public override ASTNode VisitLogAndExpression([NotNull] DaedalusParser.LogAndExpressionContext context)
		{
			return CreateBinaryExpressionNode(GetLocation(context), context.oper, context.expression());
		}


		public override ASTNode VisitValueExpression([NotNull] DaedalusParser.ValueExpressionContext context)
		{
			return Visit(context.GetChild(0));
		}

		public override ASTNode VisitArrayIndex([NotNull] DaedalusParser.ArrayIndexContext context)
		{
			ExpressionNode expressionNode;
			
			if (context.reference() != null)
			{
				expressionNode = (ExpressionNode) VisitReference(context.reference());
			}
			else
			{
				expressionNode = new IntegerLiteralNode(GetLocation(context), long.Parse(context.GetText()));
			}
			
			return new ArrayIndexNode(expressionNode, GetLocation(context));
			
		}

		public override ASTNode VisitArraySize([NotNull] DaedalusParser.ArraySizeContext context)
		{
			if (context.reference() != null)
			{
				return VisitReference(context.reference());
			}
			return new IntegerLiteralNode(GetLocation(context), long.Parse(context.GetText()));
		}

		public override ASTNode VisitIntegerLiteralValue([NotNull] DaedalusParser.IntegerLiteralValueContext context)
		{
			bool evaluatedCorrectly = long.TryParse(context.GetText(), out long value);
			return new IntegerLiteralNode(GetLocation(context), value, evaluatedCorrectly);
		}

		public override ASTNode VisitFloatLiteralValue([NotNull] DaedalusParser.FloatLiteralValueContext context)
		{
			return new FloatLiteralNode(GetLocation(context), float.Parse(context.GetText()));
		}

		public override ASTNode VisitStringLiteralValue([NotNull] DaedalusParser.StringLiteralValueContext context)
		{
			return new StringLiteralNode(GetLocation(context), context.GetText().Replace("\"", ""));
		}

		public override ASTNode VisitNullLiteralValue([NotNull] DaedalusParser.NullLiteralValueContext context)
		{
			return new NullNode(GetLocation(context));
		}

		public override ASTNode VisitNoFuncLiteralValue([NotNull] DaedalusParser.NoFuncLiteralValueContext context)
		{
			return new NoFuncNode(GetLocation(context));
		}

		public override ASTNode VisitFunctionCallValue([NotNull] DaedalusParser.FunctionCallValueContext context)
		{
			return VisitFunctionCall(context.functionCall());
		}

		public override ASTNode VisitReferenceValue([NotNull] DaedalusParser.ReferenceValueContext context)
		{
			return VisitReference(context.reference());
		}

		public override ASTNode VisitReference([NotNull] DaedalusParser.ReferenceContext context)
		{
			List<ReferencePartNode> referencePartNodes = new List<ReferencePartNode>();
			foreach (var referenceAtomContext in context.referenceAtom())
			{

				AttributeNode attributeNode = new AttributeNode(
					referenceAtomContext.nameNode().GetText(),
					GetLocation(referenceAtomContext)
					);
				referencePartNodes.Add(attributeNode);

				if (referenceAtomContext.arrayIndex() != null)
				{
					ArrayIndexNode arrayIndexNode = (ArrayIndexNode) VisitArrayIndex(referenceAtomContext.arrayIndex());
					attributeNode.ArrayIndexNode = arrayIndexNode;
					referencePartNodes.Add(arrayIndexNode);
				}
			}

			string name = ((AttributeNode) referencePartNodes[0]).Name;
			referencePartNodes.RemoveAt(0);
			ReferenceNode referenceNode = new ReferenceNode(name, referencePartNodes, GetLocation(context));
			ReferenceNodes.Add(referenceNode);
			return referenceNode;
		}
		
		
		private InstanceDeclarationsTemporaryNode GetInstanceDeclarationsTemporaryNode(DaedalusParser.InstanceDeclContext instanceDeclContext)
		{
			DaedalusParser.ParentReferenceContext parentReferenceContext = instanceDeclContext.parentReference();
			InheritanceParentReferenceNode inheritanceParentReferenceNode = new InheritanceParentReferenceNode(parentReferenceContext.GetText(), GetLocation(parentReferenceContext));
			_inheritanceReferenceNodes.Add(inheritanceParentReferenceNode);
			
			List<DeclarationNode> instanceDeclarationNodes = new List<DeclarationNode>();
			
			foreach (DaedalusParser.NameNodeContext nameNodeContext in instanceDeclContext.nameNode())
			{
				NameNode nameNode = new NameNode(GetLocation(nameNodeContext), nameNodeContext.GetText());
				instanceDeclarationNodes.Add(new InstanceDefinitionNode(GetLocation(instanceDeclContext), nameNode, inheritanceParentReferenceNode, new List<StatementNode>(), definedWithoutBody: true));
			}
			
			return new InstanceDeclarationsTemporaryNode(GetLocation(instanceDeclContext), instanceDeclarationNodes);
		}
		
		
		
		private ConstDefinitionsTemporaryNode GetConstDefinitionsTemporaryNode(DaedalusParser.ConstDefContext constDefContext)
		{
			NameNode typeNameNode = new NameNode(GetLocation(constDefContext.dataType()),constDefContext.dataType().GetText());
			
			List<DeclarationNode> constDefinitionNodes = new List<DeclarationNode>();

			foreach (IParseTree childContext in constDefContext.children)
			{
				if (childContext is DaedalusParser.ConstValueDefContext constValueDefContext)
				{
					DaedalusParser.NameNodeContext nameNodeContext = constValueDefContext.nameNode();
					NameNode nameNode = new NameNode(GetLocation(nameNodeContext), nameNodeContext.GetText());
					ExpressionNode rightSideNode = (ExpressionNode) Visit(constValueDefContext.constValueAssignment().expression());
					ConstDefinitionNode constDefinitionNode = new ConstDefinitionNode(GetLocation(constValueDefContext),
						typeNameNode, nameNode, rightSideNode);
					_constDefinitionNodes.Add(constDefinitionNode);
					constDefinitionNodes.Add(constDefinitionNode);
				}
				else if (childContext is DaedalusParser.ConstArrayDefContext constArrayDefContext)
				{	
					DaedalusParser.NameNodeContext nameNodeContext = constArrayDefContext.nameNode();
					NameNode nameNode = new NameNode(GetLocation(nameNodeContext), nameNodeContext.GetText());
					ExpressionNode arraySizeNode = (ExpressionNode) VisitArraySize(constArrayDefContext.arraySize());
					
					List<ExpressionNode> elementNodes = new List<ExpressionNode>();
					foreach (DaedalusParser.ExpressionContext expressionContext in constArrayDefContext.constArrayAssignment().expression())
					{
						elementNodes.Add((ExpressionNode) Visit(expressionContext));
					}

					ConstArrayDefinitionNode constArrayDefinitionNode =
						new ConstArrayDefinitionNode(GetLocation(nameNodeContext), typeNameNode, nameNode, arraySizeNode,
							elementNodes);
					_arrayDeclarationNodes.Add(constArrayDefinitionNode);
					constDefinitionNodes.Add(constArrayDefinitionNode);
				}
			}
			return new ConstDefinitionsTemporaryNode(GetLocation(constDefContext), constDefinitionNodes);
		}

		
		private VarDeclarationsTemporaryNode GetVarDeclarationsTemporaryNode(DaedalusParser.VarDeclContext varDeclContext)
		{
			NameNode typeNameNode = new NameNode(GetLocation(varDeclContext.dataType()),varDeclContext.dataType().GetText());
			
			List<DeclarationNode> varDeclarationNodes = new List<DeclarationNode>();
			foreach (IParseTree childContext in varDeclContext.children)
			{
				if (childContext is DaedalusParser.VarValueDeclContext varValueDeclContext)
				{
					DaedalusParser.NameNodeContext nameNodeContext = varValueDeclContext.nameNode();
					NameNode nameNode = new NameNode(GetLocation(nameNodeContext), nameNodeContext.GetText());
					
					ExpressionNode rightSideNode = null;
					if (varValueDeclContext.varValueAssignment() != null)
					{
						rightSideNode = (ExpressionNode) Visit(varValueDeclContext.varValueAssignment().expression()); // TODO check if null
					}
					
					varDeclarationNodes.Add(new VarDeclarationNode(GetLocation(varValueDeclContext), typeNameNode, nameNode, rightSideNode));
				}
				else if (childContext is DaedalusParser.VarArrayDeclContext varArrayDeclContext)
				{	
					DaedalusParser.NameNodeContext nameNodeContext = varArrayDeclContext.nameNode();
					NameNode nameNode = new NameNode(GetLocation(nameNodeContext), nameNodeContext.GetText());
					ExpressionNode arraySizeNode = (ExpressionNode) VisitArraySize(varArrayDeclContext.arraySize());


					List<ExpressionNode> elementNodes = null;
					if (varArrayDeclContext.varArrayAssignment() != null)
					{
						elementNodes = new List<ExpressionNode>();
						foreach (DaedalusParser.ExpressionContext expressionContext in varArrayDeclContext.varArrayAssignment().expression())
						{
							elementNodes.Add((ExpressionNode) Visit(expressionContext));
						}
					}
					
					VarArrayDeclarationNode varArrayDeclarationNode =
						new VarArrayDeclarationNode(GetLocation(nameNodeContext), typeNameNode, nameNode, arraySizeNode, elementNodes); // TODO check if null
					_arrayDeclarationNodes.Add(varArrayDeclarationNode);
					varDeclarationNodes.Add(varArrayDeclarationNode);
				}
			}
			return new VarDeclarationsTemporaryNode(GetLocation(varDeclContext), varDeclarationNodes);
		}
		private List<StatementNode> GetStatementNodes(DaedalusParser.StatementBlockContext statementBlockContext)
		{
			List<StatementNode> statementNodes = new List<StatementNode>();
			foreach (IParseTree childContext in statementBlockContext.children)
			{
				if (childContext is TerminalNodeImpl)
				{
					continue;
				}

				ASTNode node = Visit(childContext);
				if (node is TemporaryNode temporaryNode)
				{
					statementNodes.AddRange(temporaryNode.Nodes);
				}
				else
				{
					statementNodes.Add((StatementNode) node);
				}
			}
			return statementNodes;
		}

		private UnaryOperator GetUnaryOperator(string oper)
		{
			switch (oper)
			{
				case "-":
					return UnaryOperator.Minus;
				case "!":
					return UnaryOperator.Not;
				case "~":
					return UnaryOperator.Negate;
				case "+":
					return UnaryOperator.Plus;
				default:
					throw new Exception();
			}
		}
		
		private CompoundAssignmentOperator GetCompoundAssignmentOperator(string oper)
		{
			switch (oper)
			{
				case "+=":
					return CompoundAssignmentOperator.Add;
				case "-=":
					return CompoundAssignmentOperator.Sub;
				case "*=":
					return CompoundAssignmentOperator.Mult;
				case "/=":
					return CompoundAssignmentOperator.Div;
				default:
					throw new Exception();
			}
		}
		
		private BinaryOperator GetBinaryOperator(string oper)
		{
			switch (oper)
			{
				case "*":
					return BinaryOperator.Mult;
				case "/":
					return BinaryOperator.Div;
				case "%":
					return BinaryOperator.Modulo;
				
				case "+":
					return BinaryOperator.Add;
				case "-":
					return BinaryOperator.Sub;
				
				case "<<":
					return BinaryOperator.ShiftLeft;
				case ">>":
					return BinaryOperator.ShiftRight;
				
				case "<":
					return BinaryOperator.Less;
				case ">":
					return BinaryOperator.Greater;
				case "<=":
					return BinaryOperator.LessOrEqual;
				case ">=":
					return BinaryOperator.GreaterOrEqual;
				
				case "==":
					return BinaryOperator.Equal;
				case "!=":
					return BinaryOperator.NotEqual;
				
				case "&":
					return BinaryOperator.BinAnd;
				case "|":
					return BinaryOperator.BinOr;
				
				case "&&":
					return BinaryOperator.LogAnd;
				case "||":
					return BinaryOperator.LogOr;
				
				default:
					throw new Exception();
			}
		}
		
		private BinaryExpressionNode CreateBinaryExpressionNode(NodeLocation location, ParserRuleContext operatorContext, DaedalusParser.ExpressionContext[] expressionContexts)
		{
			string oper = operatorContext.GetText();
			NodeLocation operatorLocation = GetLocation(operatorContext);
			ExpressionNode leftSide = (ExpressionNode) Visit(expressionContexts[0]);
			ExpressionNode rightSide = (ExpressionNode) Visit(expressionContexts[1]);
			return new BinaryExpressionNode(location, GetBinaryOperator(oper), operatorLocation, leftSide, rightSide );
		}
		
		private NodeLocation GetLocation(ParserRuleContext context)
		{
			if (context.Stop == null)
			{
				// In case of empty file, context has Start (EOF token) but has no end
				context.Stop = context.Start;
			}
			

			return new NodeLocation
			{
				FileIndex = _sourceFileNumber,
				Line = context.Start.Line,
				Column = context.Start.Column,
				Index = context.Start.StartIndex,
				LinesCount = context.Stop.Line - context.Start.Line + 1,
				CharsCount = context.Stop.StopIndex - context.Start.StartIndex + 1,
			
				EndColumn = context.Stop.StopIndex, // TODO it will not work correctly if Stop token is multiline token
			};
		}
		
		private NodeLocation GetLocation(CommonToken token)
		{
			return new NodeLocation // TODO check if we need anything other than Line here
			{
				// FileIndex = _sourceFileNumber,
				Line = token.Line,
				// Column = token.Column,
				// Index = token.StartIndex,
				// LinesCount = Regex.Matches(token.Text, Environment.NewLine).Count + 1,
				// CharsCount = token.StopIndex - token.StartIndex + 1,
				// EndColumn = token.StopIndex,
			};
		}
    }
}