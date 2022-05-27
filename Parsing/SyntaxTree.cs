using System.Collections.Generic;
using System.Reflection.Emit;
using OCRRFcompiler.Expressions;
using OCRRFcompiler.IlGeneration;
using OCRRFcompiler.Statements;

namespace OCRRFcompiler.Parsing
{
	public class SyntaxTree
	{
		public List<ExpressionVariable> AllVariables = new List<ExpressionVariable>();
		
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
				conditionalStatement.SubScope = new Scope(CurrentScope);
				CurrentScope.SubScopes.Add(conditionalStatement);
				CurrentScope = conditionalStatement.SubScope;
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
			_variable.VariableIndex = CurrentScope.UpperScopeVariables.Count + CurrentScope.Variables.Count;
			AllVariables.Add(_variable);
			CurrentScope.AddVariable(_variable);
		}
	}

	public class Scope
	{
		public Scope Parent;
		public List<Statement> Statements = new List<Statement>();
		public HashSet<ExpressionVariable> Variables = new HashSet<ExpressionVariable>();
		public List<SubScopeStatement> SubScopes = new List<SubScopeStatement>();

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

		public void AddVariable(ExpressionVariable _variable)
		{
			Variables.Add(_variable);
			foreach (ConditionalStatement lowerScope in SubScopes)
			{
				lowerScope.SubScope.UpperScopeVariables.Add(_variable);
			}
		}
		
		public Scope(Scope _parent)
		{
			Parent = _parent;
			if (_parent is not null)
			{
				UpperScopeVariables = new HashSet<ExpressionVariable>(_parent.Variables);
				UpperScopeVariables.UnionWith(_parent.UpperScopeVariables);
			}
			else
			{
				UpperScopeVariables = new HashSet<ExpressionVariable>();
			}
		}
	}
}