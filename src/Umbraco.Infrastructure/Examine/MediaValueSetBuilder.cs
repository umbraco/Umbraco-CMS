using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Examine
{
    public class MediaValueSetBuilder : BaseValueSetBuilder<IMedia>
    {
        private readonly UrlSegmentProviderCollection _urlSegmentProviders;
        private readonly IUserService _userService;
        private readonly ILogger<MediaValueSetBuilder> _logger;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IJsonSerializer _serializer;

        public MediaValueSetBuilder(PropertyEditorCollection propertyEditors,
            UrlSegmentProviderCollection urlSegmentProviders,
            IUserService userService, ILogger<MediaValueSetBuilder> logger, IShortStringHelper shortStringHelper, IJsonSerializer serializer)
            : base(propertyEditors, false)
        {
            _urlSegmentProviders = urlSegmentProviders;
            _userService = userService;
            _logger = logger;
            _shortStringHelper = shortStringHelper;
            _serializer = serializer;
        }

        /// <inheritdoc />
        public override IEnumerable<ValueSet> GetValueSets(params IMedia[] media)
        {
            foreach (var m in media)
            {
                var urlValue = m.GetUrlSegment(_shortStringHelper, _urlSegmentProviders);

                var umbracoFilePath = string.Empty;
                var umbracoFile = string.Empty;

                var umbracoFileSource  = m.GetValue<string>(Constants.Conventions.Media.File);

                if (umbracoFileSource.DetectIsJson())
                {
                    ImageCropperValue cropper = null;
                    try
                    {
                        cropper = _serializer.Deserialize<ImageCropperValue>(
                            m.GetValue<string>(Constants.Conventions.Media.File));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Could not Deserialize ImageCropperValue for item with key {m.Key} ");
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
                    {UmbracoExamineFieldNames.NodeKeyFieldName, new object[] {m.Key}},
                    {"parentID", new object[] {m.Level > 1 ? m.ParentId : -1}},
                    {"level", new object[] {m.Level}},
                    {"creatorID", new object[] {m.CreatorId}},
                    {"sortOrder", new object[] {m.SortOrder}},
                    {"createDate", new object[] {m.CreateDate}},
                    {"updateDate", new object[] {m.UpdateDate}},
                    {UmbracoExamineFieldNames.NodeNameFieldName, m.Name?.Yield() ?? Enumerable.Empty<string>()},
                    {"urlName", urlValue?.Yield() ?? Enumerable.Empty<string>()},
                    {"path", m.Path?.Yield() ?? Enumerable.Empty<string>()},
                    {"nodeType", m.ContentType.Id.ToString().Yield() },
                    {"creatorName", (m.GetCreatorProfile(_userService)?.Name ?? "??").Yield()},
                    {UmbracoExamineFieldNames.UmbracoFileFieldName, umbracoFile.Yield()}
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
