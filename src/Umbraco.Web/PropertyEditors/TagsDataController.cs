using System.Collections.Generic;
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
    /// json formatter as it causes prob.
    /// </remarks>
    [PluginController("UmbracoApi")]
    public class TagsDataController : UmbracoAuthorizedApiController
    {
        public IEnumerable<TagModel> GetTags(string tagGroup, string culture)
        {
            if (culture == string.Empty) culture = null;
            return Umbraco.TagQuery.GetAllTags(tagGroup, culture);
        }
    }
}
