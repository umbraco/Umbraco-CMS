using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// A resolver to map <see cref="IMember"/> properties to a collection of <see cref="ContentPropertyBasic"/>
    /// </summary>
    internal class MemberBasicPropertiesResolver : IValueResolver<IMember, MemberBasic, IEnumerable<ContentPropertyBasic>>
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public MemberBasicPropertiesResolver(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new System.ArgumentNullException(nameof(umbracoContextAccessor));
        }

        public IEnumerable<ContentPropertyBasic> Resolve(IMember source, MemberBasic destination, IEnumerable<ContentPropertyBasic> destMember, ResolutionContext context)
        {
            var umbracoContext = _umbracoContextAccessor.UmbracoContext;
            if (umbracoContext == null) throw new InvalidOperationException("Cannot resolve value without an UmbracoContext available");

            var result = Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyBasic>>(
                    // Sort properties so items from different compositions appear in correct order (see U4-9298). Map sorted properties.
                    source.Properties.OrderBy(prop => prop.PropertyType.SortOrder))
                .ToList();

            var memberType = source.ContentType;

            //now update the IsSensitive value
            foreach (var prop in result)
            {
                //check if this property is flagged as sensitive
                var isSensitiveProperty = memberType.IsSensitiveProperty(prop.Alias);
                //check permissions for viewing sensitive data
                if (isSensitiveProperty && umbracoContext.Security.CurrentUser.HasAccessToSensitiveData() == false)
                {
                    //mark this property as sensitive
                    prop.IsSensitive = true;
                    //clear the value
                    prop.Value = null;
                }
            }
            return result;
        }
    }
}
