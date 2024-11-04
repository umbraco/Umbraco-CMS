using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Wrapper class for OEmbed response.
/// </summary>
[DataContract]
public class OEmbedResponse : OEmbedResponseBase<double>
{

    // these is only here to avoid breaking changes. In theory it should still be source code compatible to remove them.
    public new double? ThumbnailHeight => base.ThumbnailHeight;

    public new double? ThumbnailWidth => base.ThumbnailWidth;

    public new double? Height => base.Height;

    public new double? Width => base.Width;
}

