using System;

namespace OCRRFcompiler.Errors
{
	public class UnexpectedTokenException : Exception
	{
		public UnexpectedTokenException() : base() {}
		public UnexpectedTokenException(string _message) : base(_message) {} 
		public UnexpectedTokenException(string _message, Exception _inner) : base(_message, _inner) {}
		
		public UnexpectedTokenException(int _line, Type _expectedType, Type _type) : base($"Unexpected token {_type} When Expecting {_expectedType}, on line: {_line}")
		{
		}
	}
}