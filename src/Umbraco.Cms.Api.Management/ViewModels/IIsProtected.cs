namespace Umbraco.Cms.Api.Management.ViewModels;

/// <summary>
/// Marker interface that indicates the type can represent the state of protected content.
/// </summary>
public interface IIsProtected
{
    /// <summary>
    /// Gets or sets a value indicating whether the model represents content that is protected.
    /// </summary>
    bool IsProtected { get; set; }
}
