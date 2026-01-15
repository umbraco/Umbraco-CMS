using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents an audited event.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class AuditEntry : EntityBase, IAuditEntry
{
    private string? _affectedDetails;
    private int _affectedUserId;
    private Guid? _affectedUserKey;
    private string? _eventDetails;
    private string? _eventType;
    private string? _performingDetails;
    private string? _performingIp;
    private int _performingUserId;
    private Guid? _performingUserKey;

    /// <inheritdoc />
    public int PerformingUserId
    {
        get => _performingUserId;
        set => SetPropertyValueAndDetectChanges(value, ref _performingUserId, nameof(PerformingUserId));
    }

    /// <inheritdoc />
    public Guid? PerformingUserKey
    {
        get => _performingUserKey;
        set => SetPropertyValueAndDetectChanges(value, ref _performingUserKey, nameof(PerformingUserKey));
    }

    /// <inheritdoc />
    public string? PerformingDetails
    {
        get => _performingDetails;
        set => SetPropertyValueAndDetectChanges(value, ref _performingDetails, nameof(PerformingDetails));
    }

    /// <inheritdoc />
    public string? PerformingIp
    {
        get => _performingIp;
        set => SetPropertyValueAndDetectChanges(value, ref _performingIp, nameof(PerformingIp));
    }

    /// <inheritdoc />
    public DateTime EventDate
    {
        get => CreateDate;
        set => CreateDate = value;
    }

    /// <inheritdoc />
    public int AffectedUserId
    {
        get => _affectedUserId;
        set => SetPropertyValueAndDetectChanges(value, ref _affectedUserId, nameof(AffectedUserId));
    }

    /// <inheritdoc />
    public Guid? AffectedUserKey
    {
        get => _affectedUserKey;
        set => SetPropertyValueAndDetectChanges(value, ref _affectedUserKey, nameof(AffectedUserKey));
    }

    /// <inheritdoc />
    public string? AffectedDetails
    {
        get => _affectedDetails;
        set => SetPropertyValueAndDetectChanges(value, ref _affectedDetails, nameof(AffectedDetails));
    }

    /// <inheritdoc />
    public string? EventType
    {
        get => _eventType;
        set => SetPropertyValueAndDetectChanges(value, ref _eventType, nameof(EventType));
    }

    /// <inheritdoc />
    public string? EventDetails
    {
        get => _eventDetails;
        set => SetPropertyValueAndDetectChanges(value, ref _eventDetails, nameof(EventDetails));
    }
}
