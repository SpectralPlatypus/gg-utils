using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GGUtils
{
    internal static class Extensions
    {
        public static ushort ReadUInt16(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            ushort result = MemoryMarshal.Read<ushort>(buffer[offset..]);
            offset += sizeof(ushort);

            return result;
        }
        public static uint ReadUInt32(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            uint result = MemoryMarshal.Read<uint>(buffer[offset..]);
            offset += sizeof(uint);

            return result;
        }
        public static int ReadInt32(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            int result = MemoryMarshal.Read<int>(buffer[offset..]);
            offset += sizeof(int);

            return result;
        }
        public static int ReadByte(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            byte result = buffer[offset];
            ++offset;

            return result;
        }
        public static string? ReadCString(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            var strPos = buffer[offset..];
            int nullPos = strPos.IndexOf((byte)0);
            if (nullPos == -1)
                return null;

            string result = Encoding.UTF8.GetString(strPos[..nullPos]);
            offset += nullPos;

            return result;
        }
    }
}
