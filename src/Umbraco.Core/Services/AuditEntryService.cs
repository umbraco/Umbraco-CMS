using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc cref="IAuditEntryService"/>
public class AuditEntryService : RepositoryService, IAuditEntryService
{
    private readonly IAuditEntryRepository _auditEntryRepository;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly Lock _isAvailableLock = new();
    private bool _isAvailable;

    /// <summary>
    ///    Initializes a new instance of the <see cref="AuditEntryService" /> class.
    /// </summary>
    public AuditEntryService(
        IAuditEntryRepository auditEntryRepository,
        IUserIdKeyResolver userIdKeyResolver,
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _auditEntryRepository = auditEntryRepository;
        _userIdKeyResolver = userIdKeyResolver;
    }

    /// <inheritdoc />
    public async Task<Attempt<IAuditEntry, AuditEntryOperationStatus>> WriteAsync(
        Guid? performingUserKey,
        string performingDetails,
        string performingIp,
        DateTime eventDateUtc,
        Guid? affectedUserKey,
        string? affectedDetails,
        string eventType,
        string eventDetails)
    {
        var performingUserId = await GetUserId(performingUserKey);
        var affectedUserId = await GetUserId(affectedUserKey);

        return WriteInner(
            performingUserId,
            performingUserKey,
            performingDetails,
            performingIp,
            eventDateUtc,
            affectedUserId,
            affectedUserKey,
            affectedDetails,
            eventType,
            eventDetails);
    }

    /// <inheritdoc />
    [Obsolete("Use the overload that takes user keys. Scheduled for removal in Umbraco 19.")]
    public async Task<Attempt<IAuditEntry, AuditEntryOperationStatus>> WriteAsync(
        int? performingUserId,
        string performingDetails,
        string performingIp,
        DateTime eventDateUtc,
        int? affectedUserId,
        string? affectedDetails,
        string eventType,
        string eventDetails) =>
        await WriteInner(
            performingUserId,
            performingDetails,
            performingIp,
            eventDateUtc,
            affectedUserId,
            affectedDetails,
            eventType,
            eventDetails);

    // This method is used by the AuditService while the AuditService.Write() method is not removed.
    internal async Task<Attempt<IAuditEntry, AuditEntryOperationStatus>> WriteInner(
        int? performingUserId,
        string performingDetails,
        string performingIp,
        DateTime eventDateUtc,
        int? affectedUserId,
        string? affectedDetails,
        string eventType,
        string eventDetails)
    {
        Guid? performingUserKey = await GetUserKey(performingUserId);
        Guid? affectedUserKey = await GetUserKey(affectedUserId);

        return WriteInner(
            performingUserId,
            performingUserKey,
            performingDetails,
            performingIp,
            eventDateUtc,
            affectedUserId,
            affectedUserKey,
            affectedDetails,
            eventType,
            eventDetails);
    }

    private Attempt<IAuditEntry, AuditEntryOperationStatus> WriteInner(
        int? performingUserId,
        Guid? performingUserKey,
        string performingDetails,
        string performingIp,
        DateTime eventDateUtc,
        int? affectedUserId,
        Guid? affectedUserKey,
        string? affectedDetails,
        string eventType,
        string eventDetails)
    {
        if (performingUserId < Constants.Security.SuperUserId)
        {
            throw new ArgumentOutOfRangeException(nameof(performingUserId));
        }

        if (string.IsNullOrWhiteSpace(performingDetails))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(performingDetails));
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(eventType));
        }

        if (string.IsNullOrWhiteSpace(eventDetails))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(eventDetails));
        }

        // we need to truncate the data else we'll get SQL errors
        affectedDetails =
            affectedDetails?[..Math.Min(affectedDetails.Length, Constants.Audit.DetailsLength)];
        eventDetails = eventDetails[..Math.Min(eventDetails.Length, Constants.Audit.DetailsLength)];

        // validate the eventType - must contain a forward slash, no spaces, no special chars
        var eventTypeParts = eventType.ToCharArray();
        if (eventTypeParts.Contains('/') == false ||
            eventTypeParts.All(c => char.IsLetterOrDigit(c) || c == '/' || c == '-') == false)
        {
            throw new ArgumentException(
                nameof(eventType) + " must contain only alphanumeric characters, hyphens and at least one '/' defining a category");
        }

        if (eventType.Length > Constants.Audit.EventTypeLength)
        {
            throw new ArgumentException($"Must be max {Constants.Audit.EventTypeLength} chars.", nameof(eventType));
        }

        if (performingIp is { Length: > Constants.Audit.IpLength })
        {
            throw new ArgumentException($"Must be max {Constants.Audit.EventTypeLength} chars.", nameof(performingIp));
        }

        var entry = new AuditEntry
        {
            PerformingUserId = performingUserId ?? Constants.Security.UnknownUserId, // Default to UnknownUserId as it is non-nullable
            PerformingUserKey = performingUserKey,
            PerformingDetails = performingDetails,
            PerformingIp = performingIp,
            EventDateUtc = eventDateUtc,
            AffectedUserId = affectedUserId ?? Constants.Security.UnknownUserId, // Default to UnknownUserId as it is non-nullable
            AffectedUserKey = affectedUserKey,
            AffectedDetails = affectedDetails,
            EventType = eventType,
            EventDetails = eventDetails,
        };

        if (!IsAvailable())
        {
            return Attempt.FailWithStatus(AuditEntryOperationStatus.RepositoryNotAvailable, (IAuditEntry)entry);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _auditEntryRepository.Save(entry);
            scope.Complete();
        }

        return Attempt.SucceedWithStatus(AuditEntryOperationStatus.Success, (IAuditEntry)entry);
    }

    internal async Task<int?> GetUserId(Guid? key) =>
        key is not null && await _userIdKeyResolver.TryGetAsync(key.Value) is { Success: true } attempt
            ? attempt.Result
            : null;

    internal async Task<Guid?> GetUserKey(int? id) =>
        id is not null && await _userIdKeyResolver.TryGetAsync(id.Value) is { Success: true } attempt
            ? attempt.Result
            : null;

    // TODO: Currently used in testing only, not part of the interface, need to add queryable methods to the interface instead
    internal IEnumerable<IAuditEntry> GetAll()
    {
        if (!IsAvailable())
        {
            return [];
        }

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _auditEntryRepository.GetMany();
        }
    }

    private bool IsAvailable()
    {
        if (_isAvailable)
        {
            return true;
        }

        lock (_isAvailableLock)
        {
            if (_isAvailable)
            {
                return true;
            }

            using (ScopeProvider.CreateCoreScope(autoComplete: true))
            {
                _isAvailable = _auditEntryRepository.IsAvailable();
            }
        }

        return _isAvailable;
    }
}
