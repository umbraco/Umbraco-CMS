using System.Collections.Generic;
using Umbraco.Web.Editors;
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
    /// json formatter as it causes probs.
    /// </remarks>
    [PluginController("UmbracoApi")]
    public class TagsDataController : UmbracoAuthorizedApiController
    {
        public IEnumerable<TagModel> GetTags(string tagGroup)
        {
            return Umbraco.TagQuery.GetAllTags(tagGroup);
        }
    }
}