using System.Text;

namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Creates exif properties from interoperability parameters.
/// </summary>
internal static class ExifPropertyFactory
{
    #region Static Methods

    /// <summary>
    ///     Creates an ExifProperty from the given interoperability parameters.
    /// </summary>
    /// <param name="tag">The tag id of the exif property.</param>
    /// <param name="type">The type id of the exif property.</param>
    /// <param name="count">Byte or component count.</param>
    /// <param name="value">Field data as an array of bytes.</param>
    /// <param name="byteOrder">Byte order of value.</param>
    /// <param name="ifd">IFD section containing this property.</param>
    /// <param name="encoding">The encoding to be used for text metadata when the source encoding is unknown.</param>
    /// <returns>an ExifProperty initialized from the interoperability parameters.</returns>
    public static ExifProperty Get(ushort tag, ushort type, uint count, byte[] value, BitConverterEx.ByteOrder byteOrder, IFD ifd, Encoding encoding)
    {
        var conv = new BitConverterEx(byteOrder, BitConverterEx.SystemByteOrder);

        // Find the exif tag corresponding to given tag id
        ExifTag etag = ExifTagFactory.GetExifTag(ifd, tag);

        if (ifd == IFD.Zeroth)
        {
            // Compression
            if (tag == 0x103)
            {
                return new ExifEnumProperty<Compression>(ExifTag.Compression, (Compression)conv.ToUInt16(value, 0));
            }

            // PhotometricInterpretation
            if (tag == 0x106)
            {
                return new ExifEnumProperty<PhotometricInterpretation>(
                    ExifTag.PhotometricInterpretation,
                    (PhotometricInterpretation)conv.ToUInt16(value, 0));
            }

            // Orientation
            if (tag == 0x112)
            {
                return new ExifEnumProperty<Orientation>(ExifTag.Orientation, (Orientation)conv.ToUInt16(value, 0));
            }

            // PlanarConfiguration
            if (tag == 0x11c)
            {
                return new ExifEnumProperty<PlanarConfiguration>(
                    ExifTag.PlanarConfiguration,
                    (PlanarConfiguration)conv.ToUInt16(value, 0));
            }

            // YCbCrPositioning
            if (tag == 0x213)
            {
                return new ExifEnumProperty<YCbCrPositioning>(
                    ExifTag.YCbCrPositioning,
                    (YCbCrPositioning)conv.ToUInt16(value, 0));
            }

            // ResolutionUnit
            if (tag == 0x128)
            {
                return new ExifEnumProperty<ResolutionUnit>(
                    ExifTag.ResolutionUnit,
                    (ResolutionUnit)conv.ToUInt16(value, 0));
            }

            // DateTime
            if (tag == 0x132)
            {
                return new ExifDateTime(ExifTag.DateTime, ExifBitConverter.ToDateTime(value));
            }

            if (tag == 0x9c9b || tag == 0x9c9c || // Windows tags
                tag == 0x9c9d || tag == 0x9c9e || tag == 0x9c9f)
            {
                return new WindowsByteString(
                    etag,
                    Encoding.Unicode.GetString(value).TrimEnd(Constants.CharArrays.NullTerminator));
            }
        }
        else if (ifd == IFD.EXIF)
        {
            // ExifVersion
            if (tag == 0x9000)
            {
                return new ExifVersion(ExifTag.ExifVersion, ExifBitConverter.ToAscii(value, Encoding.ASCII));
            }

            // FlashpixVersion
            if (tag == 0xa000)
            {
                return new ExifVersion(ExifTag.FlashpixVersion, ExifBitConverter.ToAscii(value, Encoding.ASCII));
            }

            // ColorSpace
            if (tag == 0xa001)
            {
                return new ExifEnumProperty<ColorSpace>(ExifTag.ColorSpace, (ColorSpace)conv.ToUInt16(value, 0));
            }

            // UserComment
            if (tag == 0x9286)
            {
                // Default to ASCII
                Encoding enc = Encoding.ASCII;
                bool hasenc;
                if (value.Length < 8)
                {
                    hasenc = false;
                }
                else
                {
                    hasenc = true;
                    var encstr = enc.GetString(value, 0, 8);
                    if (string.Compare(encstr, "ASCII\0\0\0", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        enc = Encoding.ASCII;
                    }
                    else if (string.Compare(encstr, "JIS\0\0\0\0\0", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        enc = Encoding.GetEncoding("Japanese (JIS 0208-1990 and 0212-1990)");
                    }
                    else if (string.Compare(encstr, "Unicode\0", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        enc = Encoding.Unicode;
                    }
                    else
                    {
                        hasenc = false;
                    }
                }

                var val = (hasenc ? enc.GetString(value, 8, value.Length - 8) : enc.GetString(value)).Trim(
                    Constants.CharArrays.NullTerminator);

                return new ExifEncodedString(ExifTag.UserComment, val, enc);
            }

            // DateTimeOriginal
            if (tag == 0x9003)
            {
                return new ExifDateTime(ExifTag.DateTimeOriginal, ExifBitConverter.ToDateTime(value));
            }

            // DateTimeDigitized
            if (tag == 0x9004)
            {
                return new ExifDateTime(ExifTag.DateTimeDigitized, ExifBitConverter.ToDateTime(value));
            }

            // ExposureProgram
            if (tag == 0x8822)
            {
                return new ExifEnumProperty<ExposureProgram>(
                    ExifTag.ExposureProgram,
                    (ExposureProgram)conv.ToUInt16(value, 0));
            }

            // MeteringMode
            if (tag == 0x9207)
            {
                return new ExifEnumProperty<MeteringMode>(ExifTag.MeteringMode, (MeteringMode)conv.ToUInt16(value, 0));
            }

            // LightSource
            if (tag == 0x9208)
            {
                return new ExifEnumProperty<LightSource>(ExifTag.LightSource, (LightSource)conv.ToUInt16(value, 0));
            }

            // Flash
            if (tag == 0x9209)
            {
                return new ExifEnumProperty<Flash>(ExifTag.Flash, (Flash)conv.ToUInt16(value, 0), true);
            }

            // SubjectArea
            if (tag == 0x9214)
            {
                if (count == 3)
                {
                    return new ExifCircularSubjectArea(
                        ExifTag.SubjectArea,
                        ExifBitConverter.ToUShortArray(value, (int)count, byteOrder));
                }

                if (count == 4)
                {
                    return new ExifRectangularSubjectArea(
                        ExifTag.SubjectArea,
                        ExifBitConverter.ToUShortArray(value, (int)count, byteOrder));
                }

                return new ExifPointSubjectArea(
                    ExifTag.SubjectArea,
                    ExifBitConverter.ToUShortArray(value, (int)count, byteOrder));
            }

            // FocalPlaneResolutionUnit
            if (tag == 0xa210)
            {
                return new ExifEnumProperty<ResolutionUnit>(
                    ExifTag.FocalPlaneResolutionUnit,
                    (ResolutionUnit)conv.ToUInt16(value, 0),
                    true);
            }

            // SubjectLocation
            if (tag == 0xa214)
            {
                return new ExifPointSubjectArea(
                    ExifTag.SubjectLocation,
                    ExifBitConverter.ToUShortArray(value, (int)count, byteOrder));
            }

            // SensingMethod
            if (tag == 0xa217)
            {
                return new ExifEnumProperty<SensingMethod>(
                    ExifTag.SensingMethod,
                    (SensingMethod)conv.ToUInt16(value, 0),
                    true);
            }

            // FileSource
            if (tag == 0xa300)
            {
                return new ExifEnumProperty<FileSource>(ExifTag.FileSource, (FileSource)conv.ToUInt16(value, 0), true);
            }

            // SceneType
            if (tag == 0xa301)
            {
                return new ExifEnumProperty<SceneType>(ExifTag.SceneType, (SceneType)conv.ToUInt16(value, 0), true);
            }

            // CustomRendered
            if (tag == 0xa401)
            {
                return new ExifEnumProperty<CustomRendered>(
                    ExifTag.CustomRendered,
                    (CustomRendered)conv.ToUInt16(value, 0),
                    true);
            }

            // ExposureMode
            if (tag == 0xa402)
            {
                return new ExifEnumProperty<ExposureMode>(ExifTag.ExposureMode, (ExposureMode)conv.ToUInt16(value, 0), true);
            }

            // WhiteBalance
            if (tag == 0xa403)
            {
                return new ExifEnumProperty<WhiteBalance>(ExifTag.WhiteBalance, (WhiteBalance)conv.ToUInt16(value, 0), true);
            }

            // SceneCaptureType
            if (tag == 0xa406)
            {
                return new ExifEnumProperty<SceneCaptureType>(
                    ExifTag.SceneCaptureType,
                    (SceneCaptureType)conv.ToUInt16(value, 0),
                    true);
            }

            // GainControl
            if (tag == 0xa407)
            {
                return new ExifEnumProperty<GainControl>(ExifTag.GainControl, (GainControl)conv.ToUInt16(value, 0), true);
            }

            // Contrast
            if (tag == 0xa408)
            {
                return new ExifEnumProperty<Contrast>(ExifTag.Contrast, (Contrast)conv.ToUInt16(value, 0), true);
            }

            // Saturation
            if (tag == 0xa409)
            {
                return new ExifEnumProperty<Saturation>(ExifTag.Saturation, (Saturation)conv.ToUInt16(value, 0), true);
            }

            // Sharpness
            if (tag == 0xa40a)
            {
                return new ExifEnumProperty<Sharpness>(ExifTag.Sharpness, (Sharpness)conv.ToUInt16(value, 0), true);
            }

            // SubjectDistanceRange
            if (tag == 0xa40c)
            {
                return new ExifEnumProperty<SubjectDistanceRange>(
                    ExifTag.SubjectDistanceRange,
                    (SubjectDistanceRange)conv.ToUInt16(value, 0),
                    true);
            }
        }
        else if (ifd == IFD.GPS)
        {
            // GPSVersionID
            if (tag == 0)
            {
                return new ExifVersion(ExifTag.GPSVersionID, ExifBitConverter.ToString(value));
            }

            // GPSLatitudeRef
            if (tag == 1)
            {
                return new ExifEnumProperty<GPSLatitudeRef>(ExifTag.GPSLatitudeRef, (GPSLatitudeRef)value[0]);
            }

            // GPSLatitude
            if (tag == 2)
            {
                return new GPSLatitudeLongitude(
                    ExifTag.GPSLatitude,
                    ExifBitConverter.ToURationalArray(value, (int)count, byteOrder));
            }

            // GPSLongitudeRef
            if (tag == 3)
            {
                return new ExifEnumProperty<GPSLongitudeRef>(ExifTag.GPSLongitudeRef, (GPSLongitudeRef)value[0]);
            }

            // GPSLongitude
            if (tag == 4)
            {
                return new GPSLatitudeLongitude(
                    ExifTag.GPSLongitude,
                    ExifBitConverter.ToURationalArray(value, (int)count, byteOrder));
            }

            // GPSAltitudeRef
            if (tag == 5)
            {
                return new ExifEnumProperty<GPSAltitudeRef>(ExifTag.GPSAltitudeRef, (GPSAltitudeRef)value[0]);
            }

            // GPSTimeStamp
            if (tag == 7)
            {
                return new GPSTimeStamp(
                    ExifTag.GPSTimeStamp,
                    ExifBitConverter.ToURationalArray(value, (int)count, byteOrder));
            }

            // GPSStatus
            if (tag == 9)
            {
                return new ExifEnumProperty<GPSStatus>(ExifTag.GPSStatus, (GPSStatus)value[0]);
            }

            // GPSMeasureMode
            if (tag == 10)
            {
                return new ExifEnumProperty<GPSMeasureMode>(ExifTag.GPSMeasureMode, (GPSMeasureMode)value[0]);
            }

            // GPSSpeedRef
            if (tag == 12)
            {
                return new ExifEnumProperty<GPSSpeedRef>(ExifTag.GPSSpeedRef, (GPSSpeedRef)value[0]);
            }

            // GPSTrackRef
            if (tag == 14)
            {
                return new ExifEnumProperty<GPSDirectionRef>(ExifTag.GPSTrackRef, (GPSDirectionRef)value[0]);
            }

            // GPSImgDirectionRef
            if (tag == 16)
            {
                return new ExifEnumProperty<GPSDirectionRef>(ExifTag.GPSImgDirectionRef, (GPSDirectionRef)value[0]);
            }

            // GPSDestLatitudeRef
            if (tag == 19)
            {
                return new ExifEnumProperty<GPSLatitudeRef>(ExifTag.GPSDestLatitudeRef, (GPSLatitudeRef)value[0]);
            }

            // GPSDestLatitude
            if (tag == 20)
            {
                return new GPSLatitudeLongitude(
                    ExifTag.GPSDestLatitude,
                    ExifBitConverter.ToURationalArray(value, (int)count, byteOrder));
            }

            // GPSDestLongitudeRef
            if (tag == 21)
            {
                return new ExifEnumProperty<GPSLongitudeRef>(ExifTag.GPSDestLongitudeRef, (GPSLongitudeRef)value[0]);
            }

            // GPSDestLongitude
            if (tag == 22)
            {
                return new GPSLatitudeLongitude(
                    ExifTag.GPSDestLongitude,
                    ExifBitConverter.ToURationalArray(value, (int)count, byteOrder));
            }

            // GPSDestBearingRef
            if (tag == 23)
            {
                return new ExifEnumProperty<GPSDirectionRef>(ExifTag.GPSDestBearingRef, (GPSDirectionRef)value[0]);
            }

            // GPSDestDistanceRef
            if (tag == 25)
            {
                return new ExifEnumProperty<GPSDistanceRef>(ExifTag.GPSDestDistanceRef, (GPSDistanceRef)value[0]);
            }

            // GPSDate
            if (tag == 29)
            {
                return new ExifDateTime(ExifTag.GPSDateStamp, ExifBitConverter.ToDateTime(value, false));
            }

            // GPSDifferential
            if (tag == 30)
            {
                return new ExifEnumProperty<GPSDifferential>(
                    ExifTag.GPSDifferential,
                    (GPSDifferential)conv.ToUInt16(value, 0));
            }
        }
        else if (ifd == IFD.Interop)
        {
            // InteroperabilityIndex
            if (tag == 1)
            {
                return new ExifAscii(ExifTag.InteroperabilityIndex, ExifBitConverter.ToAscii(value, Encoding.ASCII), Encoding.ASCII);
            }

            // InteroperabilityVersion
            if (tag == 2)
            {
                return new ExifVersion(
                    ExifTag.InteroperabilityVersion,
                    ExifBitConverter.ToAscii(value, Encoding.ASCII));
            }
        }
        else if (ifd == IFD.First)
        {
            // Compression
            if (tag == 0x103)
            {
                return new ExifEnumProperty<Compression>(
                    ExifTag.ThumbnailCompression,
                    (Compression)conv.ToUInt16(value, 0));
            }

            // PhotometricInterpretation
            if (tag == 0x106)
            {
                return new ExifEnumProperty<PhotometricInterpretation>(
                    ExifTag.ThumbnailPhotometricInterpretation,
                    (PhotometricInterpretation)conv.ToUInt16(value, 0));
            }

            // Orientation
            if (tag == 0x112)
            {
                return new ExifEnumProperty<Orientation>(
                    ExifTag.ThumbnailOrientation,
                    (Orientation)conv.ToUInt16(value, 0));
            }

            // PlanarConfiguration
            if (tag == 0x11c)
            {
                return new ExifEnumProperty<PlanarConfiguration>(
                    ExifTag.ThumbnailPlanarConfiguration,
                    (PlanarConfiguration)conv.ToUInt16(value, 0));
            }

            // YCbCrPositioning
            if (tag == 0x213)
            {
                return new ExifEnumProperty<YCbCrPositioning>(
                    ExifTag.ThumbnailYCbCrPositioning,
                    (YCbCrPositioning)conv.ToUInt16(value, 0));
            }

            // ResolutionUnit
            if (tag == 0x128)
            {
                return new ExifEnumProperty<ResolutionUnit>(
                    ExifTag.ThumbnailResolutionUnit,
                    (ResolutionUnit)conv.ToUInt16(value, 0));
            }

            // DateTime
            if (tag == 0x132)
            {
                return new ExifDateTime(ExifTag.ThumbnailDateTime, ExifBitConverter.ToDateTime(value));
            }
        }

        // 1 = BYTE An 8-bit unsigned integer.
        if (type == 1)
        {
            if (count == 1)
            {
                return new ExifByte(etag, value[0]);
            }

            return new ExifByteArray(etag, value);
        }

        // 2 = ASCII An 8-bit byte containing one 7-bit ASCII code.
        if (type == 2)
        {
            return new ExifAscii(etag, ExifBitConverter.ToAscii(value, encoding), encoding);
        }

        // 3 = SHORT A 16-bit (2-byte) unsigned integer.
        if (type == 3)
        {
            if (count == 1)
            {
                return new ExifUShort(etag, conv.ToUInt16(value, 0));
            }

            return new ExifUShortArray(etag, ExifBitConverter.ToUShortArray(value, (int)count, byteOrder));
        }

        // 4 = LONG A 32-bit (4-byte) unsigned integer.
        if (type == 4)
        {
            if (count == 1)
            {
                return new ExifUInt(etag, conv.ToUInt32(value, 0));
            }

            return new ExifUIntArray(etag, ExifBitConverter.ToUIntArray(value, (int)count, byteOrder));
        }

        // 5 = RATIONAL Two LONGs. The first LONG is the numerator and the second LONG expresses the denominator.
        if (type == 5)
        {
            if (count == 1)
            {
                return new ExifURational(etag, ExifBitConverter.ToURational(value, byteOrder));
            }

            return new ExifURationalArray(etag, ExifBitConverter.ToURationalArray(value, (int)count, byteOrder));
        }

        // 7 = UNDEFINED An 8-bit byte that can take any value depending on the field definition.
        if (type == 7)
        {
            return new ExifUndefined(etag, value);
        }

        // 9 = SLONG A 32-bit (4-byte) signed integer (2's complement notation).
        if (type == 9)
        {
            if (count == 1)
            {
                return new ExifSInt(etag, conv.ToInt32(value, 0));
            }

            return new ExifSIntArray(etag, ExifBitConverter.ToSIntArray(value, (int)count, byteOrder));
        }

        // 10 = SRATIONAL Two SLONGs. The first SLONG is the numerator and the second SLONG is the denominator.
        if (type == 10)
        {
            if (count == 1)
            {
                return new ExifSRational(etag, ExifBitConverter.ToSRational(value, byteOrder));
            }

            return new ExifSRationalArray(etag, ExifBitConverter.ToSRationalArray(value, (int)count, byteOrder));
        }

        throw new ArgumentException("Unknown property type.");
    }

    #endregion
}
