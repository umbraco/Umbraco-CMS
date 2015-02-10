using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Media.Exif
{
    /// <summary>
    /// As per: http://www.media.mit.edu/pia/Research/deepview/exif.html
    /// </summary>
    internal enum ExifTagFormat
    {
        BYTE = 1,
        STRING = 2,
        USHORT = 3,
        ULONG = 4,
        URATIONAL = 5,
        SBYTE = 6,
        UNDEFINED = 7,
        SSHORT = 8,
        SLONG = 9,
        SRATIONAL = 10,
        SINGLE = 11,
        DOUBLE = 12,

        NUM_FORMATS = 12
    }

    internal class ExifTag
    {
        public int Tag { get; private set; }
        public ExifTagFormat Format { get; private set; }
        public int Components { get; private set; }
        public byte[] Data { get; private set; }
        public bool LittleEndian { get; private set; }

        private static int[] BytesPerFormat = new int[] { 0, 1, 1, 2, 4, 8, 1, 1, 2, 4, 8, 4, 8 };

        public ExifTag(byte[] section, int sectionOffset, int offsetBase, int length, bool littleEndian)
        {
            this.IsValid = false;
            this.Tag = ExifIO.ReadUShort(section, sectionOffset, littleEndian);
            int format = ExifIO.ReadUShort(section, sectionOffset + 2, littleEndian);
            if (format < 1 || format > 12)
                return;
            this.Format = (ExifTagFormat)format;
            this.Components = ExifIO.ReadInt(section, sectionOffset + 4, littleEndian);
            if (this.Components > 0x10000)
                return;
            this.LittleEndian = littleEndian;

            int byteCount = this.Components * BytesPerFormat[format];
            int valueOffset = 0;

            if (byteCount > 4)
            {
                int offsetVal = ExifIO.ReadInt(section, sectionOffset + 8, littleEndian);
                if (offsetVal + byteCount > length)
                {
                    // bad offset...
                    return;
                }
                valueOffset = offsetBase + offsetVal;
            }
            else
            {
                valueOffset = sectionOffset + 8;
            }
            this.Data = new byte[byteCount];
            Array.Copy(section, valueOffset, this.Data, 0, byteCount);
            this.IsValid = true;
        }

        public bool IsValid { get; private set; }

        private short ReadShort(int offset)
        {
            return ExifIO.ReadShort(Data, offset, LittleEndian);
        }

        private ushort ReadUShort(int offset)
        {
            return ExifIO.ReadUShort(Data, offset, LittleEndian);
        }

        private int ReadInt(int offset)
        {
            return ExifIO.ReadInt(Data, offset, LittleEndian);
        }

        private uint ReadUInt(int offset)
        {
            return ExifIO.ReadUInt(Data, offset, LittleEndian);
        }

        private float ReadSingle(int offset)
        {
            return ExifIO.ReadSingle(Data, offset, LittleEndian);
        }

        private double ReadDouble(int offset)
        {
            return ExifIO.ReadDouble(Data, offset, LittleEndian);
        }

        public bool IsNumeric
        {
            get
            {
                switch (Format)
                {
                    case ExifTagFormat.STRING:
                    case ExifTagFormat.UNDEFINED:
                        return false;
                    default:
                        return true;
                }
            }
        }

        public int GetInt(int componentIndex)
        {
            return (int)GetNumericValue(componentIndex);
        }

        public double GetNumericValue(int componentIndex)
        {
            switch (Format)
            {
                case ExifTagFormat.BYTE: return (double)this.Data[componentIndex];
                case ExifTagFormat.USHORT: return (double)ReadUShort(componentIndex * 2);
                case ExifTagFormat.ULONG: return (double)ReadUInt(componentIndex * 4);
                case ExifTagFormat.URATIONAL: return (double)ReadUInt(componentIndex * 8) / (double)ReadUInt((componentIndex * 8) + 4);
                case ExifTagFormat.SBYTE:
                    {
                        unchecked
                        {
                            return (double)(sbyte)this.Data[componentIndex];
                        }
                    }
                case ExifTagFormat.SSHORT: return (double)ReadShort(componentIndex * 2);
                case ExifTagFormat.SLONG: return (double)ReadInt(componentIndex * 4);
                case ExifTagFormat.SRATIONAL: return (double)ReadInt(componentIndex * 8) / (double)ReadInt((componentIndex * 8) + 4);
                case ExifTagFormat.SINGLE: return (double)ReadSingle(componentIndex * 4);
                case ExifTagFormat.DOUBLE: return ReadDouble(componentIndex * 8);
                default: return 0.0;
            }
        }

        public string GetStringValue()
        {
            return GetStringValue(0);
        }

        public string GetStringValue(int componentIndex)
        {
            switch (Format)
            {
                case ExifTagFormat.STRING:
                case ExifTagFormat.UNDEFINED:
                    return Encoding.UTF8.GetString(this.Data, 0, this.Data.Length).Trim(' ', '\t', '\r', '\n', '\0');
                case ExifTagFormat.URATIONAL:
                    return ReadUInt(componentIndex * 8).ToString() + "/" + ReadUInt((componentIndex * 8) + 4).ToString();
                case ExifTagFormat.SRATIONAL:
                    return ReadInt(componentIndex * 8).ToString() + "/" + ReadInt((componentIndex * 8) + 4).ToString();
                default:
                    return GetNumericValue(componentIndex).ToString();
            }
        }

        public virtual void Populate(JpegInfo info, ExifIFD ifd)
        {
            if (ifd == ExifIFD.Exif)
            {
                switch ((ExifId)this.Tag)
                {
                    case ExifId.ImageWidth: info.Width = GetInt(0); break;
                    case ExifId.ImageHeight: info.Height = GetInt(0); break;
                    case ExifId.Orientation: info.Orientation = (ExifOrientation)GetInt(0); break;
                    case ExifId.XResolution: info.XResolution = GetNumericValue(0); break;
                    case ExifId.YResolution: info.YResolution = GetNumericValue(0); break;
                    case ExifId.ResolutionUnit: info.ResolutionUnit = (ExifUnit)GetInt(0); break;
                    case ExifId.DateTime: info.DateTime = GetStringValue(); break;
                    case ExifId.DateTimeOriginal: info.DateTimeOriginal = GetStringValue(); break;
                    case ExifId.Description: info.Description = GetStringValue(); break;
                    case ExifId.Make: info.Make = GetStringValue(); break;
                    case ExifId.Model: info.Model = GetStringValue(); break;
                    case ExifId.Software: info.Software = GetStringValue(); break;
                    case ExifId.Artist: info.Artist = GetStringValue(); break;
                    case ExifId.ThumbnailOffset: info.ThumbnailOffset = GetInt(0); break;
                    case ExifId.ThumbnailLength: info.ThumbnailSize = GetInt(0); break;
                    case ExifId.Copyright: info.Copyright = GetStringValue(); break;
                    case ExifId.UserComment: info.UserComment = GetStringValue(); break;
                    case ExifId.ExposureTime: info.ExposureTime = GetNumericValue(0); break;
                    case ExifId.FNumber: info.FNumber = GetNumericValue(0); break;
                    case ExifId.FlashUsed: info.Flash = (ExifFlash)GetInt(0); break;
                    default: break;
                }
            }
            else if (ifd == ExifIFD.Gps)
            {
                switch ((ExifGps)this.Tag)
                {
                    case ExifGps.LatitudeRef:
                        {
                            if (GetStringValue() == "N") info.GpsLatitudeRef = ExifGpsLatitudeRef.North;
                            else if (GetStringValue() == "S") info.GpsLatitudeRef = ExifGpsLatitudeRef.South;
                        } break;
                    case ExifGps.LongitudeRef:
                        {
                            if (GetStringValue() == "E") info.GpsLongitudeRef = ExifGpsLongitudeRef.East;
                            else if (GetStringValue() == "W") info.GpsLongitudeRef = ExifGpsLongitudeRef.West;
                        } break;
                    case ExifGps.Latitude:
                        {
                            if (Components == 3)
                            {
                                info.GpsLatitude[0] = GetNumericValue(0);
                                info.GpsLatitude[1] = GetNumericValue(1);
                                info.GpsLatitude[2] = GetNumericValue(2);
                            }
                        } break;
                    case ExifGps.Longitude:
                        {
                            if (Components == 3)
                            {
                                info.GpsLongitude[0] = GetNumericValue(0);
                                info.GpsLongitude[1] = GetNumericValue(1);
                                info.GpsLongitude[2] = GetNumericValue(2);
                            }
                        } break;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(64);
            sb.Append("0x");
            sb.Append(this.Tag.ToString("X4"));
            sb.Append("-");
            sb.Append(((ExifId)this.Tag).ToString());
            if (this.Components > 0)
            {
                sb.Append(": (");
                sb.Append(GetStringValue(0));
                if (Format != ExifTagFormat.UNDEFINED && Format != ExifTagFormat.STRING)
                {
                    for (int i = 1; i < Components; ++i)
                        sb.Append(", " + GetStringValue(i));
                }
                sb.Append(")");
            }
            return sb.ToString();
        }
    }
}
