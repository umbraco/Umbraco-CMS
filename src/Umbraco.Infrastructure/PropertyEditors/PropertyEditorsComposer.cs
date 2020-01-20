using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.PropertyEditors
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    internal class PropertyEditorsComposer : ComponentComposer<PropertyEditorsComponent>, ICoreComposer
    { }
}
