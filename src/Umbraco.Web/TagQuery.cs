using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.Models;

namespace Umbraco.Web
{
    /// <summary>
    /// Implements <see cref="ITagQuery"/>.
    /// </summary>
    public class TagQuery : ITagQuery
    {
        private readonly ITagService _tagService;
        private readonly IPublishedContentQuery _contentQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagQuery"/> class.
        /// </summary>
        public TagQuery(ITagService tagService, IPublishedContentQuery contentQuery)
        {
            _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
            _contentQuery = contentQuery ?? throw new ArgumentNullException(nameof(contentQuery));
        }

        /// <inheritdoc />
        public IEnumerable<IPublishedContent> GetContentByTag(string tag, string group = null, string culture = null)
        {
            var ids = _tagService.GetTaggedContentByTag(tag, group, culture)
                .Select(x => x.EntityId);
            return _contentQuery.Content(ids)
                .Where(x => x != null);
        }

        /// <inheritdoc />
        public IEnumerable<IPublishedContent> GetContentByTagGroup(string group, string culture = null)
        {
            var ids = _tagService.GetTaggedContentByTagGroup(group, culture)
                .Select(x => x.EntityId);
            return _contentQuery.Content(ids)
                .Where(x => x != null);
        }

        /// <inheritdoc />
        public IEnumerable<IPublishedContent> GetMediaByTag(string tag, string group = null, string culture = null)
        {
            var ids = _tagService.GetTaggedMediaByTag(tag, group, culture)
                .Select(x => x.EntityId);
            return _contentQuery.Media(ids)
                .Where(x => x != null);
        }

        /// <inheritdoc />
        public IEnumerable<IPublishedContent> GetMediaByTagGroup(string group, string culture = null)
        {
            var ids = _tagService.GetTaggedMediaByTagGroup(group, culture)
                .Select(x => x.EntityId);
            return _contentQuery.Media(ids)
                .Where(x => x != null);
        }

        /// <inheritdoc />
        public IEnumerable<TagModel> GetAllTags(string group = null, string culture = null)
        {
            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetAllTags(group, culture));
        }

        /// <inheritdoc />
        public IEnumerable<TagModel> GetAllContentTags(string group = null, string culture = null)
        {
            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetAllContentTags(group, culture));
        }

        /// <inheritdoc />
        public IEnumerable<TagModel> GetAllMediaTags(string group = null, string culture = null)
        {
            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetAllMediaTags(group, culture));
        }

        /// <inheritdoc />
        public IEnumerable<TagModel> GetAllMemberTags(string group = null, string culture = null)
        {
            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetAllMemberTags(group, culture));
        }

        /// <inheritdoc />
        public IEnumerable<TagModel> GetTagsForProperty(int contentId, string propertyTypeAlias, string group = null, string culture = null)
        {
            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetTagsForProperty(contentId, propertyTypeAlias, group, culture));
        }

        /// <inheritdoc />
        public IEnumerable<TagModel> GetTagsForEntity(int contentId, string group = null, string culture = null)
        {
            return Mapper.Map<IEnumerable<TagModel>>(_tagService.GetTagsForEntity(contentId, group, culture));
        }
    }
}
