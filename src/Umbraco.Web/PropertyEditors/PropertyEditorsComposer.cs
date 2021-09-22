using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.PropertyEditors
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class PropertyEditorsComposer : ComponentComposer<PropertyEditorsComponent>, ICoreComposer
    { }
}
