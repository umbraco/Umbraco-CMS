namespace Umbraco.Cms.Core.Models.ContentEditing;

public class ValidateContentUpdateModel : ContentUpdateModel
{
    public ISet<string>? Cultures { get; set; }
}
