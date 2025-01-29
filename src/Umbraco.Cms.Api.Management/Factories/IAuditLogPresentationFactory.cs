using Umbraco.Cms.Api.Management.ViewModels.AuditLog;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IAuditLogPresentationFactory
{
    IEnumerable<AuditLogResponseModel> CreateAuditLogViewModel(IEnumerable<IAuditItem> auditItems);
}
