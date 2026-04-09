using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc cref="IAuditEntryService"/>
public class AuditEntryService : RepositoryService, IAuditEntryService
{
    private readonly IAuditEntryRepository _auditEntryRepository;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    /// <summary>
    ///    Initializes a new instance of the <see cref="AuditEntryService" /> class.
    /// </summary>
    /// <param name="auditEntryRepository">The audit entry repository.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
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
    public async Task<IAuditEntry> WriteAsync(
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

    /// <summary>
    /// Writes an audit entry using user IDs instead of keys.
    /// </summary>
    /// <param name="performingUserId">The ID of the user performing the action.</param>
    /// <param name="performingDetails">Details about the performing user.</param>
    /// <param name="performingIp">The IP address of the performing user.</param>
    /// <param name="eventDateUtc">The UTC date and time of the event.</param>
    /// <param name="affectedUserId">The ID of the affected user, if any.</param>
    /// <param name="affectedDetails">Details about the affected user.</param>
    /// <param name="eventType">The type of event.</param>
    /// <param name="eventDetails">Details about the event.</param>
    /// <returns>The created audit entry.</returns>
    /// <remarks>This method is used by the AuditService while the AuditService.Write() method is not removed.</remarks>
    internal async Task<IAuditEntry> WriteInner(
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

    /// <summary>
    /// Internal implementation that writes an audit entry with all required data.
    /// </summary>
    /// <param name="performingUserId">The ID of the user performing the action.</param>
    /// <param name="performingUserKey">The key of the user performing the action.</param>
    /// <param name="performingDetails">Details about the performing user.</param>
    /// <param name="performingIp">The IP address of the performing user.</param>
    /// <param name="eventDateUtc">The UTC date and time of the event.</param>
    /// <param name="affectedUserId">The ID of the affected user, if any.</param>
    /// <param name="affectedUserKey">The key of the affected user, if any.</param>
    /// <param name="affectedDetails">Details about the affected user.</param>
    /// <param name="eventType">The type of event.</param>
    /// <param name="eventDetails">Details about the event.</param>
    /// <returns>The created audit entry.</returns>
    private IAuditEntry WriteInner(
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
            EventDate = eventDateUtc,
            AffectedUserId = affectedUserId ?? Constants.Security.UnknownUserId, // Default to UnknownUserId as it is non-nullable
            AffectedUserKey = affectedUserKey,
            AffectedDetails = affectedDetails,
            EventType = eventType,
            EventDetails = eventDetails,
        };

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _auditEntryRepository.Save(entry);
            scope.Complete();
        }

        return entry;
    }

    /// <summary>
    /// Gets the user ID for a given user key.
    /// </summary>
    /// <param name="key">The user key to resolve.</param>
    /// <returns>The user ID if found; otherwise, <c>null</c>.</returns>
    internal async Task<int?> GetUserId(Guid? key) =>
        key is not null && await _userIdKeyResolver.TryGetAsync(key.Value) is { Success: true } attempt
            ? attempt.Result
            : null;

    /// <summary>
    /// Gets the user key for a given user ID.
    /// </summary>
    /// <param name="id">The user ID to resolve.</param>
    /// <returns>The user key if found; otherwise, <c>null</c>.</returns>
    internal async Task<Guid?> GetUserKey(int? id) =>
        id is not null && await _userIdKeyResolver.TryGetAsync(id.Value) is { Success: true } attempt
            ? attempt.Result
            : null;

    /// <summary>
    /// Gets all audit entries.
    /// </summary>
    /// <returns>A collection of all audit entries.</returns>
    /// <remarks>Currently used in testing only. This is not part of the interface; queryable methods should be added to the interface instead.</remarks>
    internal IEnumerable<IAuditEntry> GetAll()
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _auditEntryRepository.GetMany();
        }
    }
}
