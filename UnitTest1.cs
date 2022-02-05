using System;
using System.IO;
using NUnit.Framework;
using OCRRFcompiler.Scanning;

namespace OCRRFcompiler.Testing
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void Test1()
		{
			Parser parser = new Parser();
			parser.Parse(Directory.GetCurrentDirectory() + @"\Test.rl");

			Assert.Pass();
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