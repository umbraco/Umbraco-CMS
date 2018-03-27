using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Examine;
using Examine.LuceneEngine;
using Examine.Session;
using Lucene.Net.Documents;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;
using Umbraco.Web.Composing;
using Umbraco.Web.PropertyEditors;
using Umbraco.Examine;

namespace Umbraco.Web.Search
{
    /// <summary>
    /// Configures and installs Examine.
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class ExamineComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        private IScopeProvider _scopeProvider;

        // the default enlist priority is 100
        // enlist with a lower priority to ensure that anything "default" runs after us
        // but greater that SafeXmlReaderWriter priority which is 60
        private const int EnlistPriority = 80;

        public void Initialize(IRuntimeState runtime, PropertyEditorCollection propertyEditors, IExamineIndexCollectionAccessor indexCollection, IScopeProvider scopeProvider, ILogger logger)
        {
            _scopeProvider = scopeProvider;

            logger.Info<ExamineComponent>("Starting initialize async background thread.");

            // make it async in order not to slow down the boot
            // fixme - should be a proper background task else we cannot stop it!
            var bg = new Thread(() =>
            {
                try
                {
                    // from WebRuntimeComponent
                    // rebuilds any empty indexes
                    RebuildIndexes(true);
                }
                catch (Exception e)
                {
                    logger.Error<ExamineComponent>("Failed to rebuild empty indexes.", e);
                }

                try
                {
                    // from PropertyEditorsComponent
                    var grid = propertyEditors.OfType<GridPropertyEditor>().FirstOrDefault();
                    if (grid != null) BindGridToExamine(grid, indexCollection);
                }
                catch (Exception e)
                {
                    logger.Error<ExamineComponent>("Failed to bind grid property editor.", e);
                }
            });
            bg.Start();

            // the rest is the original Examine event handler

            logger.Info<ExamineComponent>("Initialize and bind to business logic events.");

            //TODO: For now we'll make this true, it means that indexes will be near real time
            // we'll see about what implications this may have - should be great in most scenarios
            DefaultExamineSession.RequireImmediateConsistency = true;

            var registeredProviders = ExamineManager.Instance.IndexProviderCollection
                .OfType<BaseUmbracoIndexer>().Count(x => x.EnableDefaultEventHandler);

            logger.Info<ExamineComponent>($"Adding examine event handlers for {registeredProviders} index providers.");

            // don't bind event handlers if we're not suppose to listen
            if (registeredProviders == 0)
                return;

            // bind to distributed cache events - this ensures that this logic occurs on ALL servers
            // that are taking part in a load balanced environment.
            ContentCacheRefresher.CacheUpdated += ContentCacheRefresherUpdated;
            MediaCacheRefresher.CacheUpdated += MediaCacheRefresherUpdated;
            MemberCacheRefresher.CacheUpdated += MemberCacheRefresherUpdated;

            // fixme - content type?
            // events handling removed in ef013f9d3b945d0a48a306ff1afbd49c10c3fff8
            // because, could not make sense of it?

            var contentIndexer = ExamineManager.Instance.IndexProviderCollection[Constants.Examine.InternalIndexer] as UmbracoContentIndexer;
            if (contentIndexer != null)
            {
                contentIndexer.DocumentWriting += IndexerDocumentWriting;
            }
            var memberIndexer = ExamineManager.Instance.IndexProviderCollection[Constants.Examine.InternalMemberIndexer] as UmbracoMemberIndexer;
            if (memberIndexer != null)
            {
                memberIndexer.DocumentWriting += IndexerDocumentWriting;
            }
        }

        private static void RebuildIndexes(bool onlyEmptyIndexes)
        {
            var indexers = (IEnumerable<KeyValuePair<string, IExamineIndexer>>)ExamineManager.Instance.IndexProviders;
            if (onlyEmptyIndexes)
                indexers = indexers.Where(x => x.Value.IsIndexNew());
            foreach (var indexer in indexers)
                indexer.Value.RebuildIndex();
        }

