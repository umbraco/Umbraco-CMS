using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

public class CreateDocumentTypeTemplateRequestModel
{
    [Required]
    public required string Alias { get; set; }

    [Required]
    public required string Name { get; set; }

    public bool IsDefault { get; set; }
}
