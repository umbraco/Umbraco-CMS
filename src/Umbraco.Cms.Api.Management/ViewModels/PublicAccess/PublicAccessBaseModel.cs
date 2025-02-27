namespace Umbraco.Cms.Api.Management.ViewModels.PublicAccess;

public class PublicAccessBaseModel
{
    public required ReferenceByIdModel LoginDocument { get; set; }

    public required ReferenceByIdModel ErrorDocument { get; set; }
}
