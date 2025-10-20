using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Template;

public class CreateTemplateForDocumentTypeRequestModel
{
    public Guid? Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Alias { get; set; }

    [Required]
    public required ReferenceByIdModel DocumentType { get; set; }
}
