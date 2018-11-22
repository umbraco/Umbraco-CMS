using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Examine;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Web.Models;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.WebServices
{
    /// <summary>
    /// A public web service for querying tags
    /// </summary>
    /// <remarks>
    /// This controller does not contain methods to query for content, media or members based on tags, those methods would require
    /// authentication and should not be exposed publicly.
    /// </remarks>
    public class TagsController : UmbracoApiController
    {
        /// <summary>
        /// Get every tag stored in the database (with optional group)
        /// </summary>
        public IEnumerable<TagModel> GetAllTags(string group = null)
        {
            return Umbraco.TagQuery.GetAllTags(group);
        }

        /// <summary>
        /// Get all tags for content items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<TagModel> GetAllContentTags(string group = null)
        {
            return Umbraco.TagQuery.GetAllContentTags(group);
        }

        /// <summary>
        /// Get all tags for media items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<TagModel> GetAllMediaTags(string group = null)
        {
            return Umbraco.TagQuery.GetAllMediaTags(group);
        }

        /// <summary>
        /// Get all tags for member items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<TagModel> GetAllMemberTags(string group = null)
        {
            return Umbraco.TagQuery.GetAllMemberTags(group);
        }

        /// <summary>
        /// Returns all tags attached to a property by entity id
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        public IEnumerable<TagModel> GetTagsForProperty(int contentId, string propertyTypeAlias, string tagGroup = null)
        {
            return Umbraco.TagQuery.GetTagsForProperty(contentId, propertyTypeAlias, tagGroup);
        }

        /// <summary>
        /// Returns all tags attached to an entity (content, media or member) by entity id
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        public IEnumerable<TagModel> GetTagsForEntity(int contentId, string tagGroup = null)
        {
            return Umbraco.TagQuery.GetTagsForEntity(contentId, tagGroup);
        }

    }
}
