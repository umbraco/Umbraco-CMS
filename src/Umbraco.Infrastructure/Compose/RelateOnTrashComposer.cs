using Umbraco.Core.Composing;

namespace Umbraco.Core.Compose
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class RelateOnTrashComposer : ComponentComposer<RelateOnTrashComponent>, ICoreComposer
    { }
}
