using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models
{
    public interface IMember : IContentBase, IMembershipUser, IHaveAdditionalData
    {
        /// <summary>
        /// String alias of the default ContentType
        /// </summary>
        string ContentTypeAlias { get; }
    }
}
