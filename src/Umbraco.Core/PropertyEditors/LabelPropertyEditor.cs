using Umbraco.Core.Logging;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a property editor for label properties.
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.Label, "Label", "readonlyvalue", Icon = "icon-readonly")]
    public class LabelPropertyEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LabelPropertyEditor"/> class.
        /// </summary>
        public LabelPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor() => new LabelPropertyValueEditor(Attribute);

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new LabelConfigurationEditor();

        // provides the property value editor
        internal class LabelPropertyValueEditor : DataValueEditor
        {
            public LabelPropertyValueEditor(DataEditorAttribute attribute)
                : base(attribute)
            { }

            /// <inheritdoc />
            public override bool IsReadOnly => true;
        }
    }
}
