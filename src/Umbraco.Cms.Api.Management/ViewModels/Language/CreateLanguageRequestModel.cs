namespace Umbraco.Cms.Api.Management.ViewModels.Language;

public class CreateLanguageRequestModel : LanguageModelBase
{
    public string IsoCode { get; set; } = string.Empty;
}
