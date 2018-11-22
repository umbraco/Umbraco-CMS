using System;
using System.Text;

namespace Umbraco.Core.Media.Exif
{
    /// <summary>
    /// Creates exif properties from interoperability parameters.
    /// </summary>
    internal static class ExifPropertyFactory
    {
        #region Static Methods
        /// <summary>
        /// Creates an ExifProperty from the given interoperability parameters.
        /// </summary>
        /// <param name="tag">The tag id of the exif property.</param>
        /// <param name="type">The type id of the exif property.</param>
        /// <param name="count">Byte or component count.</param>
        /// <param name="value">Field data as an array of bytes.</param>
        /// <param name="byteOrder">Byte order of value.</param>
        /// <param name="ifd">IFD section containing this propery.</param>
        /// <param name="encoding">The encoding to be used for text metadata when the source encoding is unknown.</param>
        /// <returns>an ExifProperty initialized from the interoperability parameters.</returns>
        public static ExifProperty Get(ushort tag, ushort type, uint count, byte[] value, BitConverterEx.ByteOrder byteOrder, IFD ifd, Encoding encoding)
        {
            BitConverterEx conv = new BitConverterEx(byteOrder, BitConverterEx.SystemByteOrder);
            // Find the exif tag corresponding to given tag id
            ExifTag etag = ExifTagFactory.GetExifTag(ifd, tag);

            if (ifd == IFD.Zeroth)
            {
                if (tag == 0x103) // Compression
                    return new ExifEnumProperty<Compression>(ExifTag.Compression, (Compression)conv.ToUInt16(value, 0));
                else if (tag == 0x106) // PhotometricInterpretation
                    return new ExifEnumProperty<PhotometricInterpretation>(ExifTag.PhotometricInterpretation, (PhotometricInterpretation)conv.ToUInt16(value, 0));
                else if (tag == 0x112) // Orientation
                    return new ExifEnumProperty<Orientation>(ExifTag.Orientation, (Orientation)conv.ToUInt16(value, 0));
                else if (tag == 0x11c) // PlanarConfiguration
                    return new ExifEnumProperty<PlanarConfiguration>(ExifTag.PlanarConfiguration, (PlanarConfiguration)conv.ToUInt16(value, 0));
                else if (tag == 0x213) // YCbCrPositioning
                    return new ExifEnumProperty<YCbCrPositioning>(ExifTag.YCbCrPositioning, (YCbCrPositioning)conv.ToUInt16(value, 0));
                else if (tag == 0x128) // ResolutionUnit
                    return new ExifEnumProperty<ResolutionUnit>(ExifTag.ResolutionUnit, (ResolutionUnit)conv.ToUInt16(value, 0));
                else if (tag == 0x132) // DateTime
                    return new ExifDateTime(ExifTag.DateTime, ExifBitConverter.ToDateTime(value));
                else if (tag == 0x9c9b || tag == 0x9c9c ||  // Windows tags
                    tag == 0x9c9d || tag == 0x9c9e || tag == 0x9c9f)
                    return new WindowsByteString(etag, Encoding.Unicode.GetString(value).TrimEnd('\0'));
            }
            else if (ifd == IFD.EXIF)
            {
                if (tag == 0x9000) // ExifVersion
                    return new ExifVersion(ExifTag.ExifVersion, ExifBitConverter.ToAscii(value, Encoding.ASCII));
                else if (tag == 0xa000) // FlashpixVersion
                    return new ExifVersion(ExifTag.FlashpixVersion, ExifBitConverter.ToAscii(value, Encoding.ASCII));
                else if (tag == 0xa001) // ColorSpace
                    return new ExifEnumProperty<ColorSpace>(ExifTag.ColorSpace, (ColorSpace)conv.ToUInt16(value, 0));
                else if (tag == 0x9286) // UserComment
                {
                    // Default to ASCII
                    Encoding enc = Encoding.ASCII;
                    bool hasenc;
                    if (value.Length < 8)
                        hasenc = false;
                    else
                    {
                        hasenc = true;
                        string encstr = enc.GetString(value, 0, 8);
                        if (string.Compare(encstr, "ASCII\0\0\0", StringComparison.OrdinalIgnoreCase) == 0)
                            enc = Encoding.ASCII;
                        else if (string.Compare(encstr, "JIS\0\0\0\0\0", StringComparison.OrdinalIgnoreCase) == 0)
                            enc = Encoding.GetEncoding("Japanese (JIS 0208-1990 and 0212-1990)");
                        else if (string.Compare(encstr, "Unicode\0", StringComparison.OrdinalIgnoreCase) == 0)
                            enc = Encoding.Unicode;
                        else
                            hasenc = false;
                    }

                    string val = (hasenc ? enc.GetString(value, 8, value.Length - 8) : enc.GetString(value)).Trim('\0');

                    return new ExifEncodedString(ExifTag.UserComment, val, enc);
                }
                else if (tag == 0x9003) // DateTimeOriginal
                    return new ExifDateTime(ExifTag.DateTimeOriginal, ExifBitConverter.ToDateTime(value));
                else if (tag == 0x9004) // DateTimeDigitized
                    return new ExifDateTime(ExifTag.DateTimeDigitized, ExifBitConverter.ToDateTime(value));
                else if (tag == 0x8822) // ExposureProgram
                    return new ExifEnumProperty<ExposureProgram>(ExifTag.ExposureProgram, (ExposureProgram)conv.ToUInt16(value, 0));
                else if (tag == 0x9207) // MeteringMode
                    return new ExifEnumProperty<MeteringMode>(ExifTag.MeteringMode, (MeteringMode)conv.ToUInt16(value, 0));
                else if (tag == 0x9208) // LightSource
                    return new ExifEnumProperty<LightSource>(ExifTag.LightSource, (LightSource)conv.ToUInt16(value, 0));
                else if (tag == 0x9209) // Flash
                    return new ExifEnumProperty<Flash>(ExifTag.Flash, (Flash)conv.ToUInt16(value, 0), true);
                else if (tag == 0x9214) // SubjectArea
                {
                    if (count == 3)
                        return new ExifCircularSubjectArea(ExifTag.SubjectArea, ExifBitConverter.ToUShortArray(value, (int)count, byteOrder));
                    else if (count == 4)
                        return new ExifRectangularSubjectArea(ExifTag.SubjectArea, ExifBitConverter.ToUShortArray(value, (int)count, byteOrder));
                    else // count == 2
                        return new ExifPointSubjectArea(ExifTag.SubjectArea, ExifBitConverter.ToUShortArray(value, (int)count, byteOrder));
                }
                else if (tag == 0xa210) // FocalPlaneResolutionUnit
                    return new ExifEnumProperty<ResolutionUnit>(ExifTag.FocalPlaneResolutionUnit, (ResolutionUnit)conv.ToUInt16(value, 0), true);
                else if (tag == 0xa214) // SubjectLocation
                    return new ExifPointSubjectArea(ExifTag.SubjectLocation, ExifBitConverter.ToUShortArray(value, (int)count, byteOrder));
                else if (tag == 0xa217) // SensingMethod
                    return new ExifEnumProperty<SensingMethod>(ExifTag.SensingMethod, (SensingMethod)conv.ToUInt16(value, 0), true);
                else if (tag == 0xa300) // FileSource
                    return new ExifEnumProperty<FileSource>(ExifTag.FileSource, (FileSource)conv.ToUInt16(value, 0), true);
                else if (tag == 0xa301) // SceneType
                    return new ExifEnumProperty<SceneType>(ExifTag.SceneType, (SceneType)conv.ToUInt16(value, 0), true);
                else if (tag == 0xa401) // CustomRendered
                    return new ExifEnumProperty<CustomRendered>(ExifTag.CustomRendered, (CustomRendered)conv.ToUInt16(value, 0), true);
                else if (tag == 0xa402) // ExposureMode
                    return new ExifEnumProperty<ExposureMode>(ExifTag.ExposureMode, (ExposureMode)conv.ToUInt16(value, 0), true);
                else if (tag == 0xa403) // WhiteBalance
                    return new ExifEnumProperty<WhiteBalance>(ExifTag.WhiteBalance, (WhiteBalance)conv.ToUInt16(value, 0), true);
                else if (tag == 0xa406) // SceneCaptureType
                    return new ExifEnumProperty<SceneCaptureType>(ExifTag.SceneCaptureType, (SceneCaptureType)conv.ToUInt16(value, 0), true);
                else if (tag == 0xa407) // GainControl
                    return new ExifEnumProperty<GainControl>(ExifTag.GainControl, (GainControl)conv.ToUInt16(value, 0), true);
                else if (tag == 0xa408) // Contrast
                    return new ExifEnumProperty<Contrast>(ExifTag.Contrast, (Contrast)conv.ToUInt16(value, 0), true);
                else if (tag == 0xa409) // Saturation
                    return new ExifEnumProperty<Saturation>(ExifTag.Saturation, (Saturation)conv.ToUInt16(value, 0), true);
                else if (tag == 0xa40a) // Sharpness
                    return new ExifEnumProperty<Sharpness>(ExifTag.Sharpness, (Sharpness)conv.ToUInt16(value, 0), true);
                else if (tag == 0xa40c) // SubjectDistanceRange
                    return new ExifEnumProperty<SubjectDistanceRange>(ExifTag.SubjectDistanceRange, (SubjectDistanceRange)conv.ToUInt16(value, 0), true);
            }
            else if (ifd == IFD.GPS)
            {
                if (tag == 0) // GPSVersionID
                    return new ExifVersion(ExifTag.GPSVersionID, ExifBitConverter.ToString(value));
                else if (tag == 1) // GPSLatitudeRef
                    return new ExifEnumProperty<GPSLatitudeRef>(ExifTag.GPSLatitudeRef, (GPSLatitudeRef)value[0]);
                else if (tag == 2) // GPSLatitude
                    return new GPSLatitudeLongitude(ExifTag.GPSLatitude, ExifBitConverter.ToURationalArray(value, (int)count, byteOrder));
                else if (tag == 3) // GPSLongitudeRef
                    return new ExifEnumProperty<GPSLongitudeRef>(ExifTag.GPSLongitudeRef, (GPSLongitudeRef)value[0]);
                else if (tag == 4) // GPSLongitude
                    return new GPSLatitudeLongitude(ExifTag.GPSLongitude, ExifBitConverter.ToURationalArray(value, (int)count, byteOrder));
                else if (tag == 5) // GPSAltitudeRef
                    return new ExifEnumProperty<GPSAltitudeRef>(ExifTag.GPSAltitudeRef, (GPSAltitudeRef)value[0]);
                else if (tag == 7) // GPSTimeStamp
                    return new GPSTimeStamp(ExifTag.GPSTimeStamp, ExifBitConverter.ToURationalArray(value, (int)count, byteOrder));
                else if (tag == 9) // GPSStatus
                    return new ExifEnumProperty<GPSStatus>(ExifTag.GPSStatus, (GPSStatus)value[0]);
                else if (tag == 10) // GPSMeasureMode
                    return new ExifEnumProperty<GPSMeasureMode>(ExifTag.GPSMeasureMode, (GPSMeasureMode)value[0]);
                else if (tag == 12) // GPSSpeedRef
                    return new ExifEnumProperty<GPSSpeedRef>(ExifTag.GPSSpeedRef, (GPSSpeedRef)value[0]);
                else if (tag == 14) // GPSTrackRef
                    return new ExifEnumProperty<GPSDirectionRef>(ExifTag.GPSTrackRef, (GPSDirectionRef)value[0]);
                else if (tag == 16) // GPSImgDirectionRef
                    return new ExifEnumProperty<GPSDirectionRef>(ExifTag.GPSImgDirectionRef, (GPSDirectionRef)value[0]);
                else if (tag == 19) // GPSDestLatitudeRef
                    return new ExifEnumProperty<GPSLatitudeRef>(ExifTag.GPSDestLatitudeRef, (GPSLatitudeRef)value[0]);
                else if (tag == 20) // GPSDestLatitude
                    return new GPSLatitudeLongitude(ExifTag.GPSDestLatitude, ExifBitConverter.ToURationalArray(value, (int)count, byteOrder));
                else if (tag == 21) // GPSDestLongitudeRef
                    return new ExifEnumProperty<GPSLongitudeRef>(ExifTag.GPSDestLongitudeRef, (GPSLongitudeRef)value[0]);
                else if (tag == 22) // GPSDestLongitude
                    return new GPSLatitudeLongitude(ExifTag.GPSDestLongitude, ExifBitConverter.ToURationalArray(value, (int)count, byteOrder));
                else if (tag == 23) // GPSDestBearingRef
                    return new ExifEnumProperty<GPSDirectionRef>(ExifTag.GPSDestBearingRef, (GPSDirectionRef)value[0]);
                else if (tag == 25) // GPSDestDistanceRef
                    return new ExifEnumProperty<GPSDistanceRef>(ExifTag.GPSDestDistanceRef, (GPSDistanceRef)value[0]);
                else if (tag == 29) // GPSDate
                    return new ExifDateTime(ExifTag.GPSDateStamp, ExifBitConverter.ToDateTime(value, false));
                else if (tag == 30) // GPSDifferential
                    return new ExifEnumProperty<GPSDifferential>(ExifTag.GPSDifferential, (GPSDifferential)conv.ToUInt16(value, 0));
            }
            else if (ifd == IFD.Interop)
            {
                if (tag == 1) // InteroperabilityIndex
                    return new ExifAscii(ExifTag.InteroperabilityIndex, ExifBitConverter.ToAscii(value, Encoding.ASCII), Encoding.ASCII);
                else if (tag == 2) // InteroperabilityVersion
                    return new ExifVersion(ExifTag.InteroperabilityVersion, ExifBitConverter.ToAscii(value, Encoding.ASCII));
            }
            else if (ifd == IFD.First)
            {
                if (tag == 0x103) // Compression
                    return new ExifEnumProperty<Compression>(ExifTag.ThumbnailCompression, (Compression)conv.ToUInt16(value, 0));
                else if (tag == 0x106) // PhotometricInterpretation
                    return new ExifEnumProperty<PhotometricInterpretation>(ExifTag.ThumbnailPhotometricInterpretation, (PhotometricInterpretation)conv.ToUInt16(value, 0));
                else if (tag == 0x112) // Orientation
                    return new ExifEnumProperty<Orientation>(ExifTag.ThumbnailOrientation, (Orientation)conv.ToUInt16(value, 0));
                else if (tag == 0x11c) // PlanarConfiguration
                    return new ExifEnumProperty<PlanarConfiguration>(ExifTag.ThumbnailPlanarConfiguration, (PlanarConfiguration)conv.ToUInt16(value, 0));
                else if (tag == 0x213) // YCbCrPositioning
                    return new ExifEnumProperty<YCbCrPositioning>(ExifTag.ThumbnailYCbCrPositioning, (YCbCrPositioning)conv.ToUInt16(value, 0));
                else if (tag == 0x128) // ResolutionUnit
                    return new ExifEnumProperty<ResolutionUnit>(ExifTag.ThumbnailResolutionUnit, (ResolutionUnit)conv.ToUInt16(value, 0));
                else if (tag == 0x132) // DateTime
                    return new ExifDateTime(ExifTag.ThumbnailDateTime, ExifBitConverter.ToDateTime(value));
            }

            if (type == 1) // 1 = BYTE An 8-bit unsigned integer.
            {
                if (count == 1)
                    return new ExifByte(etag, value[0]);
                else
                    return new ExifByteArray(etag, value);
            }
            else if (type == 2) // 2 = ASCII An 8-bit byte containing one 7-bit ASCII code. 
            {
                return new ExifAscii(etag, ExifBitConverter.ToAscii(value, encoding), encoding);
            }
            else if (type == 3) // 3 = SHORT A 16-bit (2-byte) unsigned integer.
            {
                if (count == 1)
                    return new ExifUShort(etag, conv.ToUInt16(value, 0));
                else
                    return new ExifUShortArray(etag, ExifBitConverter.ToUShortArray(value, (int)count, byteOrder));
            }
            else if (type == 4) // 4 = LONG A 32-bit (4-byte) unsigned integer.
            {
                if (count == 1)
                    return new ExifUInt(etag, conv.ToUInt32(value, 0));
                else
                    return new ExifUIntArray(etag, ExifBitConverter.ToUIntArray(value, (int)count, byteOrder));
            }
            else if (type == 5) // 5 = RATIONAL Two LONGs. The first LONG is the numerator and the second LONG expresses the denominator.
            {
                if (count == 1)
                    return new ExifURational(etag, ExifBitConverter.ToURational(value, byteOrder));
                else
                    return new ExifURationalArray(etag, ExifBitConverter.ToURationalArray(value, (int)count, byteOrder));
            }
            else if (type == 7) // 7 = UNDEFINED An 8-bit byte that can take any value depending on the field definition.
                return new ExifUndefined(etag, value);
            else if (type == 9) // 9 = SLONG A 32-bit (4-byte) signed integer (2's complement notation).
            {
                if (count == 1)
                    return new ExifSInt(etag, conv.ToInt32(value, 0));
                else
                    return new ExifSIntArray(etag, ExifBitConverter.ToSIntArray(value, (int)count, byteOrder));
            }
            else if (type == 10) // 10 = SRATIONAL Two SLONGs. The first SLONG is the numerator and the second SLONG is the denominator.
            {
                if (count == 1)
                    return new ExifSRational(etag, ExifBitConverter.ToSRational(value, byteOrder));
                else
                    return new ExifSRationalArray(etag, ExifBitConverter.ToSRationalArray(value, (int)count, byteOrder));
            }
            else
                throw new ArgumentException("Unknown property type.");
        }
        #endregion
    }
}
