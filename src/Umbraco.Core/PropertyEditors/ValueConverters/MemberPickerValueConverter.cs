using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class MemberPickerValueConverter : PropertyValueConverterBase
    {
        private readonly IMemberService _memberService;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public MemberPickerValueConverter(
            IMemberService memberService,
            IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IUmbracoContextAccessor umbracoContextAccessor)
        {
            _memberService = memberService;
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.MemberPicker);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Snapshot;

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof (IPublishedContent);

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;

            var attemptConvertInt = source.TryConvertTo<int>();
            if (attemptConvertInt.Success)
                return attemptConvertInt.Result;
            var attemptConvertUdi = source.TryConvertTo<Udi>();
            if (attemptConvertUdi.Success)
                return attemptConvertUdi.Result;
            return null;
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            if (_umbracoContextAccessor.UmbracoContext != null)
            {
                IPublishedContent member;
                if (source is int id)
                {
                    IMember m = _memberService.GetById(id);
                    if (m == null)
                    {
                        return null;
                    }
                    member = _publishedSnapshotAccessor.PublishedSnapshot?.Members.Get(m);
                    if (member != null)
                    {
                        return member;
                    }
                }
                else
                {
                    var sourceUdi = source as GuidUdi;
                    if (sourceUdi == null) return null;

                    IMember m = _memberService.GetByKey(sourceUdi.Guid);
                    if (m == null)
                    {
                        return null;
                    }

                    member = _publishedSnapshotAccessor.PublishedSnapshot?.Members.Get(m);

                    if (member != null)
                    {
                        return member;
                    }
                }
            }

            return source;
        }
    }
}
