using OCRRFcompiler.Parsing;

namespace OCRRFcompiler.IlGeneration
{
	public class IlGenerator
	{
		private SyntaxTree Tree;

		public string GenerateIl(SyntaxTree _tree)
		{
			Tree = _tree;
			IlManager manager = new IlManager();

			return _tree.GlobalScope.GenerateIl(manager);
		}
	}
}