using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Infrastructure.Sync;

namespace Umbraco.Cms.Tests.Integration.Testing.Search;

// Delivers cache-refresher messages locally and synchronously (distributedEnabled: false) so that
// content-change notifications flow through to the search indexing pipeline within a test.
internal class LocalServerMessenger : ServerMessengerBase
{
    public LocalServerMessenger()
        : base(false, new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()))
    {
    }

    public override void SendMessages()
    {
    }

    public override void Sync()
    {
    }

    protected override void DeliverRemote(ICacheRefresher refresher, MessageType messageType, IEnumerable<object>? ids = null, string? json = null)
    {
    }
}
