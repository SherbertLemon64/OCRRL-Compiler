using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Transactions;
using OCRRFcompiler.Expressions;
using OCRRFcompiler.Tokens;

namespace OCRRFcompiler.Scanning
{
	public class Scanner
	{
		public List<object> Tokens = new List<object>();

		public void Scan(TextReader _reader)
		{
			int indented = -1;
			while (_reader.Peek() != -1)
			{
				char c = (char) _reader.Read();
				if (c == ' ')
				{
				}
				else if (char.IsDigit(c))
				{
					ReadIntegerLiteral(_reader, c);
				}
				else if (char.IsLetter(c))
				{
					ReadTextBlock(_reader, c);
				}
				else
				{
					switch (c)
					{
						case '"':
							ReadStringLiteral(_reader);
							break;
						case '=':
							ReadAssignment(_reader);
							break;
						case '!':
							ReadComparason(Operators.NotEqual);
							break;
						case '>':
							if (_reader.Peek() == '=')
							{
								ReadComparason(Operators.GreaterThanOrEqual);
								_reader.Read();
							}
							else
							{
								ReadComparason(Operators.GreaterThan);
							}

							break;
						case '<':
							if (_reader.Peek() == '=')
							{
								ReadComparason(Operators.LessThanOrEqual);
								_reader.Read();
							}
							else
							{
								ReadComparason(Operators.LessThan);
							}

							break;
						case '\n':
						{
							NullToken token = new NullToken();
							Tokens.Add(token);
							break;
						}
						case '(':
						{
							ParenthesisToken bracket = new ParenthesisToken();
							bracket.Open = true;
							Tokens.Add(bracket);
							break;
						}
						case ')':
						{
							ParenthesisToken bracket = new ParenthesisToken();
							bracket.Open = false;
							Tokens.Add(bracket);
							break;
						}
						case '+':
						{
							if (_reader.Peek() == '=')
							{
								AssignmentToken assignmentToken = new AssignmentToken
									{ AssignmentType = Assignments.AddEqual };
								Tokens.Add(assignmentToken);
							}
							else
							{
								ExpressionComparason comparason = new ExpressionComparason(Operators.PLUS);
								Tokens.Add(comparason);
							}
							
							break;
						}
						case '-':
						{
							if (_reader.Peek() == '=')
							{
								AssignmentToken assignmentToken = new AssignmentToken
									{ AssignmentType = Assignments.SubtractEqual };
								Tokens.Add(assignmentToken);
							}
							else
							{
								ExpressionComparason comparason = new ExpressionComparason(Operators.SUBTRACT);
								Tokens.Add(comparason);
							}
							
							break;
						}
						case '*':
						{
							if (_reader.Peek() == '=')
							{
								AssignmentToken assignmentToken = new AssignmentToken
									{ AssignmentType = Assignments.MultiplyEqual };
								Tokens.Add(assignmentToken);
							}
							else
							{
								ExpressionComparason comparason = new ExpressionComparason(Operators.MULTIPLY);
								Tokens.Add(comparason);
							}
							
							break;
						}
						case '/':
						{
							if (_reader.Peek() == '=')
							{
								AssignmentToken assignmentToken = new AssignmentToken
									{ AssignmentType = Assignments.DivEqual };
								Tokens.Add(assignmentToken);
							}
							else
							{
								ExpressionComparason comparason = new ExpressionComparason(Operators.DIVISION);
								Tokens.Add(comparason);
							}
							
							break;
						}
					}
				}
			}
		}

		public string ReadUntilChars(TextReader _reader, char _currenVal, char[] _endChars)
		{
			StringBuilder var = new StringBuilder();

			var.Append(_currenVal);

			while (true)
			{
				_currenVal = (char) _reader.Peek();
				bool broken = _currenVal == char.MaxValue; // check if end is hit

				foreach (char _endChar in _endChars)
				{
					if (_endChar == _currenVal)
					{
						broken = true;
						break;
					}
				}

				if (broken)
				{
					break;
				}

				_reader.Read();
				var.Append(_currenVal);
			}

			return var.ToString();
		}

		public string ReadUntilChar(TextReader _reader, char _currenVal, char _endChars)
		{
			StringBuilder var = new StringBuilder();

			var.Append(_currenVal);

			while (true)
			{
				_currenVal = (char) _reader.Read();

				if (_currenVal == _endChars)
				{
					break;
				}

				var.Append(_currenVal);
			}

			return var.ToString();
		}

