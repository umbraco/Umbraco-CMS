using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a property editor for label properties.
    /// </summary>
    [ValueEditor(Constants.PropertyEditors.Aliases.NoEdit, "Label", "readonlyvalue", Icon = "icon-readonly")]
    public class LabelPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LabelPropertyEditor"/> class.
        /// </summary>
        public LabelPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override IPropertyValueEditor CreateValueEditor() => new LabelPropertyValueEditor(Attribute);

        /// <inheritdoc />
        protected override ConfigurationEditor CreateConfigurationEditor() => new LabelConfigurationEditor();

        // provides the property value editor
        internal class LabelPropertyValueEditor : ValueEditor
        {
            public LabelPropertyValueEditor(ValueEditorAttribute attribute)
                : base(attribute)
            { }

            /// <inheritdoc />
            public override bool IsReadOnly => true;
        }
    }
}
