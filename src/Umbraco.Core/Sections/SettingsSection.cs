namespace Umbraco.Cms.Core.Sections;

/// <summary>
///     Defines the back office settings section
/// </summary>
public class SettingsSection : ISection
{
    /// <inheritdoc />
    public string Alias => Constants.Applications.Settings;

    /// <inheritdoc />
    public string Name => "Settings";
}
