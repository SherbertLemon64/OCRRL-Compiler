using OCRRFcompiler.Expressions;

namespace OCRRFcompiler.Statements
{
	public class Statement
	{
	}
	
	public class ConditionalStatement : Statement
	{
		public Expression Check;
		public Scope ConditionalScope;
	}

	public class ConditionalEndStatement : Statement { }
	public class LoopEndStatement : ConditionalEndStatement
	{
		public ConditionalStatement LoopStart;
	}

	public class ForLoopEndStatement : LoopEndStatement
	{
		public ExpressionVariable Variable;
	}

	public class ForLoopStatement : ConditionalStatement
	{
		public AssignmentStatement Assignment;
		public int Step;
	}
	
	public class AssignmentStatement : Statement
	{
		public ExpressionVariable Variable;
		public Expression Assignment;
	}
}