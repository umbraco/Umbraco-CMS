using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Obsolete base class for embed providers. Use <see cref="OEmbedProviderBase"/> instead.
/// </summary>
[Obsolete("Use OEmbedProviderBase instead")]
public abstract class EmbedProviderBase : OEmbedProviderBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EmbedProviderBase"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    protected EmbedProviderBase(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }
}
