using OCRRFcompiler.Expressions;
using OCRRFcompiler.IlGeneration;
using OCRRFcompiler.Parsing;
using OCRRFcompiler.Scanning;

namespace OCRRFcompiler.Statements
{
	public abstract class Statement
	{
		public abstract string GenerateIl(IlManager _manager);
	}
	
	public class ConditionalStatement : Statement
	{
		public Expression Check;
		public Scope ConditionalScope;
		public override string GenerateIl(IlManager _manager)
		{
			throw new System.NotImplementedException();
		}
	}

	public class ConditionalEndStatement : Statement
	{
		public override string GenerateIl(IlManager _manager)
		{
			throw new System.NotImplementedException();
		}
	}
	public class LoopEndStatement : ConditionalEndStatement
	{
		public ConditionalStatement LoopStart;
		public override string GenerateIl(IlManager _manager)
		{
			throw new System.NotImplementedException();
		}
	}

	public class ForLoopStatement : ConditionalStatement
	{
		public AssignmentStatement Assignment;
		public int Step;

		public AssignmentStatement IncrementAssignment;
		public override string GenerateIl(IlManager _manager)
		{
			long startAddress = _manager.address;

			IncrementAssignment = new AssignmentStatement();
			IncrementAssignment.Variable = Assignment.Variable;
			BinaryExpression increment = new BinaryExpression();
			increment.Comparason = new ExpressionComparason(Operators.PLUS);
			increment.LeftValue = Assignment.Variable;
			increment.RightValue = new ExpressionLiteral<int>() {Value = Step};

			string returnValue = $"{Assignment.GenerateIl(_manager)}\n" +
			                     $"{_manager.GetFormattedAddressAndIncrement()} br.s {IlManager.FormatAddress(startAddress)}\n";
			startAddress = _manager.address;
			returnValue += $"{ConditionalScope.GenerateIl(_manager)}" +
			               $"{Check.GenerateIl(_manager)}" +
			               $"{_manager.GetFormattedAddressAndIncrement()} blt.s {IlManager.FormatAddress(startAddress)}\n";


			return returnValue;
		}
	}
	
	public class AssignmentStatement : Statement
	{
		public ExpressionVariable Variable;
		public Expression Assignment;
		public override string GenerateIl(IlManager _manager)
		{
			return $"{Assignment.GenerateIl(_manager)}\n" +
			       $"{_manager.GetFormattedAddressAndIncrement()} stloc.{Variable.VariableIndex}\n";
		}
	}
}