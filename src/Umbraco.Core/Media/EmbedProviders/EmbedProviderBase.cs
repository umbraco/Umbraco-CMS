using System;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders
{
    [Obsolete("Use OEmbedProviderBase instead")]
    public abstract class EmbedProviderBase : OEmbedProviderBase
    {
        protected EmbedProviderBase(IJsonSerializer jsonSerializer)
            : base(jsonSerializer)
        {
        }
    }
}
