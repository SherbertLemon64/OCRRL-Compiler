using System;
using System.IO;
using NUnit.Framework;
using OCRRFcompiler.Scanning;

namespace OCRRFcompiler.Testing
{
	public class Tests
	{
		private Parser parser = new Parser();
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void TestParser()
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
				while y<10 
					y = y + 1
				endwhile";
			parser.Parse(whileLoop);
		}

		[Test]
		public void ScanTest()
		{
			Scanner scan = new Scanner();
			string text = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Test.rl");
			TextReader read = new StringReader(text);
			
			scan.Scan(read);
		}
	}
}