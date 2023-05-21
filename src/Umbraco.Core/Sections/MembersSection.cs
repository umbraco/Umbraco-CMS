namespace Umbraco.Cms.Core.Sections;

/// <summary>
///     Defines the back office members section
/// </summary>
public class MembersSection : ISection
{
    /// <inheritdoc />
    public string Alias => Constants.Applications.Members;

    /// <inheritdoc />
    public string Name => "Members";
}
