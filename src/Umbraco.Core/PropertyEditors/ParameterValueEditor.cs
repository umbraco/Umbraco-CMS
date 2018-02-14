using Umbraco.Core.IO;

namespace Umbraco.Core.PropertyEditors
{
    // fixme - can we kill this and use "ValueEditor" for both macro and all?

    /// <summary>
    /// Represents a value editor for macro parameters.
    /// </summary>
    public class ParameterValueEditor : IValueEditor
    {
        private string _view;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterValueEditor"/> class.
        /// </summary>
        public ParameterValueEditor()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterValueEditor"/> class.
        /// </summary>
        public ParameterValueEditor(string view)
            : this()
        {
            View = view;
        }

        /// <summary>
        /// Gets or sets the editor view.
        /// </summary>
        public string View
        {
            get => _view;
            set => _view = IOHelper.ResolveVirtualUrl(value);
        }

        /// <inheritdoc />
        public string ValueType { get; set; }
    }
}
