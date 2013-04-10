using System.Xml.XPath;

namespace Umbraco.Core.Xml
{
    /// <summary>
    /// Provides extensions to XPathNavigator.
    /// </summary>
    internal static class XPathNavigatorExtensions
    {
        /// <summary>
        /// Selects a node set, using the specified XPath expression.
        /// </summary>
        /// <param name="navigator">A source XPathNavigator.</param>
        /// <param name="expression">An XPath expression.</param>
        /// <param name="variables">A set of XPathVariables.</param>
        /// <returns>An iterator over the nodes matching the specified expression.</returns>
        public static XPathNodeIterator Select(this XPathNavigator navigator, string expression, params XPathVariable[] variables)
        {
            if (variables == null || variables.Length == 0 || variables[0] == null)
                return navigator.Select(expression);

            var compiled = navigator.Compile(expression);
            var context = new DynamicContext();
            foreach (var variable in variables)
                context.AddVariable(variable.Name, variable.Value);
            compiled.SetContext(context);
            return navigator.Select(compiled);
        }
    }
}
