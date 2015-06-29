using System;

namespace Umbraco.Core.Media.Exif
{
    /// <summary>
    /// Represents an entry in the image file directory.
    /// </summary>
    internal struct ImageFileDirectoryEntry
    {
        /// <summary>
        /// The tag that identifies the field.
        /// </summary>
        public ushort Tag;
        /// <summary>
        /// Field type identifier.
        /// </summary>
        public ushort Type;
        /// <summary>
        /// Count of Type.
        /// </summary>
        public uint Count;
        /// <summary>
        /// Field data.
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFileDirectoryEntry"/> struct.
        /// </summary>
        /// <param name="tag">The tag that identifies the field.</param>
        /// <param name="type">Field type identifier.</param>
        /// <param name="count">Count of Type.</param>
        /// <param name="data">Field data.</param>
        public ImageFileDirectoryEntry(ushort tag, ushort type, uint count, byte[] data)
        {
            Tag = tag;
            Type = type;
            Count = count;
            Data = data;
        }

        /// <summary>
        /// Returns a <see cref="ImageFileDirectoryEntry"/> initialized from the given byte data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset into <paramref name="data"/>.</param>
        /// <param name="byteOrder">The byte order of <paramref name="data"/>.</param>
        /// <returns>A <see cref="ImageFileDirectoryEntry"/> initialized from the given byte data.</returns>
        public static ImageFileDirectoryEntry FromBytes(byte[] data, uint offset, BitConverterEx.ByteOrder byteOrder)
        {
            // Tag ID
            ushort tag = BitConverterEx.ToUInt16(data, offset, byteOrder, BitConverterEx.SystemByteOrder);

            // Tag Type
            ushort type = BitConverterEx.ToUInt16(data, offset + 2, byteOrder, BitConverterEx.SystemByteOrder);

            // Count of Type
            uint count = BitConverterEx.ToUInt32(data, offset + 4, byteOrder, BitConverterEx.SystemByteOrder);

            // Field value or offset to field data
            byte[] value = new byte[4];
            Array.Copy(data, offset + 8, value, 0, 4);

            // Calculate the bytes we need to read
            uint baselength = GetBaseLength(type);
            uint totallength = count * baselength;

            // If field value does not fit in 4 bytes
            // the value field is an offset to the actual
            // field value
            if (totallength > 4)
            {
                uint dataoffset = BitConverterEx.ToUInt32(value, 0, byteOrder, BitConverterEx.SystemByteOrder);
                value = new byte[totallength];
                Array.Copy(data, dataoffset, value, 0, totallength);
            }

            // Reverse array order if byte orders are different
            if (byteOrder != BitConverterEx.SystemByteOrder)
            {
                for (uint i = 0; i < count; i++)
                {
                    byte[] val = new byte[baselength];
                    Array.Copy(value, i * baselength, val, 0, baselength);
                    Array.Reverse(val);
                    Array.Copy(val, 0, value, i * baselength, baselength);
                }
            }

            return new ImageFileDirectoryEntry(tag, type, count, value);
        }

        /// <summary>
        /// Gets the base byte length for the given type.
        /// </summary>
        /// <param name="type">Type identifier.</param>
        private static uint GetBaseLength(ushort type)
        {
            if (type == 1 || type == 6) // BYTE and SBYTE
                return 1;
            else if (type == 2 || type == 7) // ASCII and UNDEFINED
                return 1;
            else if (type == 3 || type == 8) // SHORT and SSHORT
                return 2;
            else if (type == 4 || type == 9) // LONG and SLONG
                return 4;
            else if (type == 5 || type == 10) // RATIONAL (2xLONG) and SRATIONAL (2xSLONG)
                return 8;
            else if (type == 11) // FLOAT
                return 4;
            else if (type == 12) // DOUBLE
                return 8;

            throw new ArgumentException("Unknown type identifier.", "type");
        }
    }
}
