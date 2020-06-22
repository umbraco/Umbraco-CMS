using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Umbraco.Web.Common.Formatters
{
    public class IgnoreRequiredAttributsResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);


            property.Required = Required.Default;

            return property;
        }
    }
}
