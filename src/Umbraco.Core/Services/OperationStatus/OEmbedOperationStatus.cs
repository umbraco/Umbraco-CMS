namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of an oEmbed operation for retrieving embedded content.
/// </summary>
public enum OEmbedOperationStatus
{
    /// <summary>
    /// The oEmbed operation completed successfully and embedded content was retrieved.
    /// </summary>
    Success,

    /// <summary>
    /// No oEmbed provider was found that supports the requested URL.
    /// </summary>
    NoSupportedProvider,

    /// <summary>
    /// The oEmbed provider returned an invalid or unparseable result.
    /// </summary>
    ProviderReturnedInvalidResult,

    /// <summary>
    /// An unexpected exception occurred during the oEmbed operation.
    /// </summary>
    UnexpectedException
}