        private static void BindGridToExamine(GridPropertyEditor grid, IExamineIndexCollectionAccessor indexCollection)
        {
            var indexes = indexCollection.Indexes;
            if (indexes == null) return;
            foreach (var i in indexes.Values.OfType<BaseUmbracoIndexer>())
                i.DocumentWriting += grid.DocumentWriting;
        }

        void MemberCacheRefresherUpdated(MemberCacheRefresher sender, CacheRefresherEventArgs args)
        {
            if (Suspendable.ExamineEvents.CanIndex == false)
                return;

            switch (args.MessageType)
            {
                case MessageType.RefreshById:
                    var c1 = Current.Services.MemberService.GetById((int)args.MessageObject);
                    if (c1 != null)
                    {
                        ReIndexForMember(c1);
                    }
                    break;
                case MessageType.RemoveById:

                    // This is triggered when the item is permanently deleted

                    DeleteIndexForEntity((int)args.MessageObject, false);
                    break;
                case MessageType.RefreshByInstance:
                    var c3 = args.MessageObject as IMember;
                    if (c3 != null)
                    {
                        ReIndexForMember(c3);
                    }
                    break;
                case MessageType.RemoveByInstance:

                    // This is triggered when the item is permanently deleted

                    var c4 = args.MessageObject as IMember;
                    if (c4 != null)
                    {
                        DeleteIndexForEntity(c4.Id, false);
                    }
                    break;
                case MessageType.RefreshAll:
                case MessageType.RefreshByJson:
                default:
                    //We don't support these, these message types will not fire for unpublished content
                    break;
            }
        }

