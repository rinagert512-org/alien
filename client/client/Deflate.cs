using System;
using System.IO;
using System.IO.Compression;

namespace Alien
{
	internal class Deflate
	{
		/**
		 * Deflate compression to send data
		 * 
		 * @param byte[] data - data to compress
		 * 
		 * @return byte[] compressed data
		 */
		public static byte[] Compress(byte[] data)
		{
			if (data == null || data.Length < 1)
			{
				return null;
			}
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
				{
					deflateStream.Write(data, 0, data.Length);
					deflateStream.Close();
					byte[] array = new byte[memoryStream.Length];
					memoryStream.Seek(0L, SeekOrigin.Begin);
					memoryStream.Read(array, 0, (int)memoryStream.Length);
					result = array;
				}
			}
			return result;
		}
	}
}
