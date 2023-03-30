namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class CopyDocumentRequestModel
{
    public Guid? TargetKey { get; set; }

    public bool RelateToOriginal { get; set; }

    public bool IncludeDescendants { get; set; }
}
