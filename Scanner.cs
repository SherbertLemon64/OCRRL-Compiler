using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Transactions;

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
				char c = (char)_reader.Read();
				if (c == ' ') { }
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
							ReadComparason(Comparasons.NotEqual);
							break;
						case '>':
							if (_reader.Peek() == '=')
							{
								ReadComparason(Comparasons.GreaterThanOrEqual);
								_reader.Read();
							}
							else
							{
								ReadComparason(Comparasons.GreaterThan);
							}
							break;
						case '<':
							if (_reader.Peek() == '=')
							{
								ReadComparason(Comparasons.LessThanOrEqual);
								_reader.Read();
							}
							else
							{
								ReadComparason(Comparasons.LessThan);
							}
							break;
						case '\n':
							IdentifierToken token = new IdentifierToken();
							token.Value = (int)Identifiers.ENDOFLINE;
							Tokens.Add(token);
							break;
						case '(':
						{
							Parenthesis bracket = new Parenthesis();
							bracket.Open = true;
							Tokens.Add(bracket);
							break;
						}
						case ')':
						{
							Parenthesis bracket = new Parenthesis();
							bracket.Open = false;
							Tokens.Add(bracket);
							break;
						}
					}
				} 
			}
		}

		public void ReadVar(TextReader _reader, char _currenVal)
		{
			VarToken _token = new VarToken();
			_token.Value = ReadUntilChars(_reader, _currenVal, new []{' ', '=', '!', '<', '>', '\r', '\n','(',')'});
			Tokens.Add(_token);
		}

		public string ReadUntilChars(TextReader _reader, char _currenVal, char[] _endChars)
		{
			StringBuilder var = new StringBuilder();

			var.Append(_currenVal);

			while (true)
			{
				_currenVal = (char)_reader.Peek();
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
				_currenVal = (char)_reader.Read();

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
			IntegerLiteralToken _integerToken = new IntegerLiteralToken();
			_integerToken.Value = Int32.Parse(_integer.ToString());
			Tokens.Add(_integerToken);
		}
		
		public void ReadStringLiteral(TextReader _reader)
		{
			StringLiteralToken _str = new StringLiteralToken();
			_str.Value = ReadUntilChar(_reader, '\0', '"');
			
			Tokens.Add(_str);
		}
		
		public void ReadAssignment(TextReader _reader)
		{
			if ((char) _reader.Peek() == '=')
			{
				ComparasonToken comparasonToken = new ComparasonToken();
				comparasonToken.ComparasonType = Comparasons.Equal;
				Tokens.Add(comparasonToken);
			}
			else
			{
				AssignmentToken assignmentToken = new AssignmentToken();
				assignmentToken.AssignmentType = Assignments.Equal;
				Tokens.Add(assignmentToken);
			}
		}

		public void ReadComparason(Comparasons _comparason)
		{
			ComparasonToken _token = new ComparasonToken();
			_token.ComparasonType = _comparason;
			Tokens.Add(_token);
		}

		public void ReadTextBlock(TextReader _reader, char _currenVal)
		{
			string _fullValue = ReadUntilChars(_reader, _currenVal, new []{' ', '=', '!', '<', '>', '\n','\r'});
			// check if or which value it is in the identifiers
			if (IdentifierToken.Identifiers.TryGetValue(_fullValue, out int _index))
			{
				// if _index is an AND OR NOT operator
				if (_index == 0 || _index == 1 || _index == 2)
				{
					ComparasonToken _token = new ComparasonToken();
					// offset to convert to the comparasons
					_token.ComparasonType = (Comparasons)(6 + _index);
				
					Tokens.Add(_token);
				}
				else
				{
					IdentifierToken _token = new IdentifierToken();
					_token.Value = _index;
				
					Tokens.Add(_token);
				}
			}
			else
			{
				VarToken _token = new VarToken();
				_token.Value = _fullValue;
				
				Tokens.Add(_token);
			}
		}
	}

	public struct IdentifierToken
	{
		public static readonly  Dictionary<string,int> Identifiers = new Dictionary<string, int>()
		{
			{"AND", 0},
			{"OR",1},
			{"NOT",2},
			{"if",3},
			{"then",4},
			{"endif",5},
			{"switch",6},
			{"case",7},
			{"do",8},
			{"until",9},
			{"while",10},
			{"endwhile",22},
			{"for",11},
			{"to",12},
			{"next",13},
			{"step",14},
			{"const",15},
			{"global",16},
			{"procedure",17},
			{"endprocedure",5},
			{"function",18},
			{"return",19},
			{"endfunction",5},
			{"DIV",20},
			{"MOD",21}
		};

		public int Value;
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
	
	public struct StringLiteralToken
	{
		public string Value;
	}

	public struct IntegerLiteralToken
	{
		public int Value;
	}
	public struct ComparasonToken
	{
		public Comparasons ComparasonType;
	}

	public enum Comparasons
	{
		Equal,
		NotEqual,
		LessThan,
		LessThanOrEqual,
		GreaterThan,
		GreaterThanOrEqual,
		AND,
		OR,
		NOT
	}
	
	
	public struct VarToken
	{
		public string Value;
		public Type VarType;
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
		TimesEqual,
		DivEqual
	}

	public struct Parenthesis
	{
		public bool Open;
	}
}