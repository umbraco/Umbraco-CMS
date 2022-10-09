using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

[Obsolete("Use OEmbedProviderBase instead- This will be removed in Umbraco 12")]
public abstract class EmbedProviderBase : OEmbedProviderBase
{
    protected EmbedProviderBase(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }
}
