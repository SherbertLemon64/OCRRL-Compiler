using System;
using System.IO;
using NUnit.Framework;
using OCRRFcompiler.Scanning;

namespace OCRRFcompiler.Testing
{
	public class Scanning_Tests
	{
		private Scanner scanner = new Scanner();

		[Test]
		[Ignore("Takes 30ms")]
		public void ExternalScannerTest()
		{
			Scanner scan = new Scanner();
			string text = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Test.rl");
			TextReader read = new StringReader(text);
			
			scan.Scan(read);
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
			scanner.Scan(new StringReader(forLoop));
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
			scanner.Scan(new StringReader(whileLoop));
		}
	}
}