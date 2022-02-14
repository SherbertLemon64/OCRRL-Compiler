using System;
using OCRRFcompiler.Scanning;

namespace OCRRFcompiler.Expressions
{
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
		public Operators Operator;
		public int Precedence;
		
		private static readonly int[] ComparasonsPrecedence = new[]
		{
			3,
			3,
			3,
			3,
			3,
			3,
			5,
			4,
			3,
			2,
			1,
			0,
			-1
		};
		public ExpressionComparason(Operators _operator)
		{
			Operator = _operator;
			Precedence = ComparasonsPrecedence[(int) _operator];
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
}