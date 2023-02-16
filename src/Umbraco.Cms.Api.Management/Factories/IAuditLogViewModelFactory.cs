using Umbraco.Cms.Api.Management.ViewModels.AuditLogs;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IAuditLogViewModelFactory
{
    IEnumerable<AuditlogViewModel> CreateAuditLogViewModel(IEnumerable<IAuditItem> auditItems);

    IEnumerable<AuditLogWithUsernameViewModel> CreateAuditLogWithUsernameViewModels(IEnumerable<IAuditItem> auditItems);
}
