using System.IO;
using NUnit.Framework;
using OCRRFcompiler.IlGeneration;
using OCRRFcompiler.Parsing;

namespace OCRRFcompiler.Testing
{
	
	public class IlGeneratorTests
	{
		private IlGenerator generator = new IlGenerator();

		private Parser parser = new Parser();
		
		[Test]
		[Ignore("Takes 30ms")]
		public void ExternalGeneratorTest()
		{
			parser.ParseLocation(Directory.GetCurrentDirectory() + @"\Test.rl");
			
			string il = generator.GenerateIl(ref parser.Tree);
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
					z = 10
				next x";
			parser.Parse(forLoop);
			
			string il = generator.GenerateIl(ref parser.Tree);
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

			string il = generator.GenerateIl(ref parser.Tree);
		}
	}
}