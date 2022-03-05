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
			
			return GenerateMainMethod(locals + ilCode);
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

		public string GenerateMainMethod(string _code)
		{
			return @"
					.assembly extern System.Runtime
					{
					  .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A )
					  .ver 5:0:0:0
					}
					.assembly extern System.Console
					{
					  .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A )                       
					  .ver 5:0:0:0
					}
					.assembly _
					{
						.custom instance void [System.Private.CoreLib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = (
					        01 00 08 00 00 00 00 00
					    )
					}
					.class private auto ansi abstract sealed beforefieldinit '<Program>$'
						extends [System.Private.CoreLib]System.Object
					{
						.custom instance void [System.Private.CoreLib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = (
						01 00 00 00
						)
						
						.method private hidebysig static 
							void '<Main>$' (
								string[] args
							) cil managed 
						{
							.maxstack 3
							.entrypoint
							" + _code + @"
						} // end of method '<Program>$'::'<Main>$'

					} // end of class <Program>$
					";
		}
	}
}