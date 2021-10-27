using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Compose
{
    /// <summary>
    /// A composer for Block editors to run a component
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class BlockEditorComposer : ComponentComposer<BlockEditorComponent>, ICoreComposer
    { }
}
