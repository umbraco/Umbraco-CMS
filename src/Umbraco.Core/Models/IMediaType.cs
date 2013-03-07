using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a ContentType, which Media is based on
    /// </summary>
    [Mapper(typeof(MediaTypeMapper))]
    public interface IMediaType : IContentTypeComposition
    {
         
    }
}