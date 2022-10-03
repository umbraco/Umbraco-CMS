using System.ComponentModel;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core;

/// <summary>
///     Represents a guid-based entity identifier.
/// </summary>
[TypeConverter(typeof(UdiTypeConverter))]
public class GuidUdi : Udi
{
    /// <summary>
    ///     Initializes a new instance of the GuidUdi class with an entity type and a guid.
    /// </summary>
    /// <param name="entityType">The entity type part of the udi.</param>
    /// <param name="guid">The guid part of the udi.</param>
    public GuidUdi(string entityType, Guid guid)
        : base(entityType, CreateStringValue(entityType, guid)) =>
        Guid = guid;

    /// <summary>
    ///     Initializes a new instance of the GuidUdi class with an uri value.
    /// </summary>
    /// <param name="uriValue">The uri value of the udi.</param>
    public GuidUdi(Uri uriValue)
        : base(uriValue)
    {
        if (Guid.TryParse(uriValue.AbsolutePath.TrimStart(Constants.CharArrays.ForwardSlash), out Guid guid) == false)
        {
            throw new FormatException("URI \"" + uriValue + "\" is not a GUID entity ID.");
        }

        Guid = guid;
    }

    /// <summary>
    ///     The guid part of the identifier.
    /// </summary>
    public Guid Guid { get; }

    /// <inheritdoc />
    public override bool IsRoot => Guid == Guid.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is not GuidUdi other)
        {
            return false;
        }

        return EntityType == other.EntityType && Guid == other.Guid;
    }

    public override int GetHashCode() => base.GetHashCode();

    public GuidUdi EnsureClosed()
    {
        EnsureNotRoot();
        return this;
    }

    private static string CreateStringValue(ReadOnlySpan<char> entityType, Guid guid)
    {
        var startUdiLength = Constants.Conventions.Udi.Prefix.Length;
        var outputSize = entityType.Length + startUdiLength + 32 + 1; //Based on the format umb://entityType/guid (32 = Guid N format, 1 = / between entityType and guid)
        Span<char> output = stackalloc char[outputSize];

        //Add all the values of the format to the output
        Constants.Conventions.Udi.Prefix.CopyTo(output[..startUdiLength]);
        entityType.CopyTo(output.Slice(startUdiLength, entityType.Length));
        output[startUdiLength + entityType.Length] = '/';
        guid.TryFormat(output.Slice(outputSize - 32, 32), out _, "N");

        return new string(output);
    }
}
