using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Compose
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class NestedContentPropertyComposer : ComponentComposer<NestedContentPropertyComponent>, ICoreComposer
    { }
}
