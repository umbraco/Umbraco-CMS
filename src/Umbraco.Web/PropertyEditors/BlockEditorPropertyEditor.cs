using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Abstract class for block editor based editors
    /// </summary>
    public abstract class BlockEditorPropertyEditor : DataEditor
    {
        public const string ContentTypeAliasPropertyKey = "contentTypeAlias";

        public BlockEditorPropertyEditor(ILogger logger) : base(logger)
        {
        }
    }
}
