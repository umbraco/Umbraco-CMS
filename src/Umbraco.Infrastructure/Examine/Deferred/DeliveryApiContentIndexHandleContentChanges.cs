using Examine;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine.Deferred;

internal sealed class DeliveryApiContentIndexHandleContentChanges : DeliveryApiContentIndexDeferredBase, IDeferredAction
{
    private readonly IList<KeyValuePair<int, TreeChangeTypes>> _changes;
    private readonly IContentService _contentService;
    private readonly DeliveryApiIndexingHandler _deliveryApiIndexingHandler;
    private readonly IDeliveryApiContentIndexValueSetBuilder _deliveryApiContentIndexValueSetBuilder;
    private readonly IDeliveryApiContentIndexHelper _deliveryApiContentIndexHelper;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public DeliveryApiContentIndexHandleContentChanges(
        IList<KeyValuePair<int, TreeChangeTypes>> changes,
        DeliveryApiIndexingHandler deliveryApiIndexingHandler,
        IContentService contentService,
        IDeliveryApiContentIndexValueSetBuilder deliveryApiContentIndexValueSetBuilder,
        IDeliveryApiContentIndexHelper deliveryApiContentIndexHelper,
        IBackgroundTaskQueue backgroundTaskQueue)
    {
        _changes = changes;
        _deliveryApiIndexingHandler = deliveryApiIndexingHandler;
        _contentService = contentService;
        _backgroundTaskQueue = backgroundTaskQueue;
        _deliveryApiContentIndexValueSetBuilder = deliveryApiContentIndexValueSetBuilder;
        _deliveryApiContentIndexHelper = deliveryApiContentIndexHelper;
    }

    public void Execute() => _backgroundTaskQueue.QueueBackgroundWorkItem(_ =>
    {
        IIndex index = _deliveryApiIndexingHandler.GetIndex()
                       ?? throw new InvalidOperationException("Could not obtain the delivery API content index");

        var pendingRemovals = new List<int>();
        foreach ((int contentId, TreeChangeTypes changeTypes) in _changes)
        {
            var remove = changeTypes.HasType(TreeChangeTypes.Remove);
            var reindex = changeTypes.HasType(TreeChangeTypes.RefreshNode) || changeTypes.HasType(TreeChangeTypes.RefreshBranch);

            if (remove)
            {
                pendingRemovals.Add(contentId);
            }
            else if (reindex)
            {
                IContent? content = _contentService.GetById(contentId);
                if (content == null || content.Trashed)
                {
                    pendingRemovals.Add(contentId);
                    continue;
                }

                RemoveFromIndex(pendingRemovals, index);
                pendingRemovals.Clear();

                Reindex(content, index);
            }
        }

        RemoveFromIndex(pendingRemovals, index);

        return Task.CompletedTask;
    });

    private void Reindex(IContent content, IIndex index)
    {
        // get the currently indexed cultures for the content
        CulturePublishStatus[] existingCultures = index
            .Searcher
            .CreateQuery()
            .Field(UmbracoExamineFieldNames.DeliveryApiContentIndex.Id, content.Id.ToString())
            .SelectFields(new HashSet<string>
            {
                UmbracoExamineFieldNames.DeliveryApiContentIndex.Culture,
                UmbracoExamineFieldNames.DeliveryApiContentIndex.Published
            })
            .Execute()
            .Select(f => new CulturePublishStatus
            {
                Culture = f.GetValues(UmbracoExamineFieldNames.DeliveryApiContentIndex.Culture).Single(),
                Published = f.GetValues(UmbracoExamineFieldNames.DeliveryApiContentIndex.Published).Single()
            })
            .ToArray();

        // index the content
        CulturePublishStatus[] indexedCultures = UpdateIndex(content, index);
        if (indexedCultures.Any() is false)
        {
            // we likely got here because a removal triggered a "refresh branch" notification, now we
            // need to delete every last culture of this content and all descendants
            RemoveFromIndex(content.Id, index);
            return;
        }

        // if the published state changed of any culture, chances are there are similar changes ot the content descendants
        // that need to be reflected in the index, so we'll reindex all descendants
        var changedCulturePublishStatus = indexedCultures.Intersect(existingCultures).Count() != existingCultures.Length;
        if (changedCulturePublishStatus)
        {
            ReindexDescendants(content, index);
        }
    }

    private CulturePublishStatus[] UpdateIndex(IContent content, IIndex index)
    {
        ValueSet[] valueSets = _deliveryApiContentIndexValueSetBuilder.GetValueSets(content).ToArray();
        if (valueSets.Any() is false)
        {
            return Array.Empty<CulturePublishStatus>();
        }

        index.IndexItems(valueSets);
        return valueSets
            .Select(v => new CulturePublishStatus
            {
                Culture = v.GetValue(UmbracoExamineFieldNames.DeliveryApiContentIndex.Culture).ToString()!,
                Published = v.GetValue(UmbracoExamineFieldNames.DeliveryApiContentIndex.Published).ToString()!
            })
            .ToArray();
    }

    private void ReindexDescendants(IContent content, IIndex index)
        => _deliveryApiContentIndexHelper.EnumerateApplicableDescendantsForContentIndex(
            content.Id,
            descendants =>
            {
                foreach (IContent descendant in descendants)
                {
                    UpdateIndex(descendant, index);
                }
            });

    private class CulturePublishStatus : IEquatable<CulturePublishStatus>
    {
        public required string Culture { get; set; }

        public required string Published { get; set; }

        public bool Equals(CulturePublishStatus? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Culture == other.Culture && Published == other.Published;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((CulturePublishStatus)obj);
        }

        public override int GetHashCode() => HashCode.Combine(Culture, Published);
    }
}
