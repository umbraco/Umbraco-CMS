using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Manages <see cref="IMemberType"/> objects.
    /// </summary>
    public interface IMemberTypeService : IContentTypeServiceBase<IMemberType>
    {
        string GetDefault();
    }
}
