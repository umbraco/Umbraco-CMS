using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Maps the Creator for content
    /// </summary>
    internal class CreatorResolver : ValueResolver<IContent, UserBasic>
    {
        protected override UserBasic ResolveCore(IContent source)
        {
            return Mapper.Map<IProfile, UserBasic>(source.GetWriterProfile());
        }
    }
}