namespace Umbraco.Cms.Core.Sections;

/// <summary>
///     Defines the back office content section
/// </summary>
public class ContentSection : ISection
{
    /// <inheritdoc />
    public string Alias => Constants.Applications.Content;

    /// <inheritdoc />
    public string Name => "Content";
}
