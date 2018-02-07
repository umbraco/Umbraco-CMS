using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a void editor.
    /// </summary>
    /// <remarks>Can be used in some places where an editor is needed but no actual
    /// editor is available. Not to be used otherwise. Not discovered, and therefore
    /// not part of the editors collection.</remarks>
    [HideFromTypeFinder]
    public class VoidEditor : PropertyEditor
    {
        public VoidEditor(ILogger logger)
            : base(logger)
        {
            Alias = "Umbraco.Void";
        }
    }
}