        void MediaCacheRefresherUpdated(MediaCacheRefresher sender, CacheRefresherEventArgs args)
        {
            if (Suspendable.ExamineEvents.CanIndex == false)
                return;

            if (args.MessageType != MessageType.RefreshByPayload)
                throw new NotSupportedException();

            var mediaService = Current.Services.MediaService;

            foreach (var payload in (MediaCacheRefresher.JsonPayload[]) args.MessageObject)
            {
                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
                {
                    // remove from *all* indexes
                    DeleteIndexForEntity(payload.Id, false);
                }
                else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                {
                    // ExamineEvents does not support RefreshAll
                    // just ignore that payload
                    // so what?!
                }
                else // RefreshNode or RefreshBranch (maybe trashed)
                {
                    var media = mediaService.GetById(payload.Id);
                    if (media == null || media.Trashed)
                    {
                        // gone fishing, remove entirely
                        DeleteIndexForEntity(payload.Id, false);
                        continue;
                    }

                    // just that media
                    ReIndexForMedia(media, media.Trashed == false);

                    // branch
                    if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        var descendants = mediaService.GetDescendants(media);
                        foreach (var descendant in descendants)
                        {
                            ReIndexForMedia(descendant, descendant.Trashed == false);
                        }
                    }
                }
            }
        }

        void ContentCacheRefresherUpdated(ContentCacheRefresher sender, CacheRefresherEventArgs args)
        {
            if (Suspendable.ExamineEvents.CanIndex == false)
                return;

            if (args.MessageType != MessageType.RefreshByPayload)
                throw new NotSupportedException();

            var contentService = Current.Services.ContentService;

            foreach (var payload in (ContentCacheRefresher.JsonPayload[]) args.MessageObject)
            {
                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
                {
                    // delete content entirely (with descendants)
                    //  false: remove entirely from all indexes
                    DeleteIndexForEntity(payload.Id, false);
                }
                else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                {
                    // ExamineEvents does not support RefreshAll
                    // just ignore that payload
                    // so what?!
                }
                else // RefreshNode or RefreshBranch (maybe trashed)
                {
                    // don't try to be too clever - refresh entirely
                    // there has to be race conds in there ;-(

                    var content = contentService.GetById(payload.Id);
                    if (content == null || content.Trashed)
                    {
                        // gone fishing, remove entirely from all indexes (with descendants)
                        DeleteIndexForEntity(payload.Id, false);
                        continue;
                    }

                    IContent published = null;
                    if (content.Published && ((ContentService)contentService).IsPathPublished(content))
                        published = content;

                    // just that content
                    ReIndexForContent(content, published);

                    // branch
                    if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        var masked = published == null ? null : new List<int>();
                        var descendants = contentService.GetDescendants(content);
                        foreach (var descendant in descendants)
                        {
                            published = null;
                            if (masked != null) // else everything is masked
                            {
                                if (masked.Contains(descendant.ParentId) || !descendant.Published)
                                    masked.Add(descendant.Id);
                                else
                                    published = descendant;
                            }

                            ReIndexForContent(descendant, published);
                        }
                    }
                }

                // NOTE
                //
                // DeleteIndexForEntity is handled by UmbracoContentIndexer.DeleteFromIndex() which takes
                //  care of also deleting the descendants
                //
                // ReIndexForContent is NOT taking care of descendants so we have to reload everything
                //  again in order to process the branch - we COULD improve that by just reloading the
                //  XML from database instead of reloading content & re-serializing!
            }
        }

        private void ReIndexForContent(IContent content, IContent published)
        {
            if (published != null && content.VersionId == published.VersionId)
            {
                ReIndexForContent(content); // same = both
            }
            else
            {
                if (published == null)
                {
                    // remove 'published' - keep 'draft'
                    DeleteIndexForEntity(content.Id, true);
                }
                else
                {
                    // index 'published' - don't overwrite 'draft'
                    ReIndexForContent(published, false);
                }
                ReIndexForContent(content, true); // index 'draft'
            }
        }

        private void ReIndexForContent(IContent sender, bool? supportUnpublished = null)
        {
            var actions = DeferedActions.Get(_scopeProvider);
            if (actions != null)
                actions.Add(new DeferedReIndexForContent(sender, supportUnpublished));
            else
                DeferedReIndexForContent.Execute(sender, supportUnpublished);
        }

        private void ReIndexForMember(IMember member)
        {
            var actions = DeferedActions.Get(_scopeProvider);
            if (actions != null)
                actions.Add(new DeferedReIndexForMember(member));
            else
                DeferedReIndexForMember.Execute(member);        }

        private void ReIndexForMedia(IMedia sender, bool isMediaPublished)
        {
            var actions = DeferedActions.Get(_scopeProvider);
            if (actions != null)
                actions.Add(new DeferedReIndexForMedia(sender, isMediaPublished));
            else
                DeferedReIndexForMedia.Execute(sender, isMediaPublished);
        }

        /// <summary>
        /// Remove items from any index that doesn't support unpublished content
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="keepIfUnpublished">
        /// If true, indicates that we will only delete this item from indexes that don't support unpublished content.
        /// If false it will delete this from all indexes regardless.
        /// </param>
        private void DeleteIndexForEntity(int entityId, bool keepIfUnpublished)
        {
            var actions = DeferedActions.Get(_scopeProvider);
            if (actions != null)
                actions.Add(new DeferedDeleteIndex(entityId, keepIfUnpublished));
            else
                DeferedDeleteIndex.Execute(entityId, keepIfUnpublished);
        }

        /// <summary>
        /// Event handler to create a lower cased version of the node name, this is so we can support case-insensitive searching and still
        /// use the Whitespace Analyzer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private static void IndexerDocumentWriting(object sender, DocumentWritingEventArgs e)
        {
            if (e.Fields.Keys.Contains("nodeName"))
            {
                //TODO: This logic should really be put into the content indexer instead of hidden here!!

                //add the lower cased version
                e.Document.Add(new Field("__nodeName",
                                        e.Fields["nodeName"].ToLower(),
                                        Field.Store.YES,
                                        Field.Index.ANALYZED,
                                        Field.TermVector.NO
                                        ));
            }
        }

        private class DeferedActions
	    {
	        private readonly List<DeferedAction> _actions = new List<DeferedAction>();

	        public static DeferedActions Get(IScopeProvider scopeProvider)
	        {
	            var scopeContext = scopeProvider.Context;
	            if (scopeContext == null) return null;

	            return scopeContext.Enlist("examineEvents",
	                () => new DeferedActions(), // creator
	                (completed, actions) => // action
	                {
	                    if (completed) actions.Execute();
	                }, EnlistPriority);
	        }

	        public void Add(DeferedAction action)
	        {
                _actions.Add(action);
	        }

	        private void Execute()
	        {
	            foreach (var action in _actions)
	                action.Execute();
	        }
	    }

	    private abstract class DeferedAction
	    {
            public virtual void Execute()
            { }
	    }

	    private class DeferedReIndexForContent : DeferedAction
	    {
	        private readonly IContent _content;
	        private readonly bool? _supportUnpublished;

	        public DeferedReIndexForContent(IContent content, bool? supportUnpublished)
	        {
	            _content = content;
	            _supportUnpublished = supportUnpublished;
	        }

	        public override void Execute()
	        {
                Execute(_content, _supportUnpublished);
	        }

	        public static void Execute(IContent content, bool? supportUnpublished)
	        {
	            var xml = content.ToXml();
	            //add an icon attribute to get indexed
	            xml.Add(new XAttribute("icon", content.ContentType.Icon));

	            ExamineManager.Instance.ReIndexNode(
	                xml, IndexTypes.Content,
	                ExamineManager.Instance.IndexProviderCollection.OfType<UmbracoContentIndexer>()

	                    //Index this item for all indexers if the content is published, otherwise if the item is not published
	                    // then only index this for indexers supporting unpublished content

	                    .Where(x => supportUnpublished.HasValue == false || supportUnpublished.Value == x.SupportUnpublishedContent)
	                    .Where(x => x.EnableDefaultEventHandler));
	        }
	    }

	    private class DeferedReIndexForMedia : DeferedAction
	    {
	        private readonly IMedia _media;
	        private readonly bool _isPublished;

	        public DeferedReIndexForMedia(IMedia media, bool isPublished)
	        {
	            _media = media;
	            _isPublished = isPublished;
	        }

	        public override void Execute()
	        {
	            Execute(_media, _isPublished);
	        }

	        public static void Execute(IMedia media, bool isPublished)
	        {
	            var xml = media.ToXml();
	            //add an icon attribute to get indexed
	            xml.Add(new XAttribute("icon", media.ContentType.Icon));

	            ExamineManager.Instance.ReIndexNode(
	                xml, IndexTypes.Media,
	                ExamineManager.Instance.IndexProviderCollection.OfType<UmbracoContentIndexer>()

	                    //Index this item for all indexers if the media is not trashed, otherwise if the item is trashed
	                    // then only index this for indexers supporting unpublished media

	                    .Where(x => isPublished || (x.SupportUnpublishedContent))
	                    .Where(x => x.EnableDefaultEventHandler));
	        }
	    }

	    private class DeferedReIndexForMember : DeferedAction
	    {
	        private readonly IMember _member;

	        public DeferedReIndexForMember(IMember member)
	        {
	            _member = member;
	        }

	        public override void Execute()
	        {
	            Execute(_member);
	        }

	        public static void Execute(IMember member)
	        {
	            ExamineManager.Instance.ReIndexNode(
	                member.ToXml(), IndexTypes.Member,
	                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
	                    //ensure that only the providers are flagged to listen execute
	                    .Where(x => x.EnableDefaultEventHandler));
	        }
	    }

	    private class DeferedDeleteIndex : DeferedAction
	    {
	        private readonly int _id;
	        private readonly bool _keepIfUnpublished;

	        public DeferedDeleteIndex(int id, bool keepIfUnpublished)
	        {
	            _id = id;
	            _keepIfUnpublished = keepIfUnpublished;
	        }

	        public override void Execute()
	        {
	            Execute(_id, _keepIfUnpublished);
	        }

	        public static void Execute(int id, bool keepIfUnpublished)
	        {
	            ExamineManager.Instance.DeleteFromIndex(
	                id.ToString(CultureInfo.InvariantCulture),
	                ExamineManager.Instance.IndexProviderCollection.OfType<UmbracoContentIndexer>()

	                    //if keepIfUnpublished == true then only delete this item from indexes not supporting unpublished content,
	                    // otherwise if keepIfUnpublished == false then remove from all indexes

	                    .Where(x => keepIfUnpublished == false || x.SupportUnpublishedContent == false)
	                    .Where(x => x.EnableDefaultEventHandler));
	        }
	    }
    }
}
