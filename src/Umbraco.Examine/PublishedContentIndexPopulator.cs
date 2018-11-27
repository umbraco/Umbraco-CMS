using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Persistence;

namespace Umbraco.Examine
{
    /// <summary>
    /// Performs the data lookups required to rebuild a content index containing only published content
    /// </summary>
    /// <remarks>
    /// The published (external) index will still rebuild just fine using the default <see cref="ContentIndexPopulator"/> which is what
    /// is used when rebuilding all indexes, but this will be used when the single index is rebuilt and will go a little bit faster
    /// since the data query is more specific.
    /// </remarks>
    public class PublishedContentIndexPopulator : ContentIndexPopulator
    {
        public PublishedContentIndexPopulator(IContentService contentService, ISqlContext sqlContext, IValueSetBuilder<IContent> contentValueSetBuilder) :
            base(false, null, contentService, sqlContext, contentValueSetBuilder)
        {   
        }
    }
}
