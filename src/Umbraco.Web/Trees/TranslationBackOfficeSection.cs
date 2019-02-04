using Umbraco.Core;
using Umbraco.Core.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Defines the back office translation section
    /// </summary>
    public class TranslationBackOfficeSection : IBackOfficeSection
    {
        /// <inheritdoc />
        public string Alias => Constants.Applications.Translation;

        /// <inheritdoc />
        public string Name => "Translation";

        /// <inheritdoc />
        public int SortOrder => 70;
    }
}
