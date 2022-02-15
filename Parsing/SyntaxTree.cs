using System.Collections.Generic;
using OCRRFcompiler.Statements;

namespace OCRRFcompiler.Parsing
{
	public class SyntaxTree
	{
		public Scope GlobalScope = new Scope(null);

		private Scope CurrentScope;

		public SyntaxTree()
		{
			CurrentScope = GlobalScope;
		}
	
		public void AddStatement(Statement _toAdd)
		{
			CurrentScope.Statements.Add(_toAdd);
			if (_toAdd is ConditionalStatement conditionalStatement)
			{
				conditionalStatement.ConditionalScope = new Scope(CurrentScope);
				CurrentScope = conditionalStatement.ConditionalScope;
			}
		}

		public void EndScope()
		{
			CurrentScope = CurrentScope.Parent;
		}
	}

	public class Scope
	{
		public Scope Parent;
		public List<Statement> Statements = new List<Statement>();

		public Scope(Scope _parent)
		{
			Parent = _parent;
		}
	}
}