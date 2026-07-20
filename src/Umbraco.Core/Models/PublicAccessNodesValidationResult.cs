namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the validation result containing resolved content nodes for public access configuration.
/// </summary>
public class PublicAccessNodesValidationResult
{
    /// <summary>
    ///     Gets or sets the content node that is being protected.
    /// </summary>
    public IContent? ProtectedNode { get; set; }

    /// <summary>
    ///     Gets or sets the login page content node.
    /// </summary>
    public IContent? LoginNode { get; set; }

    /// <summary>
    ///     Gets or sets the error page content node shown for unauthorized access.
    /// </summary>
    public IContent? ErrorNode { get; set; }
}
