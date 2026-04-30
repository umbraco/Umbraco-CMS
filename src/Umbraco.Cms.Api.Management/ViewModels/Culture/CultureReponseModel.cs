using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Culture;

/// <summary>
/// Represents a response model for culture information in the Umbraco CMS Management API.
/// </summary>
public class CultureReponseModel
{
    /// <summary>
    /// Gets or sets the name of the culture.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the English name of the culture.
    /// </summary>
    [Required]
    public string EnglishName { get; set; } = string.Empty;
}
