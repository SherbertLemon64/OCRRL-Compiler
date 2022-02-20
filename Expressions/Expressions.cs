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
					returnValue = $"{_manager.GetFormattedAddressAndIncrement()} ceq\n";
					break;
				}
				case Operators.LessThan:
				{
					returnValue = $"{_manager.GetFormattedAddressAndIncrement()} clt\n";
					break;
				}
				case Operators.GreaterThan:
				{
					returnValue = $"{_manager.GetFormattedAddressAndIncrement()} cgt\n";
					break;
				}
				case Operators.PLUS:
				{
					returnValue = $"{_manager.GetFormattedAddressAndIncrement()} add\n";
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
				return $"{_manager.GetFormattedAddressAndIncrement()} ldstr \"{Value}\"\n";
			} else if (Value is int)
			{
				return $"{_manager.GetFormattedAddressAndIncrement()} ldc.i4.s {Value}\n";
			}

			return null;
		}
	}

	public class ExpressionVariable : Expression
	{
		public string ValueName;
		public int VariableIndex;
		public override string GenerateIl(IlManager _manager)
		{
			return $"{_manager.GetFormattedAddressAndIncrement()} ldloc.{VariableIndex}\n";
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