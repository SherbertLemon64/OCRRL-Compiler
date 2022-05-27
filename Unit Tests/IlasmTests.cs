using System.IO;
using NUnit.Framework;
using OCRRFcompiler.IlGeneration;
using OCRRFcompiler.Parsing;

namespace OCRRFcompiler.Testing
{
	
	public class IlasmTests
	{
		private IlGenerator generator = new IlGenerator();

		private Parser parser = new Parser();

		private string currentDir = Directory.GetCurrentDirectory();
		
		[Test]
		[Ignore("Takes 30ms")]
		public void ExternalGeneratorTest()
		{
			parser.ParseLocation(Directory.GetCurrentDirectory() + @"\Test.rl");
			
			string il = generator.GenerateIl(ref parser.Tree);
			
			File.WriteAllText("Test.il", il);

			System.Diagnostics.Process.Start(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\ilasm.exe", "Test.il");
		}

		[Test]
		public void TestForLoop()
		{
			string forLoop = 
				@"
				y = 0
				for x=0 to 10 step 1
					y = y + 1
				next x";
			parser.Parse(forLoop);
			
			string il = generator.GenerateIl(ref parser.Tree);
			
			File.WriteAllText("ForLoop.il", il);

			System.Diagnostics.Process.Start(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\ilasm.exe", "Forloop.il");
		}

		[Test]
		public void TestWhileLoop()
		{
			string whileLoop = 
				@"
				y = 0
				while y<10
					y = y + 1
				endwhile";
			parser.Parse(whileLoop);

			string il = generator.GenerateIl(ref parser.Tree);
			
			File.WriteAllText("WhileLoop.il", il);

			System.Diagnostics.Process.Start(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\ilasm.exe", "WhileLoop.il");
		}
	}
}