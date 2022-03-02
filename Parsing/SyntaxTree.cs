using System.Collections.Generic;
using OCRRFcompiler.Expressions;
using OCRRFcompiler.IlGeneration;
using OCRRFcompiler.Statements;

namespace OCRRFcompiler.Parsing
{
	public class SyntaxTree
	{
		public Scope GlobalScope = new Scope(null);

		public Scope CurrentScope;

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
				CurrentScope.SubScopes.Add(conditionalStatement);
				CurrentScope = conditionalStatement.ConditionalScope;
			}
		}

		public void EndScope()
		{
			CurrentScope = CurrentScope.Parent;
		}

		public void AddVariable(ref ExpressionVariable _variable)
		{
			foreach (ExpressionVariable v in CurrentScope.UpperScopeVariables)
			{
				if (v.ValueName == _variable.ValueName)
				{
					_variable = v;
					return;
				}
			}
			
			foreach (ExpressionVariable v in CurrentScope.Variables)
			{
				if (v.ValueName == _variable.ValueName)
				{
					_variable = v;
					return;
				}
			}

			CurrentScope.Variables.Add(_variable);
		}
	}

	public class Scope
	{
		public Scope Parent;
		public List<Statement> Statements = new List<Statement>();
		public HashSet<ExpressionVariable> Variables = new HashSet<ExpressionVariable>();
		public List<ConditionalStatement> SubScopes = new List<ConditionalStatement>();

		public HashSet<ExpressionVariable> UpperScopeVariables;
		public string GenerateIl(IlManager _manager)
		{
			string returnValue = "";
			foreach (Statement s in Statements)
			{
				returnValue += s.GenerateIl(_manager);
			}

			return returnValue;
		}
		
		public Scope(Scope _parent)
		{
			Parent = _parent;
		}
	}
}