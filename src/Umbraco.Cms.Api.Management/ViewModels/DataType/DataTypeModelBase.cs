using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public abstract class DataTypeModelBase
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string PropertyEditorAlias { get; set; } = string.Empty;

    public string? PropertyEditorUiAlias { get; set; }

    public IEnumerable<DataTypePropertyPresentationModel> Values { get; set; } = Enumerable.Empty<DataTypePropertyPresentationModel>();
}
