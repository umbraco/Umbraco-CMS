using System;
using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Services;
using Umbraco.Web.Models;

namespace Umbraco.Web
{
    /// <summary>
    /// A class that exposes methods used to query tag data in views
    /// </summary>
    public class TagQuery
    {
        private readonly ITagService _tagService;

        public TagQuery(ITagService tagService)
        {
            if (tagService == null) throw new ArgumentNullException("tagService");
            _tagService = tagService;
        }

        /// <summary>
        /// Get every tag stored in the database (with optional group)
        /// </summary>
        public IEnumerable<TagModel> GetAllTags(string group = null)
        {
            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetAllTags(group));
        }

        /// <summary>
        /// Get all tags for content items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<TagModel> GetAllContentTags(string group = null)
        {
            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetAllContentTags(group));
        }

        /// <summary>
        /// Get all tags for media items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<TagModel> GetAllMediaTags(string group = null)
        {
            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetAllMediaTags(group));
        }

        /// <summary>
        /// Get all tags for member items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<TagModel> GetAllMemberTags(string group = null)
        {
            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetAllMemberTags(group));
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
            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetTagsForProperty(contentId, propertyTypeAlias, tagGroup));
        }

        /// <summary>
        /// Returns all tags attached to an entity (content, media or member) by entity id
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        public IEnumerable<TagModel> GetTagsForEntity(int contentId, string tagGroup = null)
        {
            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetTagsForEntity(contentId, tagGroup));
        }
    }
}