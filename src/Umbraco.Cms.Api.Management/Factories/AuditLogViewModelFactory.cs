using Umbraco.Cms.Api.Management.ViewModels.AuditLogs;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class AuditLogViewModelFactory : IAuditLogViewModelFactory
{
    private readonly IUmbracoMapper _umbracoMapper;

    public AuditLogViewModelFactory(IUmbracoMapper umbracoMapper)
    {
        _umbracoMapper = umbracoMapper;
    }

    public IEnumerable<AuditLogByTypeViewModel> CreateAuditLogByTypeViewModel(IEnumerable<IAuditItem> auditItems) => auditItems.Select(CreateAuditLogByTypeViewModel);

    private AuditLogByTypeViewModel CreateAuditLogByTypeViewModel(IAuditItem auditItem)
    {
        AuditLogByTypeViewModel auditLogByTypeViewModel = _umbracoMapper.Map<AuditLogByTypeViewModel>(auditItem)!;
        // this is just faking until we get keys on users
        // FIXME: Get user from userservice and then use the key.
        auditLogByTypeViewModel.UserKey = Guid.NewGuid();
        return auditLogByTypeViewModel;
    }
}
