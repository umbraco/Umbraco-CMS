namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Represents the tags associated with exif fields.
/// </summary>
internal enum ExifTag
{
    // ****************************
    // Zeroth IFD
    // ****************************
    NewSubfileType = IFD.Zeroth + 254,
    SubfileType = IFD.Zeroth + 255,
    ImageWidth = IFD.Zeroth + 256,
    ImageLength = IFD.Zeroth + 257,
    BitsPerSample = IFD.Zeroth + 258,
    Compression = IFD.Zeroth + 259,
    PhotometricInterpretation = IFD.Zeroth + 262,
    Threshholding = IFD.Zeroth + 263,
    CellWidth = IFD.Zeroth + 264,
    CellLength = IFD.Zeroth + 265,
    FillOrder = IFD.Zeroth + 266,
    DocumentName = IFD.Zeroth + 269,
    ImageDescription = IFD.Zeroth + 270,
    Make = IFD.Zeroth + 271,
    Model = IFD.Zeroth + 272,
    StripOffsets = IFD.Zeroth + 273,
    Orientation = IFD.Zeroth + 274,
    SamplesPerPixel = IFD.Zeroth + 277,
    RowsPerStrip = IFD.Zeroth + 278,
    StripByteCounts = IFD.Zeroth + 279,
    MinSampleValue = IFD.Zeroth + 280,
    MaxSampleValue = IFD.Zeroth + 281,
    XResolution = IFD.Zeroth + 282,
    YResolution = IFD.Zeroth + 283,
    PlanarConfiguration = IFD.Zeroth + 284,
    PageName = IFD.Zeroth + 285,
    XPosition = IFD.Zeroth + 286,
    YPosition = IFD.Zeroth + 287,
    FreeOffsets = IFD.Zeroth + 288,
    FreeByteCounts = IFD.Zeroth + 289,
    GrayResponseUnit = IFD.Zeroth + 290,
    GrayResponseCurve = IFD.Zeroth + 291,
    T4Options = IFD.Zeroth + 292,
    T6Options = IFD.Zeroth + 293,
    ResolutionUnit = IFD.Zeroth + 296,
    PageNumber = IFD.Zeroth + 297,
    TransferFunction = IFD.Zeroth + 301,
    Software = IFD.Zeroth + 305,
    DateTime = IFD.Zeroth + 306,
    Artist = IFD.Zeroth + 315,
    HostComputer = IFD.Zeroth + 316,
    Predictor = IFD.Zeroth + 317,
    WhitePoint = IFD.Zeroth + 318,
    PrimaryChromaticities = IFD.Zeroth + 319,
    ColorMap = IFD.Zeroth + 320,
    HalftoneHints = IFD.Zeroth + 321,
    TileWidth = IFD.Zeroth + 322,
    TileLength = IFD.Zeroth + 323,
    TileOffsets = IFD.Zeroth + 324,
    TileByteCounts = IFD.Zeroth + 325,
    InkSet = IFD.Zeroth + 332,
    InkNames = IFD.Zeroth + 333,
    NumberOfInks = IFD.Zeroth + 334,
    DotRange = IFD.Zeroth + 336,
    TargetPrinter = IFD.Zeroth + 337,
    ExtraSamples = IFD.Zeroth + 338,
    SampleFormat = IFD.Zeroth + 339,
    SMinSampleValue = IFD.Zeroth + 340,
    SMaxSampleValue = IFD.Zeroth + 341,
    TransferRange = IFD.Zeroth + 342,
    JPEGProc = IFD.Zeroth + 512,
    JPEGInterchangeFormat = IFD.Zeroth + 513,
    JPEGInterchangeFormatLength = IFD.Zeroth + 514,
    JPEGRestartInterval = IFD.Zeroth + 515,
    JPEGLosslessPredictors = IFD.Zeroth + 517,
    JPEGPointTransforms = IFD.Zeroth + 518,
    JPEGQTables = IFD.Zeroth + 519,
    JPEGDCTables = IFD.Zeroth + 520,
    JPEGACTables = IFD.Zeroth + 521,
    YCbCrCoefficients = IFD.Zeroth + 529,
    YCbCrSubSampling = IFD.Zeroth + 530,
    YCbCrPositioning = IFD.Zeroth + 531,
    ReferenceBlackWhite = IFD.Zeroth + 532,
    Copyright = IFD.Zeroth + 33432,

    // Pointers to other IFDs
    EXIFIFDPointer = IFD.Zeroth + 34665,
    GPSIFDPointer = IFD.Zeroth + 34853,

    // Windows Tags
    WindowsTitle = IFD.Zeroth + 0x9c9b,
    WindowsComment = IFD.Zeroth + 0x9c9c,
    WindowsAuthor = IFD.Zeroth + 0x9c9d,
    WindowsKeywords = IFD.Zeroth + 0x9c9e,
    WindowsSubject = IFD.Zeroth + 0x9c9f,

