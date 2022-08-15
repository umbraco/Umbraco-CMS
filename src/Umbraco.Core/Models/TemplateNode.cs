namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a template in a template tree
/// </summary>
public class TemplateNode
{
    public TemplateNode(ITemplate template)
    {
        Template = template;
        Children = new List<TemplateNode>();
    }

    /// <summary>
    ///     The current template
    /// </summary>
    public ITemplate Template { get; set; }

    /// <summary>
    ///     The children of the current template
    /// </summary>
    public IEnumerable<TemplateNode> Children { get; set; }

    /// <summary>
    ///     The parent template to the current template
    /// </summary>
    /// <remarks>
    ///     Will be null if there is no parent
    /// </remarks>
    public TemplateNode? Parent { get; set; }
}
