using System;

namespace umbraco
{
    /// <summary>
    /// namespacing this interface, as only relevant to uQuery
    /// </summary>
    public static partial class uQuery
    {
        /// <summary>
        /// Implement this interface to make the uQuery .GetProperty<T>(string) extension methods pass responisbiliy for object hydration to the LoadPropertyValue(string) method
        /// 
        /// eg.IEnumerable<Node> selectedNodes = uQuery.GetCurrentNode().GetProperty<XPathAutoComplete>("propertyAlias").SelectedNodes();
        /// </summary>
        public interface IGetProperty
        {
            /// <summary>
            /// use to inflate the strongly typed object by the string value
            /// </summary>
            /// <param name="value">the Umbraco property value stored by this property</param>
            void LoadPropertyValue(string value);
        }
    }
}
