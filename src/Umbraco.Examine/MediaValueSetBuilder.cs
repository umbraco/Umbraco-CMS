using System;
using Examine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Scoping;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Examine
{
    public class MediaValueSetBuilder : BaseValueSetBuilder<IMedia>
    {
        private readonly UrlSegmentProviderCollection _urlSegmentProviders;
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly IScopeProvider _scopeProvider;
        private static readonly object[] _negativeOneArray = new object[] { -1 };
        private static readonly object[] _doubleQuestionMarkArray = new object[] { "??" };

        public MediaValueSetBuilder(PropertyEditorCollection propertyEditors,
            UrlSegmentProviderCollection urlSegmentProviders,
            IUserService userService, ILogger logger,
            IScopeProvider scopeProvider)
            : base(propertyEditors, false)
        {
            _urlSegmentProviders = urlSegmentProviders;
            _userService = userService;
            _logger = logger;
            _scopeProvider = scopeProvider;
        }

        /// <inheritdoc />
        public override IEnumerable<ValueSet> GetValueSets(params IMedia[] media)
        {
            Dictionary<int, object[]> creatorIds;

            // We can lookup all of the creator/writer names at once which can save some
            // processing below instead of one by one.
            using (var scope = _scopeProvider.CreateScope())
            {
                creatorIds = _userService.GetProfilesById(media.Select(x => x.CreatorId).ToArray())
                    .ToDictionary(x => x.Id, x => new object[] { x.Name });
                scope.Complete();
            }

            return GetValueSetsEnumerable(media, creatorIds);
        }
        public  IEnumerable<ValueSet> GetValueSetsEnumerable(IMedia[] media, Dictionary<int, object[]> creatorIds)
        {
            foreach (var m in media)
            {
                var urlValue = m.GetUrlSegment(_urlSegmentProviders);

                var umbracoFilePath = string.Empty;
                var umbracoFile = string.Empty;

                var umbracoFileSource = m.GetValue<string>(Constants.Conventions.Media.File);

                if (umbracoFileSource.DetectIsJson())
                {
                    ImageCropperValue cropper = null;
                    try
                    {
                        cropper = JsonConvert.DeserializeObject<ImageCropperValue>(
                            m.GetValue<string>(Constants.Conventions.Media.File));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error<MediaValueSetBuilder>(ex, $"Could not Deserialize ImageCropperValue for item with key {m.Key} ");
                    }

                    if (cropper != null)
                    {
                        umbracoFilePath = cropper.Src;
                    }
                }
                else
                {
                    umbracoFilePath = umbracoFileSource;
                }

                if (!string.IsNullOrEmpty(umbracoFilePath))
                {
                    // intentional dummy Uri
                    var uri = new Uri("https://localhost/" + umbracoFilePath);
                    umbracoFile = uri.Segments.Last();
                }

                var values = new Dictionary<string, IEnumerable<object>>
                {
                    {"icon", m.ContentType.Icon?.Yield() ?? Enumerable.Empty<string>()},
                    {"id", new object[] {m.Id}},
                    {UmbracoExamineIndex.NodeKeyFieldName, new object[] {m.Key}},
                    {"parentID", m.Level > 1 ? new object[] {m.ParentId } : _negativeOneArray},
                    {"level", new object[] {m.Level}},
                    {"creatorID", new object[] {m.CreatorId}},
                    {"sortOrder", new object[] {m.SortOrder}},
                    {"createDate", new object[] {m.CreateDate}},
                    {"updateDate", new object[] {m.UpdateDate}},
                    {"nodeName", m.Name?.Yield() ?? Enumerable.Empty<string>()},
                    {"urlName", urlValue?.Yield() ?? Enumerable.Empty<string>()},
                    {"path", m.Path?.Yield() ?? Enumerable.Empty<string>()},
                    {"nodeType", m.ContentType.Id.ToString().Yield() },
                   {"creatorName", creatorIds.TryGetValue(m.CreatorId, out var creatorProfile) ? creatorProfile : _doubleQuestionMarkArray },
                    {UmbracoExamineIndex.UmbracoFileFieldName, umbracoFile.Yield()}
                };

                foreach (var property in m.Properties)
                {
                    AddPropertyValue(property, null, null, values);
                }

                var vs = new ValueSet(m.Id.ToInvariantString(), IndexTypes.Media, m.ContentType.Alias, values);

                yield return vs;
            }
        }
    }

}
