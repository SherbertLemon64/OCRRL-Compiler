using System;
using OCRRFcompiler.Errors;

namespace OCRRFcompiler.Parsing
{
	public class Reader<T>
	{
		private T[] values;
		public int index;

		public Reader(T[] _values)
		{
			values = _values;
		}

		public Reader(T[] _values, int _index)
		{
			values = _values;
			index = _index;
		}

		public bool IsEnd()
		{
			return values.Length < index + 1;
		}
		
		public T Read()
		{
			T val = values[index];
			index++;
			return val;
		}

		public T Seek()
		{
			return Seek(1);
		}

		public T Seek(int _distance)
		{
			try
			{
				return values[index + _distance];
			}
			catch
			{
				return default(T);
			}
		}

		public object ReadValueAsType(Type t)
		{
			object val = Read();
			Type valType = val.GetType();
			if (valType != t)
			{
				throw new UnexpectedTokenException(0,valType,t);
			}

			return val;
		}
		
		public T SeekCurrent()
		{
			return values[index];
		}
	}
}