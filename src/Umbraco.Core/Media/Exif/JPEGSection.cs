namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Represents the memory view of a JPEG section.
///     A JPEG section is the data between markers of the JPEG file.
/// </summary>
internal class JPEGSection
{
    #region Instance Methods

    /// <summary>
    ///     Returns a string representation of the current section.
    /// </summary>
    /// <returns>A System.String that represents the current section.</returns>
    public override string ToString() => string.Format("{0} => Header: {1} bytes, Entropy Data: {2} bytes", Marker, Header.Length, EntropyData.Length);

    #endregion

    #region Properties

    /// <summary>
    ///     The marker byte representing the section.
    /// </summary>
    public JPEGMarker Marker { get; }

    /// <summary>
    ///     Section header as a byte array. This is different from the header
    ///     definition in JPEG specification in that it does not include the
    ///     two byte section length.
    /// </summary>
    public byte[] Header { get; set; }

    /// <summary>
    ///     For the SOS and RST markers, this contains the entropy coded data.
    /// </summary>
    public byte[] EntropyData { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    ///     Constructs a JPEGSection represented by the marker byte and containing
    ///     the given data.
    /// </summary>
    /// <param name="marker">The marker byte representing the section.</param>
    /// <param name="data">Section data.</param>
    /// <param name="entropydata">Entropy coded data.</param>
    public JPEGSection(JPEGMarker marker, byte[] data, byte[] entropydata)
    {
        Marker = marker;
        Header = data;
        EntropyData = entropydata;
    }

    /// <summary>
    ///     Constructs a JPEGSection represented by the marker byte.
    /// </summary>
    /// <param name="marker">The marker byte representing the section.</param>
    public JPEGSection(JPEGMarker marker)
        : this(marker, new byte[0], new byte[0])
    {
    }

    #endregion
}
