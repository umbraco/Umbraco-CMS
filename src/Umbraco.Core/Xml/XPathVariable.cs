// source: mvpxml.codeplex.com

namespace Umbraco.Cms.Core.Xml;

/// <summary>
///     Represents a variable in an XPath query.
/// </summary>
/// <remarks>The name must be <c>foo</c> in the constructor and <c>$foo</c> in the XPath query.</remarks>
public class XPathVariable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="XPathVariable" /> class with a name and a value.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public XPathVariable(string name, string value)
    {
        Name = name;
        Value = value;
    }

    /// <summary>
    ///     Gets or sets the name of the variable.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets or sets the value of the variable.
    /// </summary>
    public string Value { get; }
}
