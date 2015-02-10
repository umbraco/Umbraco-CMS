using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Media.Exif
{
    internal static class JpegId
    {
        public const int START = 0xFF;
        public const int SOI = 0xD8;
        public const int SOS = 0xDA;
        public const int EOI = 0xD9;
        public const int COM = 0xFE;
        public const int JFIF = 0xE0;
        public const int EXIF = 0xE1;
        public const int IPTC = 0xED;
    }

    internal enum ExifIFD
    {
        Exif = 0x8769,
        Gps = 0x8825
    }

    internal enum ExifId
    {
        Unknown = -1,

        ImageWidth = 0x100,
        ImageHeight = 0x101,
        Orientation = 0x112,
        XResolution = 0x11A,
        YResolution = 0x11B,
        ResolutionUnit = 0x128,
        DateTime = 0x132,
        Description = 0x10E,
        Make = 0x10F,
        Model = 0x110,
        Software = 0x131,
        Artist = 0x13B,
        ThumbnailOffset = 0x201,
        ThumbnailLength = 0x202,
        ExposureTime = 0x829A,
        FNumber = 0x829D,
        Copyright = 0x8298,
        DateTimeOriginal = 0x9003,
        FlashUsed = 0x9209,
        UserComment = 0x9286
    }

    internal enum ExifGps
    {
        Version = 0x0,
        LatitudeRef = 0x1,
        Latitude = 0x2,
        LongitudeRef = 0x3,
        Longitude = 0x4,
        AltitudeRef = 0x5,
        Altitude = 0x6,
        TimeStamp = 0x7,
        Satellites = 0x8,
        Status = 0x9,
        MeasureMode = 0xA,
        DOP = 0xB,
        SpeedRef = 0xC,
        Speed = 0xD,
        TrackRef = 0xE,
        Track = 0xF,
        ImgDirectionRef = 0x10,
        ImgDirection = 0x11,
        MapDatum = 0x12,
        DestLatitudeRef = 0x13,
        DestLatitude = 0x14,
        DestLongitudeRef = 0x15,
        DestLongitude = 0x16,
        DestBearingRef = 0x17,
        DestBearing = 0x18,
        DestDistanceRef = 0x19,
        DestDistance = 0x1A,
        ProcessingMethod = 0x1B,
        AreaInformation = 0x1C,
        DateStamp = 0x1D,
        Differential = 0x1E
    }

    internal enum ExifOrientation
    {
        TopLeft = 1,
        BottomRight = 3,
        TopRight = 6,
        BottomLeft = 8,
        Undefined = 9
    }

    internal enum ExifUnit
    {
        Undefined = 1,
        Inch = 2,
        Centimeter = 3
    }

    /// <summary>
    /// As per http://www.exif.org/Exif2-2.PDF
    /// </summary>
    [Flags]
    internal enum ExifFlash
    {
        No = 0x0,
        Fired = 0x1,
        StrobeReturnLightDetected = 0x6,
        On = 0x8,
        Off = 0x10,
        Auto = 0x18,
        FlashFunctionPresent = 0x20,
        RedEyeReduction = 0x40
    }

    internal enum ExifGpsLatitudeRef
    {
        Unknown = 0,
        North,
        South
    }

    internal enum ExifGpsLongitudeRef
    {
        Unknown = 0,
        East,
        West
    }
}
