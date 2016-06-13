using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a ContentType, which Media is based on
    /// </summary
    public interface ISchemaType : IContentTypeComposition
    {

        /// <summary>
        /// Creates a deep clone of the current entity with its identity/alias and it's property identities reset
        /// </summary>
        /// <param name="newAlias"></param>
        /// <returns></returns>
        ISchemaType DeepCloneWithResetIdentities(string newAlias);
    }
}