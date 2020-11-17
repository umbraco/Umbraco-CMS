using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Compose
{
    /// <summary>
    /// A composer for nested content to run a component
    /// </summary>
    public class NestedContentPropertyComposer : ComponentComposer<NestedContentPropertyComponent>, ICoreComposer
    { }
}
