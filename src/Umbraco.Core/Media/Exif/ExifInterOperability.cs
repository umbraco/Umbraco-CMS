namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Represents interoperability data for an exif tag in the platform byte order.
/// </summary>
internal struct ExifInterOperability
{
    public ExifInterOperability(ushort tagid, ushort typeid, uint count, byte[] data)
    {
        TagID = tagid;
        TypeID = typeid;
        Count = count;
        Data = data;
    }

    /// <summary>
    ///     Gets the tag ID defined in the Exif standard.
    /// </summary>
    public ushort TagID { get; }

    /// <summary>
    ///     Gets the type code defined in the Exif standard.
    ///     <list type="bullet">
    ///         <item>1 = BYTE (byte)</item>
    ///         <item>2 = ASCII (byte array)</item>
    ///         <item>3 = SHORT (ushort)</item>
    ///         <item>4 = LONG (uint)</item>
    ///         <item>5 = RATIONAL (2 x uint: numerator, denominator)</item>
    ///         <item>6 = BYTE (sbyte)</item>
    ///         <item>7 = UNDEFINED (byte array)</item>
    ///         <item>8 = SSHORT (short)</item>
    ///         <item>9 = SLONG (int)</item>
    ///         <item>10 = SRATIONAL (2 x int: numerator, denominator)</item>
    ///         <item>11 = FLOAT (float)</item>
    ///         <item>12 = DOUBLE (double)</item>
    ///     </list>
    /// </summary>
    public ushort TypeID { get; }

    /// <summary>
    ///     Gets the byte count or number of components.
    /// </summary>
    public uint Count { get; }

    /// <summary>
    ///     Gets the field value as an array of bytes.
    /// </summary>
    public byte[] Data { get; }

    /// <summary>
    ///     Returns the string representation of this instance.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => string.Format("Tag: {0}, Type: {1}, Count: {2}, Data Length: {3}", TagID, TypeID, Count, Data.Length);
}
