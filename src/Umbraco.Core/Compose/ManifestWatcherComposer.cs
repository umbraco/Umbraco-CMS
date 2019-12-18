using Umbraco.Core.Composing;

namespace Umbraco.Core.Compose
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ManifestWatcherComposer : ComponentComposer<ManifestWatcherComponent>, ICoreComposer
    { }
}
