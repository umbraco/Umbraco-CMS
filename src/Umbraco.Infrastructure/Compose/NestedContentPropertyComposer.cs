using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Compose
{
    /// <summary>
    /// A composer for nested content to run a component
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class NestedContentPropertyComposer : ComponentComposer<NestedContentPropertyComponent>, ICoreComposer
    { }
}
