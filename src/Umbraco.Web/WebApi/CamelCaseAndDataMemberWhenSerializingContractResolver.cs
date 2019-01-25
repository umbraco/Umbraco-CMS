using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using Umbraco.Core.Manifest;

namespace Umbraco.Web.WebApi
{

    public class CamelCaseAndDataMemberWhenSerializingContractResolver : CamelCasePropertyNamesContractResolver
    {
        /// <inheritdoc/>
        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            return base.GetSerializableMembers(objectType)
                .Where(HasNotTheIgnoreDataMemberWhenSerializingAttribute)
                .ToList();
        }

        private bool HasNotTheIgnoreDataMemberWhenSerializingAttribute(MemberInfo member)
        {
            if (member.GetCustomAttributes(typeof(IgnoreDataMemberWhenSerializingAttribute), true).Any())
            {
                return false;
            }

            return true;
        }
    }
}
