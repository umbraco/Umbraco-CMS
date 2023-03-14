namespace Umbraco.Cms.Core.Sections;

/// <summary>
///     Defines the back office translation section
/// </summary>
public class TranslationSection : ISection
{
    /// <inheritdoc />
    public string Alias => Constants.Applications.Translation;

    /// <inheritdoc />
    public string Name => "Translation";
}
