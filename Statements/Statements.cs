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
			string returnValue = "";
			// put an expression infront, which should push a 1 or 0 onto the stack for evaluation
			returnValue += Check.GenerateIl(_manager);
			string branchStatement = $"{_manager.NextFormattedAddress()} brfalse.s "; // this needs an index at the end to tell it where to branch to
			// Get the subscope Il
			string subscope = ConditionalScope.GenerateIl(_manager);
			// tell the branch statement to branch to the address one after the end of the subscope
			branchStatement += $"{IlManager.FormatAddress(_manager.address + 1)}";
			
			return returnValue + branchStatement + subscope;
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
			IncrementAssignment.Assignment = increment;
			
			string returnValue = $"{Assignment.GenerateIl(_manager)}" +
			                     $"{_manager.NextFormattedAddress()} br.s ";
			startAddress = _manager.address;

			string scopeIl = $"{ConditionalScope.GenerateIl(_manager)}";
			
			string incrementIl = $"{IncrementAssignment.GenerateIl(_manager)}";
			// sets the break to the start of the comparason
			returnValue += $"{IlManager.FormatAddress(_manager.address)}";
			string checkIl = $"{Check.GenerateIl(_manager)}";
			checkIl +=
				$"{_manager.NextFormattedAddress()} brtrue.s {IlManager.FormatAddress(startAddress)}";

			return returnValue + scopeIl + incrementIl + checkIl;
		}
	}
	
	public class AssignmentStatement : Statement
	{
		public ExpressionVariable Variable;
		public Expression Assignment;
		public override string GenerateIl(IlManager _manager)
		{
			return $"{Assignment.GenerateIl(_manager)}" +
			       $"{_manager.NextFormattedAddress()} stloc.{Variable.VariableIndex}";
		}
	}

	public class WhileLoopStatement : ConditionalStatement
	{
		public override string GenerateIl(IlManager _manager)
		{
			string branchToCondition = $"{_manager.NextFormattedAddress()} br.s ";

			long subscopeStart = _manager.address + 1;
			
			string subscope = ConditionalScope.GenerateIl(_manager);
			branchToCondition += $"{IlManager.FormatAddress(_manager.address + 1)}";

			string condidition = Check.GenerateIl(_manager) +
			                     $"{_manager.NextFormattedAddress()} blt.s {IlManager.FormatAddress(subscopeStart)}";

			return branchToCondition + subscope + condidition;
		}
	}
}