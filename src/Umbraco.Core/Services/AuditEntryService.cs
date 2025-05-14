// <copyright file="AuditEntryService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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
    private readonly Lazy<bool> _isAvailable;

    /// <summary>
    ///    Initializes a new instance of the <see cref="AuditEntryService" /> class.
    /// </summary>
    public AuditEntryService(
        IAuditEntryRepository auditEntryRepository,
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _auditEntryRepository = auditEntryRepository;
        _isAvailable = new Lazy<bool>(DetermineIsAvailable);
    }

    /// <inheritdoc />
    public Task<Attempt<IAuditEntry, AuditEntryOperationStatus>> WriteAsync(
        int performingUserId,
        string perfomingDetails,
        string performingIp,
        DateTime eventDateUtc,
        int affectedUserId,
        string? affectedDetails,
        string eventType,
        string eventDetails) =>
        Task.FromResult(
            WriteInner(
                performingUserId,
                perfomingDetails,
                performingIp,
                eventDateUtc,
                affectedUserId,
                affectedDetails,
                eventType,
                eventDetails));

    // This method is used by the AuditService while the AuditService.Write() method is not removed.
    internal Attempt<IAuditEntry, AuditEntryOperationStatus> WriteInner(
        int performingUserId,
        string performingDetails,
        string performingIp,
        DateTime eventDateUtc,
        int affectedUserId,
        string? affectedDetails,
        string eventType,
        string eventDetails)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(performingUserId, Constants.Security.SuperUserId);

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
            PerformingUserId = performingUserId,
            PerformingDetails = performingDetails,
            PerformingIp = performingIp,
            EventDateUtc = eventDateUtc,
            AffectedUserId = affectedUserId,
            AffectedDetails = affectedDetails,
            EventType = eventType,
            EventDetails = eventDetails,
        };

        if (_isAvailable.Value == false)
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


    // TODO: Currently used in testing only, not part of the interface, need to add queryable methods to the interface instead
    internal IEnumerable<IAuditEntry> GetAll()
    {
        if (_isAvailable.Value == false)
        {
            return [];
        }

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _auditEntryRepository.GetMany();
        }
    }

    /// <summary>
    ///     Determines whether the repository is available.
    /// </summary>
    private bool DetermineIsAvailable()
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _auditEntryRepository.IsAvailable();
        }
    }
}
