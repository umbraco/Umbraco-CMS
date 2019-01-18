namespace Umbraco.Core.Models.Trees
{
    /// <summary>
    /// Defines a back office section.
    /// </summary>
    public interface IBackOfficeSection
    {
        /// <summary>
        /// Gets the alias of the section.
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// Gets the name of the section.
        /// </summary>
        string Name { get; }
    }
}
