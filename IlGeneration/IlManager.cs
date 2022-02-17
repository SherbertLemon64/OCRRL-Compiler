namespace OCRRFcompiler.IlGeneration
{
	public class IlManager
	{
		public long address { get; private set; }

		public string GetFormattedAddressAndIncrement()
		{
			string returnValue = $"IL_{address.ToString("X")}:";
			address++;
			return returnValue;
		}

		public long GetAddressAndIncrement()
		{
			long t_address = address;
			address++;
			
			return t_address;
		}

		public static string FormatAddress(long _address)
		{
			return $"IL_{_address.ToString("X")}";
		}
	}
}