    // Rating
    Rating = IFD.Zeroth + 0x4746,
    RatingPercent = IFD.Zeroth + 0x4749,

    // Microsoft specifying padding and offset tags
    ZerothIFDPadding = IFD.Zeroth + 0xea1c,

    // ****************************
    // EXIF Tags
    // ****************************
    ExifVersion = IFD.EXIF + 36864,
    FlashpixVersion = IFD.EXIF + 40960,
    ColorSpace = IFD.EXIF + 40961,
    ComponentsConfiguration = IFD.EXIF + 37121,
    CompressedBitsPerPixel = IFD.EXIF + 37122,
    PixelXDimension = IFD.EXIF + 40962,
    PixelYDimension = IFD.EXIF + 40963,
    MakerNote = IFD.EXIF + 37500,
    UserComment = IFD.EXIF + 37510,
    RelatedSoundFile = IFD.EXIF + 40964,
    DateTimeOriginal = IFD.EXIF + 36867,
    DateTimeDigitized = IFD.EXIF + 36868,
    SubSecTime = IFD.EXIF + 37520,
    SubSecTimeOriginal = IFD.EXIF + 37521,
    SubSecTimeDigitized = IFD.EXIF + 37522,
    ExposureTime = IFD.EXIF + 33434,
    FNumber = IFD.EXIF + 33437,
    ExposureProgram = IFD.EXIF + 34850,
    SpectralSensitivity = IFD.EXIF + 34852,
    ISOSpeedRatings = IFD.EXIF + 34855,
    OECF = IFD.EXIF + 34856,
    ShutterSpeedValue = IFD.EXIF + 37377,
    ApertureValue = IFD.EXIF + 37378,
    BrightnessValue = IFD.EXIF + 37379,
    ExposureBiasValue = IFD.EXIF + 37380,
    MaxApertureValue = IFD.EXIF + 37381,
    SubjectDistance = IFD.EXIF + 37382,
    MeteringMode = IFD.EXIF + 37383,
    LightSource = IFD.EXIF + 37384,
    Flash = IFD.EXIF + 37385,
    FocalLength = IFD.EXIF + 37386,
    SubjectArea = IFD.EXIF + 37396,
    FlashEnergy = IFD.EXIF + 41483,
    SpatialFrequencyResponse = IFD.EXIF + 41484,
    FocalPlaneXResolution = IFD.EXIF + 41486,
    FocalPlaneYResolution = IFD.EXIF + 41487,
    FocalPlaneResolutionUnit = IFD.EXIF + 41488,
    SubjectLocation = IFD.EXIF + 41492,
    ExposureIndex = IFD.EXIF + 41493,
    SensingMethod = IFD.EXIF + 41495,
    FileSource = IFD.EXIF + 41728,
    SceneType = IFD.EXIF + 41729,
    CFAPattern = IFD.EXIF + 41730,
    CustomRendered = IFD.EXIF + 41985,
    ExposureMode = IFD.EXIF + 41986,
    WhiteBalance = IFD.EXIF + 41987,
    DigitalZoomRatio = IFD.EXIF + 41988,
    FocalLengthIn35mmFilm = IFD.EXIF + 41989,
    SceneCaptureType = IFD.EXIF + 41990,
    GainControl = IFD.EXIF + 41991,
    Contrast = IFD.EXIF + 41992,
    Saturation = IFD.EXIF + 41993,
    Sharpness = IFD.EXIF + 41994,
    DeviceSettingDescription = IFD.EXIF + 41995,
    SubjectDistanceRange = IFD.EXIF + 41996,
    ImageUniqueID = IFD.EXIF + 42016,
    InteroperabilityIFDPointer = IFD.EXIF + 40965,

    // Microsoft specifying padding and offset tags
    ExifIFDPadding = IFD.EXIF + 0xea1c,
    OffsetSchema = IFD.EXIF + 0xea1d,

    // ****************************
    // GPS Tags
    // ****************************
    GPSVersionID = IFD.GPS + 0,
    GPSLatitudeRef = IFD.GPS + 1,
    GPSLatitude = IFD.GPS + 2,
    GPSLongitudeRef = IFD.GPS + 3,
    GPSLongitude = IFD.GPS + 4,
    GPSAltitudeRef = IFD.GPS + 5,
    GPSAltitude = IFD.GPS + 6,
    GPSTimeStamp = IFD.GPS + 7,
    GPSSatellites = IFD.GPS + 8,
    GPSStatus = IFD.GPS + 9,
    GPSMeasureMode = IFD.GPS + 10,
    GPSDOP = IFD.GPS + 11,
    GPSSpeedRef = IFD.GPS + 12,
    GPSSpeed = IFD.GPS + 13,
    GPSTrackRef = IFD.GPS + 14,
    GPSTrack = IFD.GPS + 15,
    GPSImgDirectionRef = IFD.GPS + 16,
    GPSImgDirection = IFD.GPS + 17,
    GPSMapDatum = IFD.GPS + 18,
    GPSDestLatitudeRef = IFD.GPS + 19,
    GPSDestLatitude = IFD.GPS + 20,
    GPSDestLongitudeRef = IFD.GPS + 21,
    GPSDestLongitude = IFD.GPS + 22,
    GPSDestBearingRef = IFD.GPS + 23,
    GPSDestBearing = IFD.GPS + 24,
    GPSDestDistanceRef = IFD.GPS + 25,
    GPSDestDistance = IFD.GPS + 26,
    GPSProcessingMethod = IFD.GPS + 27,
    GPSAreaInformation = IFD.GPS + 28,
    GPSDateStamp = IFD.GPS + 29,
    GPSDifferential = IFD.GPS + 30,

