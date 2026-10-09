using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Api.Management.ViewModels.Search.Filters;

/// <summary>
/// Represents a filter in a search request. The concrete filter type is identified by the "$type" discriminator.
/// </summary>
public interface IFilterRequestModel : IOpenApiDiscriminator
{
    /// <summary>
    /// Gets or sets the name of the field to filter on.
    /// </summary>
    string FieldName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to match documents that do NOT satisfy the filter.
    /// </summary>
    bool Negate { get; set; }
}
