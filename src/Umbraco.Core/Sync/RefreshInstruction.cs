using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Sync;

[Serializable]
public class RefreshInstruction
{
    // NOTE
    // that class should be refactored
    // but at the moment it is exposed in CacheRefresher webservice
    // so for the time being we keep it as-is for backward compatibility reasons

    // need this public, parameter-less constructor so the web service messenger
    // can de-serialize the instructions it receives

    /// <summary>
    ///     Initializes a new instance of the <see cref="CacheInstructionService" /> class.
    /// </summary>
    /// <remarks>
    ///     Need this public, parameter-less constructor so the web service messenger can de-serialize the instructions it
    ///     receives.
    /// </remarks>
    public RefreshInstruction() =>

        // Set default - this value is not used for reading after it's been deserialized, it's only used for persisting the instruction to the db
        JsonIdCount = 1;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CacheInstructionService" /> class.
    /// </summary>
    /// <remarks>
    ///     Need this public one so it can be de-serialized - used by the Json thing
    ///     otherwise, should use GetInstructions(...)
    /// </remarks>
    public RefreshInstruction(Guid refresherId, RefreshMethodType refreshType, Guid guidId, int intId, string jsonIds, string jsonPayload)
        : this()
    {
        RefresherId = refresherId;
        RefreshType = refreshType;
        GuidId = guidId;
        IntId = intId;
        JsonIds = jsonIds;
        JsonPayload = jsonPayload;
    }

    private RefreshInstruction(ICacheRefresher refresher, RefreshMethodType refreshType)
        : this()
    {
        RefresherId = refresher.RefresherUniqueId;
        RefreshType = refreshType;
    }

    private RefreshInstruction(ICacheRefresher refresher, RefreshMethodType refreshType, Guid guidId)
        : this(refresher, refreshType) => GuidId = guidId;

    private RefreshInstruction(ICacheRefresher refresher, RefreshMethodType refreshType, int intId)
        : this(refresher, refreshType) => IntId = intId;

    /// <summary>
    ///     A private constructor to create a new instance
    /// </summary>
    /// <param name="idCount">
    ///     When the refresh method is <see cref="RefreshMethodType.RefreshByIds" /> we know how many Ids are being refreshed
    ///     so we know the instruction
    ///     count which will be taken into account when we store this count in the database.
    /// </param>
    private RefreshInstruction(ICacheRefresher refresher, RefreshMethodType refreshType, string? json, int idCount = 1)
        : this(refresher, refreshType)
    {
        JsonIdCount = idCount;

        if (refreshType == RefreshMethodType.RefreshByJson)
        {
            JsonPayload = json;
        }
        else
        {
            JsonIds = json;
        }
    }

    /// <summary>
    ///     Gets or sets the refresh action type.
    /// </summary>
    public RefreshMethodType RefreshType { get; set; }

    /// <summary>
    ///     Gets or sets the refresher unique identifier.
    /// </summary>
    public Guid RefresherId { get; set; }

    /// <summary>
    ///     Gets or sets the Guid data value.
    /// </summary>
    public Guid GuidId { get; set; }

    /// <summary>
    ///     Gets or sets the int data value.
    /// </summary>
    public int IntId { get; set; }

    /// <summary>
    ///     Gets or sets the ids data value.
    /// </summary>
    public string? JsonIds { get; set; }

    /// <summary>
    ///     Gets or sets the number of Ids contained in the JsonIds json value.
    /// </summary>
    /// <remarks>
    ///     This is used to determine the instruction count per row.
    /// </remarks>
    public int JsonIdCount { get; set; }

    /// <summary>
    ///     Gets or sets the payload data value.
    /// </summary>
    public string? JsonPayload { get; set; }

    public static bool operator ==(RefreshInstruction left, RefreshInstruction right) => Equals(left, right);

    public static IEnumerable<RefreshInstruction> GetInstructions(
        ICacheRefresher refresher,
        IJsonSerializer jsonSerializer,
        MessageType messageType,
        IEnumerable<object>? ids,
        Type? idType,
        string? json)
    {
        switch (messageType)
        {
            case MessageType.RefreshAll:
                return new[] { new RefreshInstruction(refresher, RefreshMethodType.RefreshAll) };

            case MessageType.RefreshByJson:
                return new[] { new RefreshInstruction(refresher, RefreshMethodType.RefreshByJson, json) };

            case MessageType.RefreshById:
                if (idType == null)
                {
                    throw new InvalidOperationException("Cannot refresh by id if idType is null.");
                }

                if (idType == typeof(int))
                {
                    // Bulk of ints is supported
                    var intIds = ids?.Cast<int>().ToArray();
                    return new[]
                    {
                        new RefreshInstruction(refresher, RefreshMethodType.RefreshByIds, jsonSerializer.Serialize(intIds), intIds?.Length ?? 0),
                    };
                }

                // Else must be guids, bulk of guids is not supported, so iterate.
                return ids?.Select(x => new RefreshInstruction(refresher, RefreshMethodType.RefreshByGuid, (Guid)x)) ??
                       Enumerable.Empty<RefreshInstruction>();

            case MessageType.RemoveById:
                if (idType == null)
                {
                    throw new InvalidOperationException("Cannot remove by id if idType is null.");
                }

                // Must be ints, bulk-remove is not supported, so iterate.
                return ids?.Select(x => new RefreshInstruction(refresher, RefreshMethodType.RemoveById, (int)x)) ??
                       Enumerable.Empty<RefreshInstruction>();

            // return new[] { new RefreshInstruction(refresher, RefreshMethodType.RemoveByIds, JsonConvert.SerializeObject(ids.Cast<int>().ToArray())) };
            default:
                // case MessageType.RefreshByInstance:
                // case MessageType.RemoveByInstance:
                throw new ArgumentOutOfRangeException("messageType");
        }
    }

    public override bool Equals(object? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other.GetType() != GetType())
        {
            return false;
        }

        return Equals((RefreshInstruction)other);
    }

    protected bool Equals(RefreshInstruction other) =>
        RefreshType == other.RefreshType
        && RefresherId.Equals(other.RefresherId)
        && GuidId.Equals(other.GuidId)
        && IntId == other.IntId
        && string.Equals(JsonIds, other.JsonIds)
        && string.Equals(JsonPayload, other.JsonPayload);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int)RefreshType;
            hashCode = (hashCode * 397) ^ RefresherId.GetHashCode();
            hashCode = (hashCode * 397) ^ GuidId.GetHashCode();
            hashCode = (hashCode * 397) ^ IntId;
            hashCode = (hashCode * 397) ^ (JsonIds != null ? JsonIds.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (JsonPayload != null ? JsonPayload.GetHashCode() : 0);
            return hashCode;
        }
    }

    public static bool operator !=(RefreshInstruction left, RefreshInstruction right) => Equals(left, right) == false;
}
