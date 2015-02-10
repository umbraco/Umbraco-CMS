using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Media.Exif
{
    /// <summary>
    /// Based on http://www.media.mit.edu/pia/Research/deepview/exif.html
    /// http://www.awaresystems.be/imaging/tiff/tifftags/privateifd/exif.html
    /// </summary>
    internal class ExifReader
    {
        public JpegInfo info { get; private set; }
        private bool littleEndian = false;

        public static JpegInfo ReadJpeg(Stream stream)
        {
            DateTime then = DateTime.Now;
            ExifReader reader = new ExifReader(stream);
            reader.info.LoadTime = (DateTime.Now - then);
            return reader.info;
        }

        protected ExifReader(Stream stream)
        {
            info = new JpegInfo();
            int a = stream.ReadByte();

            // ensure SOI marker
            if (a != JpegId.START || stream.ReadByte() != JpegId.SOI)
                return;

            info.IsValid = true;

            for (; ; )
            {
                int marker = 0, prev = 0;

                // find next marker
                for (a = 0; ; ++a)
                {
                    marker = stream.ReadByte();
                    if (marker != JpegId.START && prev == JpegId.START) break;
                    prev = marker;
                }

                // read section length
                int lenHigh = stream.ReadByte();
                int lenLow = stream.ReadByte();
                int itemlen = (lenHigh << 8) | lenLow;

                // read the section
                byte[] section = new byte[itemlen];
                section[0] = (byte)lenHigh;
                section[1] = (byte)lenLow;
                int bytesRead = stream.Read(section, 2, itemlen - 2);
                if (bytesRead != itemlen - 2)
                    return;

                switch (marker)
                {
                    case JpegId.SOS:
                        // start of stream: and we're done
                        return;
                    case JpegId.EOI:
                        // no data? no good.
                        return;
                    case JpegId.EXIF:
                        {
                            if (section[2] == 'E' &&
                                section[3] == 'x' &&
                                section[4] == 'i' &&
                                section[5] == 'f')
                            {
                                ProcessExif(section);
                            }
                        } break;
                    case JpegId.IPTC:
                        {
                            // don't care.
                        } break;
                    case 0xC0:
                    case 0xC1:
                    case 0xC2:
                    case 0xC3:
                    // case 0xC4: // not SOF
                    case 0xC5:
                    case 0xC6:
                    case 0xC7:
                    // case 0xC8: // not SOF
                    case 0xC9:
                    case 0xCA:
                    case 0xCB:
                    // case 0xCC: // not SOF
                    case 0xCD:
                    case 0xCE:
                    case 0xCF:
                        {
                            ProcessSOF(section, marker);
                        } break;
                    default:
                        {
                            // don't care.
                        } break;
                }

                section = null;
                GC.Collect();
            }
        }

        private void ProcessExif(byte[] section)
        {
            int idx = 6;
            if (section[idx++] != 0 ||
                section[idx++] != 0)
            {
                // "Exif" is not followed by 2 null bytes.
                return;
            }

            if (section[idx] == 'I' && section[idx + 1] == 'I')
            {
                // intel order
                littleEndian = true;
            }
            else
            {
                if (section[idx] == 'M' && section[idx + 1] == 'M')
                    littleEndian = false;
                else
                {
                    // unknown order...
                    return;
                }
            }
            idx += 2;

            int a = ExifIO.ReadUShort(section, idx, littleEndian);
            idx += 2;

            if (a != 0x002A)
            {
                // bad start...
                return;
            }

            a = ExifIO.ReadInt(section, idx, littleEndian);
            idx += 4;

            if (a < 8 || a > 16)
            {
                if (a < 16 || a > section.Length - 16)
                {
                    // invalid offset
                    return;
                }
            }

            ProcessExifDir(section, a + 8, 8, section.Length - 8, 0, ExifIFD.Exif);
        }

        private int DirOffset(int start, int num)
        {
            return start + 2 + 12 * num;
        }

        private void ProcessExifDir(byte[] section, int offsetDir, int offsetBase, int length, int depth, ExifIFD ifd)
        {
            if (depth > 4)
            {
                // corrupted Exif header...
                return;
            }

            ushort numEntries = ExifIO.ReadUShort(section, offsetDir, littleEndian);
            if (offsetDir + 2 + 12 * numEntries >= offsetDir + length)
            {
                // too long
                return;
            }

            int offset = 0;

            for (int de = 0; de < numEntries; ++de)
            {
                offset = DirOffset(offsetDir, de);
                ExifTag exifTag = new ExifTag(section, offset, offsetBase, length, littleEndian);

                if (!exifTag.IsValid)
                    continue;

                switch (exifTag.Tag)
                {
                    case (int)ExifIFD.Exif:
                        {
                            int dirStart = offsetBase + exifTag.GetInt(0);
                            if (dirStart <= offsetBase + length)
                            {
                                ProcessExifDir(section, dirStart, offsetBase, length, depth + 1, ExifIFD.Exif);
                            }
                        } break;
                    case (int)ExifIFD.Gps:
                        {
                            int dirStart = offsetBase + exifTag.GetInt(0);
                            if (dirStart <= offsetBase + length)
                            {
                                ProcessExifDir(section, dirStart, offsetBase, length, depth + 1, ExifIFD.Gps);
                            }
                        } break;
                    default:
                        {
                            exifTag.Populate(info, ifd);
                        } break;
                }
            }

            // final link defined?
            offset = DirOffset(offsetDir, numEntries) + 4;
            if (offset <= offsetBase + length)
            {
                offset = ExifIO.ReadInt(section, offsetDir + 2 + 12 * numEntries, littleEndian);
                if (offset > 0)
                {
                    int subDirStart = offsetBase + offset;
                    if (subDirStart <= offsetBase + length && subDirStart >= offsetBase)
                    {
                        ProcessExifDir(section, subDirStart, offsetBase, length, depth + 1, ifd);
                    }
                }
            }

            if (info.ThumbnailData == null && info.ThumbnailOffset > 0 && info.ThumbnailSize > 0)
            {
                // store it.
                info.ThumbnailData = new byte[info.ThumbnailSize];
                Array.Copy(section, offsetBase + info.ThumbnailOffset, info.ThumbnailData, 0, info.ThumbnailSize);
            }
        }

        private void ProcessSOF(byte[] section, int marker)
        {
            // bytes 1,2 is section len
            // byte 3 is precision (bytes per sample)
            info.Height = ((int)section[3] << 8) | (int)section[4];
            info.Width = ((int)section[5] << 8) | (int)section[6];
            int components = (int)section[7];

            info.IsColor = (components == 3);
        }
    }
}
