using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IAuditService : IService
    {
        void Add(AuditType type, string comment, int userId, int objectId);
    }
}