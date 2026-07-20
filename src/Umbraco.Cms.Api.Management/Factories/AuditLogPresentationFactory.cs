using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.AuditLog;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods to create presentation models for audit logs.
/// </summary>
public class AuditLogPresentationFactory : IAuditLogPresentationFactory
{
    private readonly IUserService _userService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogPresentationFactory"/> class.
    /// </summary>
    /// <param name="userService">Service for retrieving user information.</param>
    /// <param name="userIdKeyResolver">Resolves user identifiers to their corresponding keys.</param>
    public AuditLogPresentationFactory(IUserService userService, IUserIdKeyResolver userIdKeyResolver)
    {
        _userService = userService;
        _userIdKeyResolver = userIdKeyResolver;
    }

    /// <summary>
    /// Creates a collection of <see cref="AuditLogResponseModel"/> from the given audit items.
    /// </summary>
    /// <param name="auditItems">The audit items to convert.</param>
    /// <returns>An enumerable of <see cref="AuditLogResponseModel"/> representing the audit log entries.</returns>
    public IEnumerable<AuditLogResponseModel> CreateAuditLogViewModel(IEnumerable<IAuditItem> auditItems) => auditItems.Select(CreateAuditLogViewModel);

    private AuditLogResponseModel CreateAuditLogViewModel(IAuditItem auditItem) =>
        new()
        {
            Comment = auditItem.Comment,
            LogType = auditItem.AuditType,
            Parameters = auditItem.Parameters,
            Timestamp = auditItem.CreateDate,
            User = auditItem.UserId switch
            {
                Constants.Security.UnknownUserId => new ReferenceByIdModel(),
                _ => new ReferenceByIdModel(_userIdKeyResolver.GetAsync(auditItem.UserId).GetAwaiter().GetResult()),
            },
        };
}
