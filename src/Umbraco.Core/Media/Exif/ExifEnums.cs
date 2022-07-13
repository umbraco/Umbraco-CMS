namespace Umbraco.Cms.Core.Media.Exif;

internal enum Compression : ushort
{
    Uncompressed = 1,
    CCITT1D = 2,
    Group3Fax = 3,
    Group4Fax = 4,
    LZW = 5,
    JPEG = 6,
    PackBits = 32773,
}

internal enum PhotometricInterpretation : ushort
{
    WhiteIsZero = 0,
    BlackIsZero = 1,
    RGB = 2,
    RGBPalette = 3,
    TransparencyMask = 4,
    CMYK = 5,
    YCbCr = 6,
    CIELab = 8,
}

internal enum Orientation : ushort
{
    Normal = 1,
    MirroredVertically = 2,
    Rotated180 = 3,
    MirroredHorizontally = 4,
    RotatedLeftAndMirroredVertically = 5,
    RotatedRight = 6,
    RotatedLeft = 7,
    RotatedRightAndMirroredVertically = 8,
}

internal enum PlanarConfiguration : ushort
{
    ChunkyFormat = 1,
    PlanarFormat = 2,
}

internal enum YCbCrPositioning : ushort
{
    Centered = 1,
    CoSited = 2,
}

internal enum ResolutionUnit : ushort
{
    Inches = 2,
    Centimeters = 3,
}

internal enum ColorSpace : ushort
{
    SRGB = 1,
    Uncalibrated = 0xfff,
}

internal enum ExposureProgram : ushort
{
    NotDefined = 0,
    Manual = 1,
    Normal = 2,
    AperturePriority = 3,
    ShutterPriority = 4,

    /// <summary>
    ///     Biased toward depth of field.
    /// </summary>
    Creative = 5,

    /// <summary>
    ///     Biased toward fast shutter speed.
    /// </summary>
    Action = 6,

    /// <summary>
    ///     For closeup photos with the background out of focus.
    /// </summary>
    Portrait = 7,

    /// <summary>
    ///     For landscape photos with the background in focus.
    /// </summary>
    Landscape = 8,
}

internal enum MeteringMode : ushort
{
    Unknown = 0,
    Average = 1,
    CenterWeightedAverage = 2,
    Spot = 3,
    MultiSpot = 4,
    Pattern = 5,
    Partial = 6,
    Other = 255,
}

internal enum LightSource : ushort
{
    Unknown = 0,
    Daylight = 1,
    Fluorescent = 2,
    Tungsten = 3,
    Flash = 4,
    FineWeather = 9,
    CloudyWeather = 10,
    Shade = 11,

    /// <summary>
    ///     D 5700 – 7100K
    /// </summary>
    DaylightFluorescent = 12,

    /// <summary>
    ///     N 4600 – 5400K
    /// </summary>
    DayWhiteFluorescent = 13,

    /// <summary>
    ///     W 3900 – 4500K
    /// </summary>
    CoolWhiteFluorescent = 14,

    /// <summary>
    ///     WW 3200 – 3700K
    /// </summary>
    WhiteFluorescent = 15,
    StandardLightA = 17,
    StandardLightB = 18,
    StandardLightC = 19,
    D55 = 20,
    D65 = 21,
    D75 = 22,
    D50 = 23,
    ISOStudioTungsten = 24,
    OtherLightSource = 255,
}

[Flags]
internal enum Flash : ushort
{
    FlashDidNotFire = 0,
    StrobeReturnLightNotDetected = 4,
    StrobeReturnLightDetected = 2,
    FlashFired = 1,
    CompulsoryFlashMode = 8,
    AutoMode = 16,
    NoFlashFunction = 32,
    RedEyeReductionMode = 64,
}

internal enum SensingMethod : ushort
{
    NotDefined = 1,
    OneChipColorAreaSensor = 2,
    TwoChipColorAreaSensor = 3,
    ThreeChipColorAreaSensor = 4,
    ColorSequentialAreaSensor = 5,
    TriLinearSensor = 7,
    ColorSequentialLinearSensor = 8,
}

internal enum FileSource : byte // UNDEFINED
{
    DSC = 3,
}

internal enum SceneType : byte // UNDEFINED
{
    DirectlyPhotographedImage = 1,
}

internal enum CustomRendered : ushort
{
    NormalProcess = 0,
    CustomProcess = 1,
}

internal enum ExposureMode : ushort
{
    Auto = 0,
    Manual = 1,
    AutoBracket = 2,
}

internal enum WhiteBalance : ushort
{
    Auto = 0,
    Manual = 1,
}

internal enum SceneCaptureType : ushort
{
    Standard = 0,
    Landscape = 1,
    Portrait = 2,
    NightScene = 3,
}

internal enum GainControl : ushort
{
    None = 0,
    LowGainUp = 1,
    HighGainUp = 2,
    LowGainDown = 3,
    HighGainDown = 4,
}

internal enum Contrast : ushort
{
    Normal = 0,
    Soft = 1,
    Hard = 2,
}

internal enum Saturation : ushort
{
    Normal = 0,
    Low = 1,
    High = 2,
}

internal enum Sharpness : ushort
{
    Normal = 0,
    Soft = 1,
    Hard = 2,
}

internal enum SubjectDistanceRange : ushort
{
    Unknown = 0,
    Macro = 1,
    CloseView = 2,
    DistantView = 3,
}

internal enum GPSLatitudeRef : byte // ASCII
{
    North = 78, // 'N'
    South = 83, // 'S'
}

internal enum GPSLongitudeRef : byte // ASCII
{
    West = 87, // 'W'
    East = 69, // 'E'
}

internal enum GPSAltitudeRef : byte
{
    AboveSeaLevel = 0,
    BelowSeaLevel = 1,
}

internal enum GPSStatus : byte // ASCII
{
    MeasurementInProgress = 65, // 'A'
    MeasurementInteroperability = 86, // 'V'
}

internal enum GPSMeasureMode : byte // ASCII
{
    TwoDimensional = 50, // '2'
    ThreeDimensional = 51, // '3'
}

internal enum GPSSpeedRef : byte // ASCII
{
    KilometersPerHour = 75, // 'K'
    MilesPerHour = 77, // 'M'
    Knots = 78, // 'N'
}

internal enum GPSDirectionRef : byte // ASCII
{
    TrueDirection = 84, // 'T'
    MagneticDirection = 77, // 'M'
}

internal enum GPSDistanceRef : byte // ASCII
{
    Kilometers = 75, // 'K'
    Miles = 77, // 'M'
    Knots = 78, // 'N'
}

internal enum GPSDifferential : ushort
{
    MeasurementWithoutDifferentialCorrection = 0,
    DifferentialCorrectionApplied = 1,
}
