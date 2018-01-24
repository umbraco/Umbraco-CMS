using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a property editor for label properties.
    /// </summary>
    [PropertyEditor(Constants.PropertyEditors.Aliases.NoEdit, "Label", "readonlyvalue", Icon = "icon-readonly")]
    public class LabelPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LabelPropertyEditor"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public LabelPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override ValueEditor CreateValueEditor()
        {
            return new LabelPropertyValueEditor(base.CreateValueEditor());
        }

        /// <inheritdoc />
        protected override ConfigurationEditor CreateConfigurationEditor()
        {
            return new LabelConfigurationEditor();
        }

        // provides the property value editor
        internal class LabelPropertyValueEditor : PropertyValueEditorWrapper
        {
            public LabelPropertyValueEditor(ValueEditor wrapped)
                : base(wrapped)
            { }

            /// <inheritdoc />
            public override bool IsReadOnly => true;
        }
    }
}
