using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Media.Exif
{
    /// <summary>
    /// Utility to handle multi-byte primitives in both big and little endian.
    /// http://msdn.microsoft.com/en-us/library/system.bitconverter.islittleendian.aspx
    /// http://en.wikipedia.org/wiki/Endianness
    /// </summary>
    internal static class ExifIO
    {
        public static short ReadShort(byte[] Data, int offset, bool littleEndian)
        {
            if ((littleEndian && BitConverter.IsLittleEndian) ||
                (!littleEndian && !BitConverter.IsLittleEndian))
            {
                return BitConverter.ToInt16(Data, offset);
            }
            else
            {
                byte[] beBytes = new byte[2] { Data[offset + 1], Data[offset] };
                return BitConverter.ToInt16(beBytes, 0);
            }
        }

        public static ushort ReadUShort(byte[] Data, int offset, bool littleEndian)
        {
            if ((littleEndian && BitConverter.IsLittleEndian) ||
                (!littleEndian && !BitConverter.IsLittleEndian))
            {
                return BitConverter.ToUInt16(Data, offset);
            }
            else
            {
                byte[] beBytes = new byte[2] { Data[offset + 1], Data[offset] };
                return BitConverter.ToUInt16(beBytes, 0);
            }
        }

        public static int ReadInt(byte[] Data, int offset, bool littleEndian)
        {
            if ((littleEndian && BitConverter.IsLittleEndian) ||
                (!littleEndian && !BitConverter.IsLittleEndian))
            {
                return BitConverter.ToInt32(Data, offset);
            }
            else
            {
                byte[] beBytes = new byte[4] { Data[offset + 3], Data[offset + 2], Data[offset + 1], Data[offset] };
                return BitConverter.ToInt32(beBytes, 0);
            }
        }

        public static uint ReadUInt(byte[] Data, int offset, bool littleEndian)
        {
            if ((littleEndian && BitConverter.IsLittleEndian) ||
                (!littleEndian && !BitConverter.IsLittleEndian))
            {
                return BitConverter.ToUInt32(Data, offset);
            }
            else
            {
                byte[] beBytes = new byte[4] { Data[offset + 3], Data[offset + 2], Data[offset + 1], Data[offset] };
                return BitConverter.ToUInt32(beBytes, 0);
            }
        }

        public static float ReadSingle(byte[] Data, int offset, bool littleEndian)
        {
            if ((littleEndian && BitConverter.IsLittleEndian) ||
                (!littleEndian && !BitConverter.IsLittleEndian))
            {
                return BitConverter.ToSingle(Data, offset);
            }
            else
            {
                // need to swap the data first
                byte[] beBytes = new byte[4] { Data[offset + 3], Data[offset + 2], Data[offset + 1], Data[offset] };
                return BitConverter.ToSingle(beBytes, 0);
            }
        }

        public static double ReadDouble(byte[] Data, int offset, bool littleEndian)
        {
            if ((littleEndian && BitConverter.IsLittleEndian) ||
                (!littleEndian && !BitConverter.IsLittleEndian))
            {
                return BitConverter.ToDouble(Data, offset);
            }
            else
            {
                // need to swap the data first
                byte[] beBytes = new byte[8] {
                    Data[offset + 7], Data[offset + 6], Data[offset + 5], Data[offset + 4],
                    Data[offset + 3], Data[offset + 2], Data[offset + 1], Data[offset]};
                return BitConverter.ToDouble(beBytes, 0);
            }
        }
    }
}
