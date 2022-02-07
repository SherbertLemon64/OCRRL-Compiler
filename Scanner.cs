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
							if (_reader.Peek() == '	')
							{
								if (indented == -1)
								{
									indented = Tokens.Count;
									TabulationCharecter tab = new TabulationCharecter();
									tab.TabulationEnding = -1;
									Tokens.Add(tab);
								}
							}
							else if (indented > -1)
							{
								TabulationCharecter tabulationCharecter = (TabulationCharecter)Tokens[indented];
								tabulationCharecter.TabulationEnding = Tokens.Count;
								Tokens[indented] = tabulationCharecter;
							}
							break;
					}
				} 
			}
		}

		public void ReadVar(TextReader _reader, char _currenVal)
		{
			VarToken _token = new VarToken();
			_token.Value = ReadUntilChars(_reader, _currenVal, new []{' ', '=', '!', '<', '>', '\r', '\n'});
			Tokens.Add(_token);
		}

		public string ReadUntilChars(TextReader _reader, char _currenVal, char[] _endChars)
		{
			StringBuilder var = new StringBuilder();

			var.Append(_currenVal);

			while (true)
			{
				_currenVal = (char)_reader.Read();
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
			int _index = Array.IndexOf(IdentifierToken.Identifiers, _fullValue);
			if (_index != -1)
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
		public static readonly  string[] Identifiers = new[]
		{
			"AND",
			"OR",
			"NOT",
			"if",
			"then",
			"endif",
			"switch",
			"case",
			"do",
			"until",
			"while",
			"endwhile",
			"for",
			"to",
			"next",
			"step",
			"const",
			"global",
			"procedure",
			"endprocedure",
			"function",
			"return",
			"endfunction",
			"DIV",
			"MOD"
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
		ENDIF,
		SWITCH,
		CASE,
		DO,
		UNTIL,
		WHILE,
		ENDWHILE,
		FOR,
		TO,
		NEXT,
		STEP,
		CONST,
		GLOBAL,
		PROCEDURE,
		ENDPROCEDURE,
		FUNCTION,
		RETURN,
		ENDFUNCTION,
		DIV,
		MOD
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

	public struct TabulationCharecter
	{
		public int TabulationEnding;
	}
}