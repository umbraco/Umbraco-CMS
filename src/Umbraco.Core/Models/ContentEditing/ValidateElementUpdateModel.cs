namespace Umbraco.Cms.Core.Models.ContentEditing;

public class ValidateElementUpdateModel : ElementUpdateModel
{
    public ISet<string>? Cultures { get; set; }
}
