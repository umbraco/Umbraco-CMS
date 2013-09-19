using Newtonsoft.Json;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// An interface that is shared between parameter and property value editors to access their views
    /// </summary>
    public interface IValueEditor
    {
        string View { get; }
    }

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