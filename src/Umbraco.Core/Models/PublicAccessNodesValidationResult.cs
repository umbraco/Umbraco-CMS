namespace Umbraco.Cms.Core.Models;

public class PublicAccessNodesValidationResult
{
    public IContent? ProtectedNode { get; set; }

    public IContent? LoginNode { get; set; }

    public IContent? ErrorNode { get; set; }

}
