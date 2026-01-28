namespace Umbraco.Cms.Api.Management.ViewModels.Element;

public class CopyElementRequestModel
{
    public ReferenceByIdModel? Target { get; set; }

    // TODO ELEMENTS: do we want a relate-to-original feature for elements?
    // public bool RelateToOriginal { get; set; }
}
