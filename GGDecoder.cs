using GGUtils.Properties;

namespace GGUtils
{
	internal class GGDecoder
	{


		public static Span<byte> GGPackDecode(Span<byte> buffer)
		{
			int key_idx = (buffer.Length + 0x78) & 0xFFFF;
			for(int i = 0; i < buffer.Length; ++i)
			{
				byte data = buffer[i];
				byte x = (byte)(data ^ Resources.key1[(key_idx + 0x78) & 0xFF] ^ Resources.key2[key_idx]);
				key_idx = (key_idx + (Resources.key1[key_idx & 0xFF] & 0xFF)) & 0xFFFF;
				buffer[i] = x;
			}

			return buffer;
		}

		public static Span<byte> SaveDecode(Span<byte> buffer)
		{ 
			uint idx = (uint)buffer.Length + 0x6c53a24a;

			for(int i = 0; i < Resources.saveKey.Length;)
				idx ^= Resources.saveKey[i++] + (idx << 5) + (idx >> 2);

			for (int i = 0; i < buffer.Length; ++i)
			{
				uint y = ((uint)i + 0x1664 + idx) & 0xFFFF;
                buffer[i] = (byte)(Resources.key2[y] ^ buffer[i]);
			}

			return buffer;
        }
	}
}