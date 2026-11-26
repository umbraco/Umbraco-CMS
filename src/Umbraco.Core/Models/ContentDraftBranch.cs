using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <inheritdoc cref="IContentDraftBranch" />
[Serializable]
[DataContract(IsReference = true)]
public class ContentDraftBranch : EntityBase, IContentDraftBranch
{
    private static readonly DelegateEqualityComparer<ISet<string>> ScheduledCulturesComparer =
        new(
            (x, y) => x.UnsortedSequenceEqual(y),
            x => x.GetHashCode());

    private string _name;
    private DateTime? _scheduledPublishDate;
    private ISet<string> _scheduledCultures = new HashSet<string>();

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentDraftBranch" /> class.
    /// </summary>
    /// <param name="nodeId">The id of the node this draft branch belongs to.</param>
    /// <param name="contentKey">The key of the content item this draft branch belongs to.</param>
    /// <param name="name">The editor-facing name of the draft branch.</param>
    /// <param name="basedOnVersionId">
    /// The id of the content version that was current on the node's default draft when this branch was forked.
    /// </param>
    /// <param name="createdByUserId">The id of the user who created this draft branch.</param>
    public ContentDraftBranch(int nodeId, Guid contentKey, string name, int? basedOnVersionId, int? createdByUserId)
    {
        NodeId = nodeId;
        ContentKey = contentKey;
        _name = name;
        BasedOnVersionId = basedOnVersionId;
        CreatedByUserId = createdByUserId;
    }

    /// <inheritdoc />
    public int NodeId { get; }

    /// <inheritdoc />
    public Guid ContentKey { get; }

    /// <inheritdoc />
    [DataMember]
    public string Name
    {
        get => _name;
        set => SetPropertyValueAndDetectChanges(value, ref _name!, nameof(Name));
    }

    /// <inheritdoc />
    public int? CreatedByUserId { get; }

    /// <inheritdoc />
    public int? BasedOnVersionId { get; }

    /// <inheritdoc />
    [DataMember]
    public DateTime? ScheduledPublishDate
    {
        get => _scheduledPublishDate;
        set => SetPropertyValueAndDetectChanges(value, ref _scheduledPublishDate, nameof(ScheduledPublishDate));
    }

    /// <inheritdoc />
    [DataMember]
    public ISet<string> ScheduledCultures
    {
        get => _scheduledCultures;
        set => SetPropertyValueAndDetectChanges(value, ref _scheduledCultures!, nameof(ScheduledCultures), ScheduledCulturesComparer);
    }
}
