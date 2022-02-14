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
}