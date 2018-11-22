using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Resolves a UDI for a content type based on it's type
    /// </summary>
    internal class ContentTypeUdiResolver : ValueResolver<IContentTypeComposition, Udi>
    {
        protected override Udi ResolveCore(IContentTypeComposition source)
        {
            if (source == null) return null;

            return Udi.Create(
                source.GetType() == typeof(IMemberType)
                    ? Constants.UdiEntityType.MemberType
                    : source.GetType() == typeof(IMediaType)
                        ? Constants.UdiEntityType.MediaType : Constants.UdiEntityType.DocumentType, source.Key);
        }
    }
}