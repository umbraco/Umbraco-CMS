namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Represents a JPEG marker byte.
/// </summary>
internal enum JPEGMarker : byte
{
    // Start Of Frame markers, non-differential, Huffman coding
    SOF0 = 0xc0,
    SOF1 = 0xc1,
    SOF2 = 0xc2,
    SOF3 = 0xc3,

    // Start Of Frame markers, differential, Huffman coding
    SOF5 = 0xc5,
    SOF6 = 0xc6,
    SOF7 = 0xc7,

    // Start Of Frame markers, non-differential, arithmetic coding
    JPG = 0xc8,
    SOF9 = 0xc9,
    SOF10 = 0xca,
    SOF11 = 0xcb,

    // Start Of Frame markers, differential, arithmetic coding
    SOF13 = 0xcd,
    SOF14 = 0xce,
    SOF15 = 0xcf,

    // Huffman table specification
    DHT = 0xc4,

    // Arithmetic coding conditioning specification
    DAC = 0xcc,

    // Restart interval termination
    RST0 = 0xd0,
    RST1 = 0xd1,
    RST2 = 0xd2,
    RST3 = 0xd3,
    RST4 = 0xd4,
    RST5 = 0xd5,
    RST6 = 0xd6,
    RST7 = 0xd7,

    // Other markers
    SOI = 0xd8,
    EOI = 0xd9,
    SOS = 0xda,
    DQT = 0xdb,
    DNL = 0xdc,
    DRI = 0xdd,
    DHP = 0xde,
    EXP = 0xdf,

    // application segments
    APP0 = 0xe0,
    APP1 = 0xe1,
    APP2 = 0xe2,
    APP3 = 0xe3,
    APP4 = 0xe4,
    APP5 = 0xe5,
    APP6 = 0xe6,
    APP7 = 0xe7,
    APP8 = 0xe8,
    APP9 = 0xe9,
    APP10 = 0xea,
    APP11 = 0xeb,
    APP12 = 0xec,
    APP13 = 0xed,
    APP14 = 0xee,
    APP15 = 0xef,

    // JPEG extensions
    JPG0 = 0xf0,
    JPG1 = 0xf1,
    JPG2 = 0xf2,
    JPG3 = 0xf3,
    JPG4 = 0xf4,
    JPG5 = 0xf5,
    JPG6 = 0xf6,
    JPG7 = 0xf7,
    JPG8 = 0xf8,
    JPG9 = 0xf9,
    JPG10 = 0xfa,
    JPG11 = 0xfb,
    JP1G2 = 0xfc,
    JPG13 = 0xfd,

    // Comment
    COM = 0xfe,

    // Temporary
    TEM = 0x01,
}
