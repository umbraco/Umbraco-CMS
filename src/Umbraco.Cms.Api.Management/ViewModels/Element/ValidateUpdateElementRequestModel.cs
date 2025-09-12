namespace Umbraco.Cms.Api.Management.ViewModels.Element;

public class ValidateUpdateElementRequestModel : UpdateElementRequestModel
{
    public ISet<string>? Cultures { get; set; }
}
