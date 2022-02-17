using System.IO;
using NUnit.Framework;
using OCRRFcompiler.Parsing;

namespace OCRRFcompiler.Testing
{
	public class ParsingTests
	{
		private Parser parser = new Parser();
		[Test]
		public void ExternalParserTest()
		{
			parser.ParseLocation(Directory.GetCurrentDirectory() + @"\Test.rl");

			Assert.Pass();
		}

		[Test]
		public void TestForLoop()
		{
			string forLoop = 
				@"
				y = 0
				for x=0 to 100 step 2
					y = 2
				next x";
			parser.Parse(forLoop);
		}

		[Test]
		public void TestWhileLoop()
		{
			string whileLoop = 
				@"
				y = 0
				while y<10 AND y > 5
					y = y + 1
				endwhile";
			parser.Parse(whileLoop);
		}
	}
}