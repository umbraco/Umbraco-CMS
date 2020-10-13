using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web
{
    public interface IPublishedContentQuery2 : IPublishedContentQuery
    {
        /// <summary>
        /// Searches content.
        /// </summary>
        /// <param name="term">The term to search.</param>
        /// <param name="skip">The amount of results to skip.</param>
        /// <param name="take">The amount of results to take/return.</param>
        /// <param name="totalRecords">The total amount of records.</param>
        /// <param name="culture">The culture (defaults to a culture insensitive search).</param>
        /// <param name="indexName">The name of the index to search (defaults to <see cref="Constants.UmbracoIndexes.ExternalIndexName" />).</param>
        /// <param name="loadedFields">The fields to load in the results of the search (defaults to all fields loaded).</param>
        /// <returns>
        /// The search results.
        /// </returns>
        /// <remarks>
        /// <para>
        /// When the <paramref name="culture" /> is not specified or is *, all cultures are searched.
        /// To search for only invariant documents and fields use null.
        /// When searching on a specific culture, all culture specific fields are searched for the provided culture and all invariant fields for all documents.
        /// </para>
        /// <para>While enumerating results, the ambient culture is changed to be the searched culture.</para>
        /// </remarks>
        IEnumerable<PublishedSearchResult> Search(string term, int skip, int take, out long totalRecords, string culture = "*", string indexName = Umbraco.Core.Constants.UmbracoIndexes.ExternalIndexName, ISet<string> loadedFields = null);
    }
}
