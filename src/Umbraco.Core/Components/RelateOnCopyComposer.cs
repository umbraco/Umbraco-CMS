namespace Umbraco.Core.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class RelateOnCopyComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<RelateOnCopyComponent>();
        }
    }
}