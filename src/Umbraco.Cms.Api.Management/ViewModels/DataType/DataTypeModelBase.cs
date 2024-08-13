using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public abstract class DataTypeModelBase
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string EditorAlias { get; set; } = string.Empty;

    public string EditorUiAlias { get; set; } = string.Empty;

    public IEnumerable<DataTypePropertyPresentationModel> Values { get; set; } = Enumerable.Empty<DataTypePropertyPresentationModel>();
}
