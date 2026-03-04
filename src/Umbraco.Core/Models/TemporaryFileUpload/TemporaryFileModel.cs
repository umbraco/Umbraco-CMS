namespace Umbraco.Cms.Core.Models.TemporaryFile;

/// <summary>
///     Represents a temporary file with its availability expiration time.
/// </summary>
public class TemporaryFileModel : TemporaryFileModelBase
{
    /// <summary>
    ///     Gets or sets the date and time until which the temporary file is available.
    /// </summary>
    public required DateTime AvailableUntil { get; set; }
}
