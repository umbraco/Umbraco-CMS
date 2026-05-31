namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

/// <summary>
/// Represents the data required to import dictionary items into the system.
/// </summary>
public class ImportDictionaryRequestModel
{
    /// <summary>
    /// Gets or sets a reference to the temporary file used for dictionary import.
    /// </summary>
    public required ReferenceByIdModel TemporaryFile { get; set; }

    /// <summary>
    /// Gets or sets the parent dictionary item reference.
    /// </summary>
    public ReferenceByIdModel? Parent { get; set; }
}
