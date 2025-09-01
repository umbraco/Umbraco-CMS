using System.Runtime.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Represents infos for a URL.
/// </summary>
[DataContract(Name = "urlInfo", Namespace = "")]
public class UrlInfo : IEquatable<UrlInfo>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UrlInfo" /> class.
    /// </summary>
    public UrlInfo(Uri url, string provider, string? culture, string? message = null, bool isExternal = false)
    {
        if (provider.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(provider));
        }

        Url = url;
        Provider = provider;
        Culture = culture;
        Message = message;
        IsExternal = isExternal;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UrlInfo" /> class as a "message only" - that is, not an actual URL.
    /// </summary>
    public UrlInfo(string message, string provider, string? culture = null)
    {
        if (message.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));
        }

        if (provider.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(provider));
        }

        Url = null;
        Provider = provider;
        Message = message;
        Culture = culture;
    }

    public static UrlInfo AsUrl(string url, string provider, string? culture = null, bool isExternal = false)
        => new(new Uri(url, UriKind.RelativeOrAbsolute), provider, culture: culture, isExternal: isExternal);

    public static UrlInfo AsMessage(string message, string provider, string? culture = null)
        => new(message, provider, culture: culture);

    public static UrlInfo FromUri(Uri uri, string provider, string? culture = null, bool isExternal = false)
        => new(uri, provider, culture: culture, isExternal: isExternal);

    /// <summary>
    ///     Gets the culture.
    /// </summary>
    [DataMember(Name = "culture")]
    public string? Culture { get; }

    /// <summary>
    ///     Gets the URL.
    /// </summary>
    [DataMember(Name = "url")]
    public Uri? Url { get; }

    public string Provider { get; }

    /// <summary>
    ///     Gets the message.
    /// </summary>
    [DataMember(Name = "message")]
    public string? Message { get; }

    /// <summary>
    ///     Gets whether this is considered an external or a local URL (remote or local host).
    /// </summary>
    [DataMember(Name = "isExternal")]
    public bool IsExternal { get; }

    public static bool operator ==(UrlInfo left, UrlInfo right) => Equals(left, right);

    /// <summary>
    ///     Checks equality
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(UrlInfo? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Culture, other.Culture, StringComparison.InvariantCultureIgnoreCase)
               && Url == other.Url
               && string.Equals(Message, other.Message, StringComparison.InvariantCultureIgnoreCase)
               && IsExternal == other.IsExternal;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((UrlInfo)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Culture != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Culture) : 0;
            hashCode = (hashCode * 397) ^
                       (Url != null ? Url.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^
                       (Message != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Message) : 0);
            hashCode = (hashCode * 397) ^ IsExternal.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator !=(UrlInfo left, UrlInfo right) => !Equals(left, right);

    public override string ToString() => Url?.ToString() ?? Message ?? "[empty]";
}
