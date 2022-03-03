using System;
using System.Collections.Generic;
using System.IO;
using OCRRFcompiler.Errors;
using OCRRFcompiler.Scanning;
using OCRRFcompiler.Expressions;
using OCRRFcompiler.Statements;
using OCRRFcompiler.Tokens;

namespace OCRRFcompiler.Parsing
{
	public class Parser
	{
		public Scanner Scanning;
		public Statement TopStatement;
		public string CompileLocation;

		private Reader<object> TokenReader;

		public SyntaxTree Tree;

		public void ParseLocation(string _path)
		{
			string text = File.ReadAllText(_path);
			Parse(text);
		}

		public void Parse(string _data)
		{
			Tree = new SyntaxTree();

			TextReader read = new StringReader(_data);
			Scanning = new Scanner();
			Scanning.Scan(read);
			
			TokenReader = new Reader<object>(Scanning.Tokens.ToArray());

			while (!TokenReader.IsEnd())
			{
				Statement newStatement = ParseNextStatement();
				if (newStatement is not null)
					Tree.AddStatement(newStatement);
			}
		}
		
		public Expression ParseExpression()
		{
			Expression _returnExpr;
			
			if (TokenReader.Seek() is ExpressionComparason)
			{
				_returnExpr = ParseBinaryExpression();
			}
			else
			{
				_returnExpr = ParseSingleExpression();
			}

			return _returnExpr;
		}

		private Expression ParseSingleExpression()
		{
			Expression returnValue = (Expression) TokenReader.ReadValueAsType(typeof(Expression));

			if (returnValue is ExpressionVariable variable)
			{
				Tree.AddVariable(ref variable);
				return variable;
			}

			return returnValue;
		}
		
		private BinaryExpression ParseBinaryExpression()
		{
			BinaryExpression _topLevelExpression = new BinaryExpression();
			bool _exprEnd = false;
			
			_topLevelExpression = (BinaryExpression)ParseSingleBinaryExpression();
			while (true)
			{
				object _peek;
				try
				{
					_peek = ParseExpression();
					if (_peek is not ExpressionComparason)
					{
						break;
					}
				}
				catch (UnexpectedTokenException)
				{
					break;
				}
				
				
				ExpressionComparason currentComparason = (ExpressionComparason)_peek;
				BinaryExpression currExp = HighestBinaryExpressionWithPrecedenceLessThanGivenValueLeftWards(_topLevelExpression, currentComparason.Precedence);
				BinaryExpression lhs = (BinaryExpression)currExp.Copy();
				currExp.Comparason = currentComparason;
				currExp.LeftValue = lhs;
				currExp.RightValue = ParseSingleBinaryExpression();

				if (currExp.RightValue is not BinaryExpression || currExp.RightValue is null)
				{
					break;
				}
			}
			
			return _topLevelExpression;
		}
		
		public BinaryExpression HighestBinaryExpressionWithPrecedenceLessThanGivenValueLeftWards(BinaryExpression _expression, int _precedence)
		{
			while (true)
			{
				if (_expression.LeftValue is not BinaryExpression || _expression.Comparason is null || _expression.Comparason.Precedence < _precedence)
				{
					break;
				}

				_expression = (BinaryExpression)_expression.LeftValue;
			}

			return _expression;
		}

		private Expression ParseSingleBinaryExpression()
		{
			if (TokenReader.Seek() is not ExpressionComparason) { return ParseExpression();}
			
			BinaryExpression currExpr = new BinaryExpression();
			try
			{
				currExpr.LeftValue = ParseSingleExpression();
				ExpressionComparason token = (ExpressionComparason) TokenReader.Read();
				currExpr.Comparason = new ExpressionComparason(token.Operator);
				currExpr.RightValue = ParseSingleExpression();
			}
			catch (UnexpectedTokenException)
			{
				currExpr = null;
			}
			
			return currExpr;
		}
		
		
		// statements
		public ConditionalStatement ParseConditionalStatement()
		{
			ConditionalStatement statement = new ConditionalStatement {Check = ParseExpression()};
			return statement;
		}

		private AssignmentStatement ParseAssignmentStatement(ExpressionVariable _var)
		{
			Tree.AddVariable(ref _var);
			AssignmentStatement returnValue = new AssignmentStatement();
			
			returnValue.Variable = _var;
			
			object assignment = TokenReader.Read();
			if (assignment is AssignmentToken assignmentToken)
			{
				returnValue.Assignment = ParseExpression();
			}
			else
			{
				throw new UnexpectedTokenException(0, typeof(Expression), assignment.GetType());
			}

			_var.ExpressionType = returnValue.Assignment.ExpressionType;
			
			Tree.CurrentScope.Variables.Add(returnValue.Variable);
			
			return returnValue;
		}

		public ForLoopStatement ParseForLoop()
		{
			ForLoopStatement returnValue = new ForLoopStatement();
			AssignmentStatement assignmentStatement = ParseAssignmentStatement((ExpressionVariable) TokenReader.ReadValueAsType(typeof(ExpressionVariable)));

			returnValue.Assignment = assignmentStatement;
			
			TokenReader.Read(); // chuck the to
			
			ExpressionLiteral<int> max = (ExpressionLiteral<int>) ParseExpression();

			returnValue.Check = new BinaryExpression()
			{
				LeftValue = returnValue.Assignment.Variable, 
				RightValue = max,
				Comparason = new ExpressionComparason(Operators.LessThan)
			};

			TokenReader.Read(); // chuck the step

			ExpressionLiteral<int> step = (ExpressionLiteral<int>) ParseExpression();

			returnValue.Step = step.Value;

			return returnValue;
		}

		public ConditionalEndStatement ParseConditionalEndStatement()
		{
			ConditionalEndStatement statement = new ConditionalEndStatement();
			Tree.EndScope();
			return statement;
		}

		public LoopEndStatement ParseLoopEndStatement()
		{
			LoopEndStatement statement = new LoopEndStatement();
			Tree.EndScope();
			return statement;
		}

		public void ParseForLoopEndStatement()
		{
			Tree.EndScope();
		}
		
		private Statement ParseNextStatement()
		{
			object currentToken = TokenReader.Read();
			
			Statement _returnStatement;

			if (currentToken is Identifier identifier)
			{
				_returnStatement = identifier.ParseStatement(this);
			}
			else if (currentToken is ExpressionVariable var)
			{
				_returnStatement = ParseAssignmentStatement(var);
			}
			else
			{
				return null;
			}
			
			return _returnStatement;
		}
	}
}