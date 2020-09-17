using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.PublishedCache.NuCache
{
    internal class SnapshotGetStrategy
    {
        private readonly IContentSnapshotAccessor _snapshot;
        private readonly IVariationContextAccessor _variationContextAccessor;

        public SnapshotGetStrategy(IContentSnapshotAccessor snapshot, IVariationContextAccessor variationContextAccessor)
        {
            _snapshot = snapshot;
            _variationContextAccessor = variationContextAccessor;
        }
        public IPublishedContent GetById(bool preview, int contentId)
        {
            var node = _snapshot.GetContentSnapshot().Get(contentId);
            return GetNodePublishedContent(node, preview);
        }

        public IPublishedContent GetById(bool preview, Guid contentId)
        {
            var node = _snapshot.GetContentSnapshot().Get(contentId);
            return GetNodePublishedContent(node, preview);
        }
        public IPublishedContent GetById(bool preview, Udi contentId)
        {
            var guidUdi = contentId as GuidUdi;
            if (guidUdi == null)
                throw new ArgumentException($"Udi must be of type {typeof(GuidUdi).Name}.", nameof(contentId));

            if (guidUdi.EntityType != Constants.UdiEntityType.Document)
                throw new ArgumentException($"Udi entity type must be \"{Constants.UdiEntityType.Document}\".", nameof(contentId));

            return GetById(preview, guidUdi.Guid);
        }
        private static IPublishedContent GetNodePublishedContent(ContentNode node, bool preview)
        {
            if (node == null)
                return null;

            // both .Draft and .Published cannot be null at the same time

            return preview
                ? node.DraftModel ?? GetPublishedContentAsDraft(node.PublishedModel)
                : node.PublishedModel;
        }

        // gets a published content as a previewing draft, if preview is true
        // this is for published content when previewing
        private static IPublishedContent GetPublishedContentAsDraft(IPublishedContent content /*, bool preview*/)
        {
            if (content == null /*|| preview == false*/) return null; //content;

            // an object in the cache is either an IPublishedContentOrMedia,
            // or a model inheriting from PublishedContentExtended - in which
            // case we need to unwrap to get to the original IPublishedContentOrMedia.

            var inner = PublishedContent.UnwrapIPublishedContent(content);
            return inner.AsDraft();
        }

        IEnumerable<IPublishedContent> GetAtRoot(bool preview) => GetAtRoot(preview);

        public IEnumerable<IPublishedContent> GetAtRoot(bool preview, string culture = null)
        {
            // handle context culture for variant
            if (culture == null)
                culture = _variationContextAccessor?.VariationContext?.Culture ?? "";

            // _snapshot.GetAtRoot() returns all ContentNode at root
            // both .Draft and .Published cannot be null at the same time
            // root is already sorted by sortOrder, and does not contain nulls
            //
            // GetNodePublishedContent may return null if !preview and there is no
            // published model, so we need to filter these nulls out

            var atRoot = _snapshot.GetContentSnapshot().GetAtRoot()
                .Select(n => GetNodePublishedContent(n, preview))
                .WhereNotNull();

            // if a culture is specified, we must ensure that it is avail/published
            if (culture != "*")
                atRoot = atRoot.Where(x => x.IsInvariantOrHasCulture(culture));

            return atRoot;
        }
    }
}
