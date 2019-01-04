namespace Umbraco.Core.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class RelateOnTrashComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<RelateOnTrashComponent>();
        }
    }
}