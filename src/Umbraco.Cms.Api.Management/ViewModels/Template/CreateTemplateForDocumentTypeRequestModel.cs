using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Template;

public class CreateTemplateForDocumentTypeRequestModel
{
    [Required]
    public required ReferenceByIdModel DocumentType { get; set; }
}
