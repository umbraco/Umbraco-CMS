using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// A controller used for type-ahead values for tags
    /// </summary>
    /// <remarks>
    /// DO NOT inherit from UmbracoAuthorizedJsonController since we don't want to use the angularized
    /// json formatter as it causes problems.
    /// </remarks>
    [PluginController("UmbracoApi")]
    public class TagsDataController : UmbracoAuthorizedApiController
    {
        /// <summary>
        /// Returns all tags matching tagGroup, culture and an optional query
        /// </summary>
        /// <param name="tagGroup"></param>
        /// <param name="culture"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<TagModel> GetTags(string tagGroup, string culture, string query = null)
        {
            if (culture == string.Empty) culture = null;

            var result = Umbraco.TagQuery.GetAllTags(tagGroup, culture);

            
            if (!query.IsNullOrWhiteSpace())
            {
                //TODO: add the query to TagQuery + the tag service, this is ugly but all we can do for now.
                //currently we are post filtering this :( but works for now
                result = result.Where(x => x.Text.InvariantContains(query));
            }

            return result.OrderBy(x => x.Text);
        }
    }
}
