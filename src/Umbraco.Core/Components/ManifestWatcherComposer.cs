namespace Umbraco.Core.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ManifestWatcherComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<ManifestWatcherComponent>();
        }
    }
}
