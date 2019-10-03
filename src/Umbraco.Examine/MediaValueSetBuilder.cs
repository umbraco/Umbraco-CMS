using System;
using Examine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Examine
{
    public class MediaValueSetBuilder : BaseValueSetBuilder<IMedia>
    {
        private readonly UrlSegmentProviderCollection _urlSegmentProviders;
        private readonly IUserService _userService;
        private readonly IRuntimeState _runtimeState;

        public MediaValueSetBuilder(PropertyEditorCollection propertyEditors,
            UrlSegmentProviderCollection urlSegmentProviders,
            IUserService userService, IRuntimeState runtimeState)
            : base(propertyEditors, false)
        {
            _urlSegmentProviders = urlSegmentProviders;
            _userService = userService;
            _runtimeState = runtimeState;
        }

        /// <inheritdoc />
        public override IEnumerable<ValueSet> GetValueSets(params IMedia[] media)
        {
            foreach (var m in media)
            {
                var urlValue = m.GetUrlSegment(_urlSegmentProviders);

                var umbracoFilePath = string.Empty;
                var umbracoFile = string.Empty;

                var umbracoFileSource  = m.GetValue<string>(Constants.Conventions.Media.File);

                if (umbracoFileSource.DetectIsJson())
                {
                    var cropper = JsonConvert.DeserializeObject<ImageCropperValue>(m.GetValue<string>(Constants.Conventions.Media.File));
                    if (cropper != null)
                    {
                        umbracoFilePath = cropper.Src;
                    }
                }
                else
                {
                    umbracoFilePath = umbracoFileSource;
                }

                var uri = new Uri(_runtimeState.ApplicationUrl.GetLeftPart(UriPartial.Authority) + umbracoFilePath);
                umbracoFile = uri.Segments.Last();

                var values = new Dictionary<string, IEnumerable<object>>
                {
                    {"icon", m.ContentType.Icon?.Yield() ?? Enumerable.Empty<string>()},
                    {"id", new object[] {m.Id}},
                    {UmbracoExamineIndex.NodeKeyFieldName, new object[] {m.Key}},
                    {"parentID", new object[] {m.Level > 1 ? m.ParentId : -1}},
                    {"level", new object[] {m.Level}},
                    {"creatorID", new object[] {m.CreatorId}},
                    {"sortOrder", new object[] {m.SortOrder}},
                    {"createDate", new object[] {m.CreateDate}},
                    {"updateDate", new object[] {m.UpdateDate}},
                    {"nodeName", m.Name?.Yield() ?? Enumerable.Empty<string>()},
                    {"urlName", urlValue?.Yield() ?? Enumerable.Empty<string>()},
                    {"path", m.Path?.Yield() ?? Enumerable.Empty<string>()},
                    {"nodeType", m.ContentType.Id.ToString().Yield() },
                    {"creatorName", (m.GetCreatorProfile(_userService)?.Name ?? "??").Yield()},
                    {"__umbracoFile", new object[] {umbracoFile}}
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
