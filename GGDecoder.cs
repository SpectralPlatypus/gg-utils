using GGUtils.Properties;

namespace GGUtils
{
	internal class GGDecoder
	{


		public static Span<byte> GGPackDecode(Span<byte> buffer)
		{
			int key_idx = (buffer.Length + 0x78) & 0xFFFF;
			var key1 = Resources.key1;
			var key2 = Resources.key2;

			for (int i = 0; i < buffer.Length; ++i)
			{
				byte data = buffer[i];
				byte x = (byte)(data ^ key1[(key_idx + 0x78) & 0xFF] ^ key2[key_idx]);
				key_idx = (key_idx + (key1[key_idx & 0xFF] & 0xFF)) & 0xFFFF;
				buffer[i] = x;
			}

			return buffer;
		}

		public static Span<byte> SaveDecode(Span<byte> buffer)
		{ 
			uint idx = (uint)buffer.Length + 0x6c53a24a;
			var saveKey = Resources.saveKey;
			var key2 = Resources.key2;

			for(int i = 0; i < saveKey.Length;)
				idx ^= saveKey[i++] + (idx << 5) + (idx >> 2);

			for (int i = 0; i < buffer.Length; ++i)
			{
				uint y = ((uint)i + 0x1664 + idx) & 0xFFFF;
                buffer[i] = (byte)(key2[y] ^ buffer[i]);
			}

			return buffer;
        }
	}
}