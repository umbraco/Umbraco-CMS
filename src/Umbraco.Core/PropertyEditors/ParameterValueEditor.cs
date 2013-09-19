using Newtonsoft.Json;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the value editor for the parameter editor during macro parameter editing
    /// </summary>
    /// <remarks>
    /// The Json serialization attributes are required for manifest property editors to work
    /// </remarks>
    public class ParameterValueEditor
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

        /// <summary>
        /// Defines the view to use for the editor, this can be one of 3 things:
        /// * the full virtual path or 
        /// * the relative path to the current Umbraco folder 
        /// * a simple view name which will map to the views/propertyeditors/{view}/{view}.html
        /// </summary>
        [JsonProperty("view", Required = Required.Always)]
        public string View { get; set; }
    }
}