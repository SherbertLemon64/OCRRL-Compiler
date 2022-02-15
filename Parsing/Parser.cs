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
		
		private Expression ParseExpression()
		{
			Expression _returnExpr;
			
			if (TokenReader.Seek() is OperatorToken)
			{
				_returnExpr = ParseBinaryExpression();
			}
			else
			{
				_returnExpr = ParseExpression(true);
			}

			return _returnExpr;
		}

		private Expression ParseExpression(bool _notComparason)
		{
			Expression _returnVal;
			
			object _currVal = TokenReader.Read();

			if (_currVal is IntegerLiteralToken)
			{
				ExpressionLiteral<int> _derived = new ExpressionLiteral<int>();
				IntegerLiteralToken _token = (IntegerLiteralToken) _currVal;

				_derived.Value = _token.Value;

				_returnVal = _derived;
			} else if (_currVal is StringLiteralToken)
			{
				ExpressionLiteral<string> _derived = new ExpressionLiteral<string>();
				StringLiteralToken _token = (StringLiteralToken) _currVal;

				_derived.Value = _token.Value;
				_returnVal = _derived;
			} else if (_currVal is VarToken)
			{
				ExpressionVariable _derived = new ExpressionVariable();
				VarToken _token = (VarToken) _currVal;

				_derived.ValueName = _token.Value;
				_returnVal = _derived;
			} else if (_currVal is OperatorToken)
			{
				OperatorToken _token = (OperatorToken) _currVal;
				ExpressionComparason _derived = new ExpressionComparason(_token.OperatorType);

				_returnVal = _derived;
			}
			else
			{
				_returnVal = null;
			}

			return _returnVal;
		}

		private ExpressionVariable ParseVarExpression(VarToken _token)
		{
			ExpressionVariable _returnExpr;

			_returnExpr = new ExpressionVariable();
			_returnExpr.ValueName = _token.Value;


			return _returnExpr;
		}
		private BinaryExpression ParseBinaryExpression()
		{
			BinaryExpression _topLevelExpression = new BinaryExpression();
			bool _exprEnd = false;
			
			_topLevelExpression = (BinaryExpression)ParseSingleBinaryExpression();
			while (true)
			{
				object _peek = ParseExpression(true);
				if (_peek is not ExpressionComparason)
				{
					break;
				}
				
				ExpressionComparason currentComparason = (ExpressionComparason)_peek;
				BinaryExpression currExp = HighestBinaryExpressionWithPrecedenceLessThanGivenValueLeftWards(_topLevelExpression, currentComparason.Precedence);
				BinaryExpression lhs = (BinaryExpression)currExp.Copy();
				currExp.Comparason = currentComparason;
				currExp.LeftValue = lhs;
				currExp.RightValue = ParseSingleBinaryExpression();

				if (currExp.RightValue is not BinaryExpression)
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
			if (TokenReader.Seek() is not OperatorToken) { return ParseExpression(true);}
			
			BinaryExpression currExpr = new BinaryExpression();

			currExpr.LeftValue = ParseExpression(true);
			OperatorToken token = (OperatorToken) TokenReader.Read();
			currExpr.Comparason = new ExpressionComparason(token.OperatorType);
			currExpr.RightValue = ParseExpression(true);
			return currExpr;
		}
		
		
		// statements
		public ConditionalStatement ParseConditionalStatement()
		{
			ConditionalStatement statement = new ConditionalStatement {Check = ParseExpression()};
			return statement;
		}

		private AssignmentStatement CreateAssignmentStatement(VarToken _var)
		{
			AssignmentStatement returnValue = new AssignmentStatement();

			ExpressionVariable variable = new ExpressionVariable();
			variable.ValueName = _var.Value;
			returnValue.Variable = variable;
			
			object assignment = TokenReader.Read();
			if (assignment is AssignmentToken assignmentToken)
			{
				returnValue.Assignment = ParseExpression();
			}
			else
			{
				throw new UnexpectedTokenException(0, typeof(Expression), assignment.GetType());
			}

			return returnValue;
		}

		public ForLoopStatement ParseForLoop()
		{
			ForLoopStatement returnValue = new ForLoopStatement();
			AssignmentStatement assignmentStatement = CreateAssignmentStatement((VarToken) TokenReader.ReadValueAsType(typeof(VarToken)));

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

		public ForLoopEndStatement ParseForLoopEndStatement()
		{
			ForLoopEndStatement statement = new ForLoopEndStatement();
			Tree.EndScope();
			statement.Variable = (ExpressionVariable) ParseExpression();
			return statement;
		}
		
		private Statement ParseNextStatement()
		{
			object currentToken = TokenReader.Read();
			
			Statement _returnStatement = new Statement();

			if (currentToken is Identifier identifier)
			{
				_returnStatement = identifier.ParseStatement(this);
			}
			else if (currentToken is VarToken var)
			{
				_returnStatement = CreateAssignmentStatement(var);
			}
			else
			{
				return null;
			}
			
			return _returnStatement;
		}
	}
}