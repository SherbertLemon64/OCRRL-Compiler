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
			object seek = TokenReader.Seek();

			if (seek is ExpressionComparason)
			{
				_returnExpr = ParseBinaryExpression();
			} else if (seek is ParenthesisToken seekToken)
			{
				if (seekToken.Open)
					_returnExpr = ParseFunctionCallExpression();
				else
				{
					_returnExpr = ParseSingleExpression();
				}
			}
			else
			{
				_returnExpr = ParseSingleExpression();
			}

			return _returnExpr;
		}

		private Expression ParseFunctionCallExpression()
		{
			FunctionCallExpression call = new FunctionCallExpression();
			List<Expression> parameters = new List<Expression>();
			ExpressionVariable function = (ExpressionVariable) TokenReader.ReadValueAsType(typeof(ExpressionVariable));
			
			object token = TokenReader.Seek();
			while (token is Expression ExpToken)
			{
				TokenReader.Read();
				parameters.Add(ParseExpression());
				
				if (TokenReader.SeekCurrent() is not Comma)
					break;
				
				token = TokenReader.Seek();
			}
			
			TokenReader.Read(); // swallow the parenthesis 
			
			call.FunctionName = function.ValueName;
			call.Params = parameters.ToArray();

			return call;
		}
		
		private Expression ParseSingleExpression()
		{
			Expression returnValue;
			
			if (TokenReader.Seek() is ParenthesisToken { Open: true })
			{
				returnValue = ParseFunctionCallExpression();
			} else if (TokenReader.SeekCurrent() is ExpressionVariable variable)
			{
				Tree.AddVariable(ref variable);
				TokenReader.Read();
				return variable;
			}
			else
			{
				returnValue = (Expression) TokenReader.ReadValueAsType(typeof(Expression));
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
				int currentPoint = TokenReader.index;
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
					TokenReader.index = currentPoint;
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
		public FunctionDefinitionStatement ParseFunctionDefinitionStatement()
		{
			FunctionDefinitionStatement function = new FunctionDefinitionStatement();
			return function;
		}
    
		public ConditionalStatement ParseConditionalStatement()
		{
			ConditionalStatement statement = new ConditionalStatement {Check = ParseExpression()};
			return statement;
		}

		private AssignmentStatement ParseAssignmentStatement()
		{
			ExpressionVariable _var = (ExpressionVariable)TokenReader.ReadValueAsType(typeof(ExpressionVariable));
			
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
			if (_var.ExpressionType is null)
				_var.ExpressionType = returnValue.Assignment.ExpressionType;
			
			Tree.AddVariable(ref returnValue.Variable);
			
			return returnValue;
		}

		public ForLoopStatement ParseForLoop()
		{
			ForLoopStatement returnValue = new ForLoopStatement();
			AssignmentStatement assignmentStatement = ParseAssignmentStatement();

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

		public Statement ParseFunctionCallStatement()
		{
			FunctionCallStatement statement = new FunctionCallStatement();
			statement.function = (FunctionCallExpression)ParseFunctionCallExpression();
			return statement;
		}
		
		private Statement ParseNextStatement()
		{
			object currentToken = TokenReader.Seek();
			
			Statement _returnStatement;

			if (currentToken is Identifier identifier)
			{
				_returnStatement = identifier.ParseStatement(this);
			}
			else if (currentToken is ExpressionVariable var)
			{
				if (TokenReader.Seek(2) is ParenthesisToken)
				{
					_returnStatement = ParseFunctionCallStatement();
				}
				else
				{
					_returnStatement = ParseAssignmentStatement();
				}
			}
			else
			{
				return null;
			}
			
			return _returnStatement;
		}
	}
}