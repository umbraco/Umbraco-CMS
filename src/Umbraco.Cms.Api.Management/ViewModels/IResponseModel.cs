namespace Umbraco.Cms.Api.Management.ViewModels;

/// <summary>
/// Basic required properties for ResponseModels
/// </summary>
public interface IResponseModel
{
    /// <summary>
    /// Gets the (entity)type of the ResponseModel to let the frontend identify the context of the data
    /// </summary>
    public string Type { get; }
}
