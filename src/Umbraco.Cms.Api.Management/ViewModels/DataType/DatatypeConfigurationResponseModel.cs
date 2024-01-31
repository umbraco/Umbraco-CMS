using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public class DatatypeConfigurationResponseModel
{
    public required DataTypeChangeMode CanBeChanged { get; set; }
}
