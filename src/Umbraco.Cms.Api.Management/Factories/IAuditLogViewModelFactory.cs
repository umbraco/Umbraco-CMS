using Umbraco.Cms.Api.Management.ViewModels.AuditLogs;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IAuditLogViewModelFactory
{
    IEnumerable<AuditLogByTypeViewModel> CreateAuditLogByTypeViewModel(IEnumerable<IAuditItem> auditItems);
}