		public void ReadIntegerLiteral(TextReader _reader, char _currentValue)
		{
			StringBuilder _integer = new StringBuilder();

			while (char.IsDigit(_currentValue))
			{
				_integer.Append(_currentValue);
				_currentValue = (char) _reader.Read();
			}
			
			ExpressionLiteral<int> _integerToken = new ExpressionLiteral<int>();
			_integerToken.ExpressionType = typeof(int);
			_integerToken.Value = Int32.Parse(_integer.ToString());
			Tokens.Add(_integerToken);
		}

		public void ReadStringLiteral(TextReader _reader)
		{
			ExpressionLiteral<string> _str = new ExpressionLiteral<string>();
			_str.Value = ReadUntilChar(_reader, '\0', '"');
			_str.ExpressionType = typeof(string);
			Tokens.Add(_str);
		}

		public void ReadAssignment(TextReader _reader)
		{
			if ((char) _reader.Peek() == '=')
			{
				ExpressionComparason operatorToken = new ExpressionComparason(Operators.Equal);
				Tokens.Add(operatorToken);
			}
			else
			{
				AssignmentToken assignmentToken = new AssignmentToken();
				assignmentToken.AssignmentType = Assignments.Equal;
				Tokens.Add(assignmentToken);
			}
		}

		public void ReadComparason(Operators _operator)
		{
			ExpressionComparason _token = new ExpressionComparason(_operator);
			Tokens.Add(_token);
		}

		public void ReadTextBlock(TextReader _reader, char _currenVal)
		{
			string _fullValue = ReadUntilChars(_reader, _currenVal, new[] {' ', '=', '!', '<', '>', '\n', '\r','"','(',')'});
			// check if or which value it is in the identifiers

			if (IdentifiersMap.TryGetValue(_fullValue, out Type _type))
			{
				Identifier _token = (Identifier) Activator.CreateInstance(_type);

				Tokens.Add(_token);
			} else if (OperatorsMap.TryGetValue(_fullValue, out Operators value))
			{
				ExpressionComparason token = new ExpressionComparason(value);

				Tokens.Add(token);
			}
			else
			{
				ExpressionVariable _token = new ExpressionVariable();
				_token.ValueName = _fullValue;

				Tokens.Add(_token);
			}
		}

		public static readonly Dictionary<string, Type> IdentifiersMap = new Dictionary<string, Type>()
		{
			{"if", typeof(IfToken)},
			{"then", typeof(NullToken)},
			{"endif", typeof(EndScopeToken)},
			{"switch", null},
			{"case", null},
			{"do", null},
			{"until", null},
			{"while", typeof(WhileToken)},
			{"endwhile", typeof(EndWhileToken)},
			{"for", typeof(ForToken)},
			{"to", typeof(NullToken)},
			{"next", typeof(NextToken)},
			{"step", typeof(NullToken)},
			{"const", null},
			{"global", null},
			{"procedure", null},
			{"endprocedure", null},
			{"function", null},
			{"return", null},
			{"endfunction", null},
			{"DIV", null},
			{"MOD", null}
		};

		public static readonly Dictionary<string, Operators> OperatorsMap = new Dictionary<string, Operators>()
		{
			{"AND", Operators.AND},
			{"OR", Operators.OR},
			{"NOT", Operators.NOT},
			{"+", Operators.PLUS},
			{"-", Operators.SUBTRACT},
			{"*", Operators.MULTIPLY},
			{"/", Operators.DIVISION},
			{"==", Operators.Equal},
			{"!=", Operators.NotEqual},
			{"<", Operators.LessThan},
			{"<=", Operators.LessThanOrEqual},
			{">", Operators.GreaterThan},
			{">=", Operators.GreaterThanOrEqual},
		};
	}

	public enum Identifiers
	{
		AND,
		OR,
		NOT,
		IF,
		THEN,
		ENDSCOPE,
		SWITCH,
		CASE,
		DO,
		UNTIL,
		WHILE,
		FOR,
		TO,
		NEXT,
		STEP,
		CONST,
		GLOBAL,
		PROCEDURE,
		FUNCTION,
		RETURN,
		DIV,
		MOD,
		ENDWHILE,
		ENDOFLINE
	}
	
	public enum Operators
	{
		Equal,
		NotEqual,
		LessThan,
		LessThanOrEqual,
		GreaterThan,
		GreaterThanOrEqual,
		AND,
		OR,
		NOT,
		MULTIPLY,
		DIVISION,
		PLUS,
		SUBTRACT,
	}

	public struct AssignmentToken
	{
		public Assignments AssignmentType;
	}

	public enum Assignments
	{
		Equal,
		AddEqual,
		SubtractEqual,
		MultiplyEqual,
		DivEqual
	}

	public struct ParenthesisToken
	{
		public bool Open;
	}
}