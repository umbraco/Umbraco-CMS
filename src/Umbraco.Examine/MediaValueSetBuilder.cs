using Examine;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Examine
{
    public class MediaValueSetBuilder : BaseValueSetBuilder<IMedia>
    {
        private readonly IEnumerable<IUrlSegmentProvider> _urlSegmentProviders;
        private readonly IUserService _userService;

        public MediaValueSetBuilder(PropertyEditorCollection propertyEditors,
            IEnumerable<IUrlSegmentProvider> urlSegmentProviders,
            IUserService userService)
            : base(propertyEditors)
        {
            _urlSegmentProviders = urlSegmentProviders;
            _userService = userService;
        }

        /// <inheritdoc />
        public override IEnumerable<ValueSet> GetValueSets(params IMedia[] media)
        {
            foreach (var m in media)
            {
                var urlValue = m.GetUrlSegment(_urlSegmentProviders);
                var values = new Dictionary<string, object[]>
                {
                    {"icon", new object[] {m.ContentType.Icon}},
                    {"id", new object[] {m.Id}},
                    {"key", new object[] {m.Key}},
                    {"parentID", new object[] {m.Level > 1 ? m.ParentId : -1}},
                    {"level", new object[] {m.Level}},
                    {"creatorID", new object[] {m.CreatorId}},
                    {"sortOrder", new object[] {m.SortOrder}},
                    {"createDate", new object[] {m.CreateDate}},
                    {"updateDate", new object[] {m.UpdateDate}},
                    {"nodeName", new object[] {m.Name}},
                    {"urlName", new object[] {urlValue}},
                    {"path", new object[] {m.Path}},
                    {"nodeType", new object[] {m.ContentType.Id}},
                    {"creatorName", new object[] {m.GetCreatorProfile(_userService).Name}}
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