    // ****************************
    // InterOp Tags
    // ****************************
    InteroperabilityIndex = IFD.Interop + 1,
    InteroperabilityVersion = IFD.Interop + 2,
    RelatedImageWidth = IFD.Interop + 0x1001,
    RelatedImageHeight = IFD.Interop + 0x1002,

    // ****************************
    // First IFD TIFF Tags
    // ****************************
    ThumbnailImageWidth = IFD.First + 256,
    ThumbnailImageLength = IFD.First + 257,
    ThumbnailBitsPerSample = IFD.First + 258,
    ThumbnailCompression = IFD.First + 259,
    ThumbnailPhotometricInterpretation = IFD.First + 262,
    ThumbnailOrientation = IFD.First + 274,
    ThumbnailSamplesPerPixel = IFD.First + 277,
    ThumbnailPlanarConfiguration = IFD.First + 284,
    ThumbnailYCbCrSubSampling = IFD.First + 530,
    ThumbnailYCbCrPositioning = IFD.First + 531,
    ThumbnailXResolution = IFD.First + 282,
    ThumbnailYResolution = IFD.First + 283,
    ThumbnailResolutionUnit = IFD.First + 296,
    ThumbnailStripOffsets = IFD.First + 273,
    ThumbnailRowsPerStrip = IFD.First + 278,
    ThumbnailStripByteCounts = IFD.First + 279,
    ThumbnailJPEGInterchangeFormat = IFD.First + 513,
    ThumbnailJPEGInterchangeFormatLength = IFD.First + 514,
    ThumbnailTransferFunction = IFD.First + 301,
    ThumbnailWhitePoint = IFD.First + 318,
    ThumbnailPrimaryChromaticities = IFD.First + 319,
    ThumbnailYCbCrCoefficients = IFD.First + 529,
    ThumbnailReferenceBlackWhite = IFD.First + 532,
    ThumbnailDateTime = IFD.First + 306,
    ThumbnailImageDescription = IFD.First + 270,
    ThumbnailMake = IFD.First + 271,
    ThumbnailModel = IFD.First + 272,
    ThumbnailSoftware = IFD.First + 305,
    ThumbnailArtist = IFD.First + 315,
    ThumbnailCopyright = IFD.First + 33432,

    // ****************************
    // JFIF Tags
    // ****************************

    /// <summary>
    ///     Represents the JFIF version.
    /// </summary>
    JFIFVersion = IFD.JFIF + 1,

    /// <summary>
    ///     Represents units for X and Y densities.
    /// </summary>
    JFIFUnits = IFD.JFIF + 101,

    /// <summary>
    ///     Horizontal pixel density.
    /// </summary>
    XDensity = IFD.JFIF + 102,

    /// <summary>
    ///     Vertical pixel density
    /// </summary>
    YDensity = IFD.JFIF + 103,

    /// <summary>
    ///     Thumbnail horizontal pixel count.
    /// </summary>
    JFIFXThumbnail = IFD.JFIF + 201,

    /// <summary>
    ///     Thumbnail vertical pixel count.
    /// </summary>
    JFIFYThumbnail = IFD.JFIF + 202,

    /// <summary>
    ///     JFIF JPEG thumbnail.
    /// </summary>
    JFIFThumbnail = IFD.JFIF + 203,

    /// <summary>
    ///     Code which identifies the JFIF extension.
    /// </summary>
    JFXXExtensionCode = IFD.JFXX + 1,

    /// <summary>
    ///     Thumbnail horizontal pixel count.
    /// </summary>
    JFXXXThumbnail = IFD.JFXX + 101,

    /// <summary>
    ///     Thumbnail vertical pixel count.
    /// </summary>
    JFXXYThumbnail = IFD.JFXX + 102,

    /// <summary>
    ///     The 256-Color RGB palette.
    /// </summary>
    JFXXPalette = IFD.JFXX + 201,

    /// <summary>
    ///     JFIF thumbnail. The thumbnail will be either a JPEG,
    ///     a 256 color palette bitmap, or a 24-bit RGB bitmap.
    /// </summary>
    JFXXThumbnail = IFD.JFXX + 202,
}
