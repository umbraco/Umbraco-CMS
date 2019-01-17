using Umbraco.Core;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Defines the back office translation section
    /// </summary>
    public class TranslationBackOfficeSection : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Translation;
        public string Name => "Translation";
    }
}