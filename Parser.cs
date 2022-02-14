using System;
using System.Collections.Generic;
using System.IO;
using OCRRFcompiler.Errors;
using OCRRFcompiler.Scanning;

namespace OCRRFcompiler
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
			
			if (TokenReader.Seek() is ComparasonToken)
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
			} else if (_currVal is ComparasonToken)
			{
				ComparasonToken _token = (ComparasonToken) _currVal;
				ExpressionComparason _derived = new ExpressionComparason(_token.ComparasonType);

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
			if (TokenReader.Seek() is not ComparasonToken) { return ParseExpression(true);}
			
			BinaryExpression currExpr = new BinaryExpression();

			currExpr.LeftValue = ParseExpression(true);
			ComparasonToken token = (ComparasonToken) TokenReader.Read();
			currExpr.Comparason = new ExpressionComparason(token.ComparasonType);
			currExpr.RightValue = ParseExpression(true);
			return currExpr;
		}
		
		
		// statements
		private ConditionalStatement CreateConditionalStatement()
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

		private ForLoopStatement ParseForLoop()
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
				Comparason = new ExpressionComparason(Comparasons.LessThan)
			};

			TokenReader.Read(); // chuck the step

			ExpressionLiteral<int> step = (ExpressionLiteral<int>) ParseExpression();

			returnValue.Step = step.Value;

			return returnValue;
		}
		
		private Statement ParseNextStatement()
		{
			object currentToken = TokenReader.Read();
			
			Statement _returnStatement = new Statement();

			if (currentToken is IdentifierToken)
			{
				IdentifierToken identifierToken = (IdentifierToken) currentToken;
				switch (identifierToken.Value)
				{
					case (int) Identifiers.IF:
					{
						_returnStatement = CreateConditionalStatement();
						break;
					}
					case (int) Identifiers.WHILE:
					{
						_returnStatement = CreateConditionalStatement();
						break;
					}
					case (int) Identifiers.ENDSCOPE:
					{
						ConditionalEndStatement statement = new ConditionalEndStatement();
						Tree.EndScope();
						_returnStatement = statement;
						break;
					}
					case (int) Identifiers.ENDWHILE:
					{
						LoopEndStatement statement = new LoopEndStatement();
						Tree.EndScope();
						_returnStatement = statement;
						break;
					}
					case (int)Identifiers.NEXT:
					{
						ForLoopEndStatement statement = new ForLoopEndStatement();
						Tree.EndScope();
						_returnStatement = statement;
						statement.Variable = (ExpressionVariable) ParseExpression();
						break;
					}
					case (int) Identifiers.FOR:
					{
						_returnStatement = ParseForLoop();
						break;
					}
					default:
						// not implemented identifier or an EOL token
						return null;
						break;
				}
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

	public class Reader<T>
	{
		private T[] values;
		public int index;

		public Reader(T[] _values)
		{
			values = _values;
		}

		public Reader(T[] _values, int _index)
		{
			values = _values;
			index = _index;
		}

		public bool IsEnd()
		{
			return values.Length <= index + 1;
		}
		
		public T Read()
		{
			T val = values[index];
			index++;
			return val;
		}

		public T Seek()
		{
			return Seek(1);
		}

		public T Seek(int _distance)
		{
			try
			{
				return values[index + _distance];
			}
			catch
			{
				return default(T);
			}
		}

		public object ReadValueAsType(Type t)
		{
			object val = Read();
			Type valType = val.GetType();
			if (valType != t)
			{
				throw new UnexpectedTokenException(0,valType,t);
			}

			return val;
		}
		
		public T SeekCurrent()
		{
			return values[index];
		}
	}

	public class SyntaxTree
	{
		public Scope GlobalScope = new Scope(null);

		private Scope CurrentScope;

		public SyntaxTree()
		{
			CurrentScope = GlobalScope;
		}
	
		public void AddStatement(Statement _toAdd)
		{
			CurrentScope.Statements.Add(_toAdd);
			if (_toAdd is ConditionalStatement conditionalStatement)
			{
				conditionalStatement.ConditionalScope = new Scope(CurrentScope);
				CurrentScope = conditionalStatement.ConditionalScope;
			}
		}

		public void EndScope()
		{
			CurrentScope = CurrentScope.Parent;
		}
	}

	public class Scope
	{
		public Scope Parent;
		public List<Statement> Statements = new List<Statement>();

		public Scope(Scope _parent)
		{
			Parent = _parent;
		}
	}
	
	public class Statement
	{
	}

	public class Expression
	{
		public Type ExpressionType;

		public Expression Copy()
		{
			return (Expression)this.MemberwiseClone();
		}
	}

	public class ExpressionComparason : Expression
	{
		public Comparasons Comparason;
		public int Precedence;
		
		private static readonly int[] ComparasonsPrecedence = new[]
		{
			1,
			1,
			1,
			1,
			1,
			1,
			3,
			2,
			1
		};
		public ExpressionComparason(Comparasons _comparason)
		{
			Comparason = _comparason;
			Precedence = ComparasonsPrecedence[(int) _comparason];
		}
	}
	public class ExpressionLiteral<T> : Expression
	{
		public T Value;
	}

	public class ExpressionVariable : Expression
	{
		public string ValueName;
	}

	public class BinaryExpression : Expression
	{
		public Expression LeftValue;
		public Expression RightValue;

		public ExpressionComparason Comparason;
	}

	public class ConditionalStatement : Statement
	{
		public Expression Check;
		public Scope ConditionalScope;
	}

	public class ConditionalEndStatement : Statement { }
	public class LoopEndStatement : ConditionalEndStatement
	{
		public ConditionalStatement LoopStart;
	}

	public class ForLoopEndStatement : LoopEndStatement
	{
		public ExpressionVariable Variable;
	}

	public class ForLoopStatement : ConditionalStatement
	{
		public AssignmentStatement Assignment;
		public int Step;
	}
	
	public class AssignmentStatement : Statement
	{
		public ExpressionVariable Variable;
		public Expression Assignment;
	}
}