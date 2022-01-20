using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents an integer property and parameter editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.Integer,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Numeric",
        "integer",
        ValueType = ValueTypes.Integer)]
    public class IntegerPropertyEditor : DataEditor
    {
        public IntegerPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory)
        { }

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            editor.Validators.Add(new IntegerValidator()); // ensure the value is validated
            return editor;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new IntegerConfigurationEditor();
    }
}
