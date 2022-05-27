using System;
using System.Collections.Generic;
using OCRRFcompiler.IlGeneration;
using OCRRFcompiler.Scanning;

namespace OCRRFcompiler.Expressions
{
	public abstract class Expression
	{
		public Type ExpressionType;
		public abstract string GenerateIl(IlManager _manager);
		public Expression Copy()
		{
			return (Expression)this.MemberwiseClone();
		}
	}

	public class ExpressionComparason : Expression
	{
		public Operators Operator;
		public int Precedence;
		public override string GenerateIl(IlManager _manager)
		{
			string returnValue = "";
			switch (Operator)
			{
				case Operators.Equal:
				{
					returnValue = $"{_manager.NextFormattedAddress()} ceq\n";
					break;
				}
				case Operators.LessThan:
				{
					returnValue = $"{_manager.NextFormattedAddress()} clt\n";
					break;
				}
				case Operators.GreaterThan:
				{
					returnValue = $"{_manager.NextFormattedAddress()} cgt\n";
					break;
				}
				case Operators.PLUS:
				{
					returnValue = $"{_manager.NextFormattedAddress()} add\n";
					break;
				}
			}

			return returnValue;
		}

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
		public override string GenerateIl(IlManager _manager)
		{
			if (Value is string)
			{
				return $"{_manager.NextFormattedAddress()} ldstr \"{Value}\"\n";
			} else if (Value is int)
			{
				return $"{_manager.NextFormattedAddress()} ldc.i4.s {Value}\n";
			}

			return null;
		}
	}

	public class ExpressionVariable : Expression
	{
		public string ValueName;
		public int VariableIndex;
		public bool IsArg = false;
		public override string GenerateIl(IlManager _manager)
		{
			if (IsArg)
				return $"{_manager.NextFormattedAddress()} ldarg.{VariableIndex}";

			// else
			return $"{_manager.NextFormattedAddress()} ldloc.{VariableIndex}";
		}
	}

	public class BinaryExpression : Expression
	{
		public Expression LeftValue;
		public Expression RightValue;

		public ExpressionComparason Comparason;
		public override string GenerateIl(IlManager _manager)
		{
			string returnValue = "";

			returnValue += LeftValue.GenerateIl(_manager);
			returnValue += RightValue.GenerateIl(_manager);

			returnValue += Comparason.GenerateIl(_manager);

			return returnValue;
		}
	}
}