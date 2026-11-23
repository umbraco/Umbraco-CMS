using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Wrapper class for OEmbed response.
/// </summary>
[DataContract]
public class OEmbedResponse : OEmbedResponseBase<double>
{
    // these is only here to avoid breaking changes. In theory it should still be source code compatible to remove them.
    // TODO (V20): remove these properties.

    /// <summary>
    ///     Gets the thumbnail height.
    /// </summary>
    [JsonIgnore]
    public new double? ThumbnailHeight => base.ThumbnailHeight;

    /// <summary>
    ///     Gets the thumbnail width.
    /// </summary>
    [JsonIgnore]
    public new double? ThumbnailWidth => base.ThumbnailWidth;

    /// <summary>
    ///     Gets the height of the embedded content.
    /// </summary>
    [JsonIgnore]
    public new double? Height => base.Height;

    /// <summary>
    ///     Gets the width of the embedded content.
    /// </summary>
    [JsonIgnore]
    public new double? Width => base.Width;
}

