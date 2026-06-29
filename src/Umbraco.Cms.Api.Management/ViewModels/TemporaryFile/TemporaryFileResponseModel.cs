using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

/// <summary>
/// Represents a response containing information about a temporary file.
/// </summary>
public class TemporaryFileResponseModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the temporary file.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the date and time until which the temporary file is available.</summary>
    public DateTimeOffset? AvailableUntil { get; set; }

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    [Required]
    public string FileName { get; set; } = string.Empty;
}
