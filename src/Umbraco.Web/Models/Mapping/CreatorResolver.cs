using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using UserProfile = Umbraco.Web.Models.ContentEditing.UserProfile;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Maps the Creator for content
    /// </summary>
    internal class CreatorResolver : ValueResolver<IContent, UserProfile>
    {
        protected override UserProfile ResolveCore(IContent source)
        {
            return Mapper.Map<IProfile, UserProfile>(source.GetWriterProfile());
        }
    }
}