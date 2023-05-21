// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Xml.XPath;
using Umbraco.Cms.Core.Xml;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extensions to XPathNavigator.
/// </summary>
public static class XPathNavigatorExtensions
{
    /// <summary>
    ///     Selects a node set, using the specified XPath expression.
    /// </summary>
    /// <param name="navigator">A source XPathNavigator.</param>
    /// <param name="expression">An XPath expression.</param>
    /// <param name="variables">A set of XPathVariables.</param>
    /// <returns>An iterator over the nodes matching the specified expression.</returns>
    public static XPathNodeIterator Select(this XPathNavigator navigator, string expression, params XPathVariable[] variables)
    {
        if (variables == null || variables.Length == 0 || variables[0] == null)
        {
            return navigator.Select(expression);
        }

        // Reflector shows that the standard XPathNavigator.Compile method just does
        //   return XPathExpression.Compile(xpath);
        // only difference is, XPathNavigator.Compile is virtual so it could be overridden
        // by a class inheriting from XPathNavigator... there does not seem to be any
        // doing it in the Framework, though... so we'll assume it's much cleaner to use
        // the static compile:
        var compiled = XPathExpression.Compile(expression);

        var context = new DynamicContext();
        foreach (XPathVariable variable in variables)
        {
            context.AddVariable(variable.Name, variable.Value);
        }

        compiled.SetContext(context);
        return navigator.Select(compiled);
    }

    /// <summary>
    ///     Selects a node set, using the specified XPath expression.
    /// </summary>
    /// <param name="navigator">A source XPathNavigator.</param>
    /// <param name="expression">An XPath expression.</param>
    /// <param name="variables">A set of XPathVariables.</param>
    /// <returns>An iterator over the nodes matching the specified expression.</returns>
    public static XPathNodeIterator Select(this XPathNavigator navigator, XPathExpression expression, params XPathVariable[] variables)
    {
        if (variables == null || variables.Length == 0 || variables[0] == null)
        {
            return navigator.Select(expression);
        }

        XPathExpression compiled = expression.Clone(); // clone for thread-safety
        var context = new DynamicContext();
        foreach (XPathVariable variable in variables)
        {
            context.AddVariable(variable.Name, variable.Value);
        }

        compiled.SetContext(context);
        return navigator.Select(compiled);
    }
}
