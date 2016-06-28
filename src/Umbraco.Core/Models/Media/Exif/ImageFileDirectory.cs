using System;
using System.Collections.Generic;

namespace Umbraco.Core.Media.Exif
{
    /// <summary>
    /// Represents an image file directory.
    /// </summary>
    internal class ImageFileDirectory
    {
        /// <summary>
        /// The fields contained in this IFD.
        /// </summary>
        public List<ImageFileDirectoryEntry> Fields { get; private set; }
        /// <summary>
        /// Offset to the next IFD.
        /// </summary>
        public uint NextIFDOffset { get; private set; }
        /// <summary>
        /// Compressed image data.
        /// </summary>
        public List<TIFFStrip> Strips { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFileDirectory"/> class.
        /// </summary>
        public ImageFileDirectory()
        {
            Fields = new List<ImageFileDirectoryEntry>();
            Strips = new List<TIFFStrip>();
        }

        /// <summary>
        /// Returns a <see cref="ImageFileDirectory"/> initialized from the given byte data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset into <paramref name="data"/>.</param>
        /// <param name="byteOrder">The byte order of <paramref name="data"/>.</param>
        /// <returns>A <see cref="ImageFileDirectory"/> initialized from the given byte data.</returns>
        public static ImageFileDirectory FromBytes(byte[] data, uint offset, BitConverterEx.ByteOrder byteOrder)
        {
            ImageFileDirectory ifd = new ImageFileDirectory();
            BitConverterEx conv = new BitConverterEx(byteOrder, BitConverterEx.SystemByteOrder);

            List<uint> stripOffsets = new List<uint>();
            List<uint> stripLengths = new List<uint>();

            // Count
            ushort fieldcount = conv.ToUInt16(data, offset);

            // Read fields
            for (uint i = 0; i < fieldcount; i++)
            {
                uint fieldoffset = offset + 2 + 12 * i;
                ImageFileDirectoryEntry field = ImageFileDirectoryEntry.FromBytes(data, fieldoffset, byteOrder);
                ifd.Fields.Add(field);

                // Read strip offsets
                if (field.Tag == 273)
                {
                    int baselen = field.Data.Length / (int)field.Count;
                    for (uint j = 0; j < field.Count; j++)
                    {
                        byte[] val = new byte[baselen];
                        Array.Copy(field.Data, j * baselen, val, 0, baselen);
                        uint stripOffset = (field.Type == 3 ? (uint)BitConverter.ToUInt16(val, 0) : BitConverter.ToUInt32(val, 0));
                        stripOffsets.Add(stripOffset);
                    }
                }

                // Read strip lengths
                if (field.Tag == 279)
                {
                    int baselen = field.Data.Length / (int)field.Count;
                    for (uint j = 0; j < field.Count; j++)
                    {
                        byte[] val = new byte[baselen];
                        Array.Copy(field.Data, j * baselen, val, 0, baselen);
                        uint stripLength = (field.Type == 3 ? (uint)BitConverter.ToUInt16(val, 0) : BitConverter.ToUInt32(val, 0));
                        stripLengths.Add(stripLength);
                    }
                }
            }

            // Save strips
            if (stripOffsets.Count != stripLengths.Count)
                throw new NotValidTIFFileException();
            for (int i = 0; i < stripOffsets.Count; i++)
                ifd.Strips.Add(new TIFFStrip(data, stripOffsets[i], stripLengths[i]));

            // Offset to next ifd
            ifd.NextIFDOffset = conv.ToUInt32(data, offset + 2 + 12 * fieldcount);

            return ifd;
        }
    }
}
