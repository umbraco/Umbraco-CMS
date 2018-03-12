using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using UserProfile = Umbraco.Web.Models.ContentEditing.UserProfile;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Maps the Owner for IContentBase
    /// </summary>
    /// <typeparam name="TPersisted"></typeparam>
    internal class OwnerResolver<TPersisted> : ValueResolver<TPersisted, UserProfile>
        where TPersisted : IContentBase
    {
        protected override UserProfile ResolveCore(TPersisted source)
        {
            return Mapper.Map<IProfile, UserProfile>(source.GetCreatorProfile());
        }
    }
}