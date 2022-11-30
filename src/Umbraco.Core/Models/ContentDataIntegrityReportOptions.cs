namespace Umbraco.Cms.Core.Models;

public class ContentDataIntegrityReportOptions
{
    /// <summary>
    ///     Set to true to try to automatically resolve data integrity issues
    /// </summary>
    public bool FixIssues { get; set; }

    // TODO: We could define all sorts of options for the data integrity check like what to check for, what to fix, etc...
    // things like Tag data consistency, etc...
}
