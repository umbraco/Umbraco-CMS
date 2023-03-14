namespace Umbraco.Cms.Core.Sections;

/// <summary>
///     Defines the back office media section
/// </summary>
public class MediaSection : ISection
{
    public string Alias => Constants.Applications.Media;

    public string Name => "Media";
}
