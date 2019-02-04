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

        /// <summary>
        /// Gets the sort order of the section
        /// </summary>
        /// <remarks>
        /// System section sort orders start at 10 (Content) and increment in steps of 10 (Media has sort order 20, Settings has sort order 30 and so on)
        /// </remarks>
        int SortOrder { get; }
    }
}
