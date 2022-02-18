using System.Collections.Generic;
using System.Linq;
using OCRRFcompiler.Expressions;
using OCRRFcompiler.Parsing;
using OCRRFcompiler.Statements;

namespace OCRRFcompiler.IlGeneration
{
	public class IlGenerator
	{
		private SyntaxTree Tree;

		public string GenerateIl(SyntaxTree _tree)
		{
			Tree = _tree;
			IlManager manager = new IlManager();
			SetVariableIndices(_tree.GlobalScope);

			return _tree.GlobalScope.GenerateIl(manager);
		}

		private void SetVariableIndices(Scope _topScope)
		{
			// if the topscope is the global scope set their variables as all are unique
			if (_topScope.Parent is null)
			{
				// can't do a for loop because hasSets aren't ordered
				int index = 0;
				foreach (ExpressionVariable v in _topScope.Variables)
				{
					v.VariableIndex = index;
				}
			}
			
			_topScope.UpperScopeVariables ??= GetAllUpperVariables(_topScope);
			foreach (ConditionalStatement c in _topScope.SubScopes)
			{
				int i = _topScope.UpperScopeVariables.Count;
				
				foreach (ExpressionVariable v in c.ConditionalScope.Variables)
				{
					// non-linear search
					bool removed = false;
					
					foreach (ExpressionVariable v2 in _topScope.UpperScopeVariables)
					{
						if (v.ValueName == v2.ValueName)
						{
							removed = true;
							c.ConditionalScope.Variables.Remove(v);
							break;
						}
					}
					// if it is a unique variable
					if (!removed)
					{
						v.VariableIndex = i;
						i++;
					}
				}
				
				SetVariableIndices(c.ConditionalScope);
			}
		}

		private HashSet<ExpressionVariable> GetAllUpperVariables(Scope _scope)
		{
			HashSet<ExpressionVariable> upperVariables = new HashSet<ExpressionVariable>(_scope.Variables);
			while (_scope.Parent is not null)
			{
				upperVariables.Concat(_scope.Parent.Variables);
				_scope = _scope.Parent;
			}

			return upperVariables;
		}
	}
}