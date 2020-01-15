using Umbraco.Core;
using Umbraco.Core.Models.Sections;

namespace Umbraco.Web.Sections
{
    /// <summary>
    /// Defines the back office translation section
    /// </summary>
    public class TranslationSection : ISection
    {
        /// <inheritdoc />
        public string Alias => Constants.Applications.Translation;

        /// <inheritdoc />
        public string Name => "Translation";
    }
}
