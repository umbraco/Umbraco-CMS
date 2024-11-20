using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

public class DocumentTypeConfigurationResponseModel
{
    public required DataTypeChangeMode DataTypesCanBeChanged { get; set; }

    public required bool DisableTemplates { get; set; }

    public required bool UseSegments { get; set; }

    public required ISet<string> ReservedFieldNames { get; set; }
}
