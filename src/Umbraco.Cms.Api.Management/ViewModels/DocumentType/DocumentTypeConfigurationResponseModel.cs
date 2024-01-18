using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

public class DocumentTypeConfigurationResponseModel
{
    public DataTypeChangeMode DataTypesCanBeChanged { get; set; }

    public bool DisableTemplates { get; set; }
}
