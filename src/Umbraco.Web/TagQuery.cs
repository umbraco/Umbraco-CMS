using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models;

namespace Umbraco.Web
{
    /// <summary>
    /// A class that exposes methods used to query tag data in views
    /// </summary>
    public class TagQuery : ITagQuery
    {

        //TODO: This class also acts as a wrapper for ITagQuery due to breaking changes, need to fix in
        // version 8: http://issues.umbraco.org/issue/U4-6899
        private readonly ITagQuery _wrappedQuery;

        private readonly ITagService _tagService;
        private readonly ITypedPublishedContentQuery _typedContentQuery;

        [Obsolete("Use the alternate constructor specifying the contentQuery instead")]
        public TagQuery(ITagService tagService)
            : this(tagService, new PublishedContentQuery(UmbracoContext.Current.ContentCache, UmbracoContext.Current.MediaCache))
        {
        }

        [Obsolete("Use the alternate constructor specifying the ITypedPublishedContentQuery instead")]
        public TagQuery(ITagService tagService, PublishedContentQuery contentQuery)
        {
            if (tagService == null) throw new ArgumentNullException("tagService");
            if (contentQuery == null) throw new ArgumentNullException("contentQuery");
            _tagService = tagService;
            _typedContentQuery = contentQuery;
        }

        /// <summary>
        /// Constructor for wrapping ITagQuery, see http://issues.umbraco.org/issue/U4-6899
        /// </summary>
        /// <param name="wrappedQuery"></param>
        internal TagQuery(ITagQuery wrappedQuery)
        {
            if (wrappedQuery == null) throw new ArgumentNullException("wrappedQuery");
            _wrappedQuery = wrappedQuery;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tagService"></param>
        /// <param name="typedContentQuery"></param>
        public TagQuery(ITagService tagService, ITypedPublishedContentQuery typedContentQuery)
        {
            if (tagService == null) throw new ArgumentNullException("tagService");
            if (typedContentQuery == null) throw new ArgumentNullException("typedContentQuery");
            _tagService = tagService;
            _typedContentQuery = typedContentQuery;
        }
        
        /// <summary>
        /// Returns all content that is tagged with the specified tag value and optional tag group
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> GetContentByTag(string tag, string tagGroup = null)
        {
            //TODO: http://issues.umbraco.org/issue/U4-6899
            if (_wrappedQuery != null) return _wrappedQuery.GetContentByTag(tag, tagGroup);

            var ids = _tagService.GetTaggedContentByTag(tag, tagGroup)
                .Select(x => x.EntityId);
            return _typedContentQuery.TypedContent(ids)
                .Where(x => x != null);
        }

        /// <summary>
        /// Returns all content that has been tagged with any tag in the specified group
        /// </summary>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> GetContentByTagGroup(string tagGroup)
        {
            //TODO: http://issues.umbraco.org/issue/U4-6899
            if (_wrappedQuery != null) return _wrappedQuery.GetContentByTagGroup(tagGroup);

            var ids = _tagService.GetTaggedContentByTagGroup(tagGroup)
                .Select(x => x.EntityId);
            return _typedContentQuery.TypedContent(ids)
                .Where(x => x != null);
        }

        /// <summary>
        /// Returns all Media that is tagged with the specified tag value and optional tag group
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> GetMediaByTag(string tag, string tagGroup = null)
        {
            //TODO: http://issues.umbraco.org/issue/U4-6899
            if (_wrappedQuery != null) return _wrappedQuery.GetMediaByTag(tag, tagGroup);

            var ids = _tagService.GetTaggedMediaByTag(tag, tagGroup)
                .Select(x => x.EntityId);
            return _typedContentQuery.TypedMedia(ids)
                .Where(x => x != null);
        }

        /// <summary>
        /// Returns all Media that has been tagged with any tag in the specified group
        /// </summary>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> GetMediaByTagGroup(string tagGroup)
        {
            //TODO: http://issues.umbraco.org/issue/U4-6899
            if (_wrappedQuery != null) return _wrappedQuery.GetMediaByTagGroup(tagGroup);

            var ids = _tagService.GetTaggedMediaByTagGroup(tagGroup)
                .Select(x => x.EntityId);
            return _typedContentQuery.TypedMedia(ids)
                .Where(x => x != null);
        }

        //TODO: Should prob implement these, requires a bit of work on the member service to do this,
        // also not sure if its necessary ?
        //public IEnumerable<IPublishedContent> GetMembersByTag(string tag, string tagGroup = null)
        //{
        //}

        //public IEnumerable<IPublishedContent> GetMembersByTagGroup(string tagGroup)
        //{
        //}

        /// <summary>
        /// Get every tag stored in the database (with optional group)
        /// </summary>
        public IEnumerable<TagModel> GetAllTags(string group = null)
        {
            //TODO: http://issues.umbraco.org/issue/U4-6899
            if (_wrappedQuery != null) return _wrappedQuery.GetAllTags(group);

            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetAllTags(group));
        }

        /// <summary>
        /// Get all tags for content items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<TagModel> GetAllContentTags(string group = null)
        {
            //TODO: http://issues.umbraco.org/issue/U4-6899
            if (_wrappedQuery != null) return _wrappedQuery.GetAllContentTags(group);

            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetAllContentTags(group));
        }

        /// <summary>
        /// Get all tags for media items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<TagModel> GetAllMediaTags(string group = null)
        {
            //TODO: http://issues.umbraco.org/issue/U4-6899
            if (_wrappedQuery != null) return _wrappedQuery.GetAllMediaTags(group);

            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetAllMediaTags(group));
        }

        /// <summary>
        /// Get all tags for member items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<TagModel> GetAllMemberTags(string group = null)
        {
            //TODO: http://issues.umbraco.org/issue/U4-6899
            if (_wrappedQuery != null) return _wrappedQuery.GetAllMemberTags(group);

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
            //TODO: http://issues.umbraco.org/issue/U4-6899
            if (_wrappedQuery != null) return _wrappedQuery.GetTagsForProperty(contentId, propertyTypeAlias, tagGroup);

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
            //TODO: http://issues.umbraco.org/issue/U4-6899
            if (_wrappedQuery != null) return _wrappedQuery.GetTagsForEntity(contentId, tagGroup);

            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetTagsForEntity(contentId, tagGroup));
        }
    }
}