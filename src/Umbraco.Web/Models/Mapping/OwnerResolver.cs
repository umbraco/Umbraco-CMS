using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Maps the Owner for IContentBase
    /// </summary>
    /// <typeparam name="TPersisted"></typeparam>
    internal class OwnerResolver<TPersisted> : ValueResolver<TPersisted, UserBasic>
        where TPersisted : IContentBase
    {
        protected override UserBasic ResolveCore(TPersisted source)
        {
            return Mapper.Map<IProfile, UserBasic>(source.GetCreatorProfile());
        }
    }
}