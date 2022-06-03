using System;
using OCRRFcompiler.Parsing;
using OCRRFcompiler.Statements;

namespace OCRRFcompiler.Tokens
{
	public abstract class Identifier
	{
		public abstract Statement ParseStatement(Parser _parser);
	}

	public class IfToken : Identifier
	{
		public override ConditionalStatement ParseStatement(Parser _parser) => _parser.ParseConditionalStatement();
	}

	public class WhileToken : Identifier
	{
		public override WhileLoopStatement ParseStatement(Parser _parser) => _parser.ParseWhileLoop();
	}

	public class EndScopeToken : Identifier
	{
		public override ConditionalEndStatement ParseStatement(Parser _parser) => _parser.ParseConditionalEndStatement();
	}

	public class EndWhileToken : Identifier
	{
		public override LoopEndStatement ParseStatement(Parser _parser)
		{
			_parser.Swallow();
			return null;
		}
	}

	public class NextToken : Identifier
	{
		public override Statement ParseStatement(Parser _parser) => _parser.ParseSwallowNext();
	}

	public class ForToken : Identifier
	{
		public override ForLoopStatement ParseStatement(Parser _parser) => _parser.ParseForLoop();
	}

	public class NullToken : Identifier
	{
		public override Statement ParseStatement(Parser _parser) => null;
	}

	public class FunctionToken : Identifier 
	{
		public override Statement ParseStatement(Parser _parser) => _parser.ParseFunctionDefinitionStatement();
	}
}