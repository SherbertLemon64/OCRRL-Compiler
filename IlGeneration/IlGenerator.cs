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

			string ilCode = _tree.GlobalScope.GenerateIl(manager);

			string locals = GenerateLocals();
			
			return null;
		}

		public string GenerateLocals()
		{
			string returnValue = $".locals init (\n";
			int i = 0;
			foreach (ExpressionVariable v in Tree.AllVariables)
			{
				returnValue += $"[{i}] {v.ExpressionType.Name} {v.ValueName}\n";
				i++;
			}

			returnValue += ")";
			
			return returnValue;
		}
	}
}