namespace Umbraco.Cms.Core.Sections;

/// <summary>
///     Defines a back office section.
/// </summary>
public interface ISection
{
    /// <summary>
    ///     Gets the alias of the section.
    /// </summary>
    string Alias { get; }

    /// <summary>
    ///     Gets the name of the section.
    /// </summary>
    string Name { get; }
}
