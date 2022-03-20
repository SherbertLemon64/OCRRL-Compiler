using System;
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
		private readonly Dictionary<Type, string> TypeNames = new Dictionary<Type, string>()
		{
			{ typeof(string), "string" },
			{ typeof(int), "int32" },
		};

		public string GenerateIl(SyntaxTree _tree)
		{
			Tree = _tree;
			IlManager manager = new IlManager();

			string ilCode = _tree.GlobalScope.GenerateIl(manager);
			ilCode += $"{manager.NextFormattedAddress()} ret";
			
			string locals = GenerateLocals();
			
			return GenerateMainMethod(locals + ilCode);
		}

		public string GenerateLocals()
		{
			string returnValue = $".locals init (\n";
			// skip final one so it doesn't have a comma
			for (int i = 0; i < Tree.AllVariables.Count - 1; i++)
			{
				ExpressionVariable v = Tree.AllVariables[i];
				returnValue += $"[{i}] {TypeNames[v.ExpressionType]} {v.ValueName},\n";
			}

			ExpressionVariable finalVariable = Tree.AllVariables[Tree.AllVariables.Count - 1];
			returnValue += $"[{Tree.AllVariables.Count - 1}] {TypeNames[finalVariable.ExpressionType]} {finalVariable.ValueName}\n";
			
			returnValue += ")\n";
			
			return returnValue;
		}

		public string GenerateMainMethod(string _code)
		{
			return @"
					.assembly System.Runtime {}

					.method private hidebysig static void Main(string[] args) cil managed
					{
						{
							.maxstack 3
							.entrypoint
							" + _code + @"
						} // end of method '<Program>$'::'<Main>$'
					}
					";
		}
	}
}