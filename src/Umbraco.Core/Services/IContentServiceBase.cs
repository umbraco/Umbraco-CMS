using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Placeholder for sharing logic between the content, media (and member) services
    /// TODO: Start sharing the logic!
    /// </summary>
    public interface IContentServiceBase : IService
    {
        /// <summary>
        /// Checks/fixes the data integrity of node paths/levels stored in the database
        /// </summary>
        ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options);
    }
}
