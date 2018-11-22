using Newtonsoft.Json;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the value editor for the parameter editor during macro parameter editing
    /// </summary>
    public class ParameterValueEditor : IValueEditor
    {
        /// <summary>
        /// default ctor
        /// </summary>
        public ParameterValueEditor()
        {           
        }

        /// <summary>
        /// Creates a new editor with the specified view
        /// </summary>
        /// <param name="view"></param>
        public ParameterValueEditor(string view)
            : this()
        {
            View = view;
        }

        public string View { get; set; }
    }
}