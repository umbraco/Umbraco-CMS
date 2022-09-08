namespace Umbraco.Cms.Core.Sections;

/// <summary>
///     Defines the back office packages section
/// </summary>
public class PackagesSection : ISection
{
    /// <inheritdoc />
    public string Alias => Constants.Applications.Packages;

    /// <inheritdoc />
    public string Name => "Packages";
}
