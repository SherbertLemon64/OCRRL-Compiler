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
		public override WhileLoopStatement ParseStatement(Parser _parser)
		{
			WhileLoopStatement whileLoopStatement = new WhileLoopStatement();
			ConditionalStatement conditionalStatement = _parser.ParseConditionalStatement();
			whileLoopStatement.Check = conditionalStatement.Check;
			whileLoopStatement.SubScope = conditionalStatement.SubScope;
			
			return whileLoopStatement;
		}
	}

	public class EndScopeToken : Identifier
	{
		public override ConditionalEndStatement ParseStatement(Parser _parser) => _parser.ParseConditionalEndStatement();
	}

	public class EndWhileToken : Identifier
	{
		public override LoopEndStatement ParseStatement(Parser _parser) => null;
	}

	public class NextToken : Identifier
	{
		public override Statement ParseStatement(Parser _parser)
		{
			// swallow variable after next statement
			_parser.ParseExpression();
			return null;
		}
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