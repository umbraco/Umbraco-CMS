using Umbraco.Core;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Security;

namespace Umbraco.Web.PropertyEditors
{
    internal class MarkDownPropertyValueEditor : DataValueEditor
    {
        private readonly IMarkdownSanitizer _markdownSanitizer;

        public MarkDownPropertyValueEditor(DataEditorAttribute attribute, IMarkdownSanitizer markdownSanitizer) : base(attribute)
        {
            _markdownSanitizer = markdownSanitizer;
        }

        public override object FromEditor(ContentPropertyData editorValue, object currentValue)
        {
            var editorValueString = editorValue.Value?.ToString();
            if (string.IsNullOrWhiteSpace(editorValueString))
            {
                return null;
            }

            var sanitized = _markdownSanitizer.Sanitize(editorValueString);

            return sanitized.NullOrWhiteSpaceAsNull();
        }
    }
}
