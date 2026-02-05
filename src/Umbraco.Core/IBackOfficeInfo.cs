namespace Umbraco.Cms.Core;

/// <summary>
///     Provides information about the Umbraco backoffice.
/// </summary>
public interface IBackOfficeInfo
{
    /// <summary>
    ///     Gets the absolute url to the Umbraco Backoffice. This info can be used to build absolute urls for Backoffice to use
    ///     in mails etc.
    /// </summary>
    string GetAbsoluteUrl { get; }
}
