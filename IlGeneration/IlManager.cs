using System;

namespace OCRRFcompiler.IlGeneration
{
	public class IlManager
	{
		public long address { get; private set; }

		public string NextFormattedAddress()
		{
			string returnValue = $"IL_{Convert.ToString(address, 16).PadLeft(4, '0')}:";
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
			return $"IL_{Convert.ToString(_address, 16).PadLeft(4, '0')}";
		}
	}
}