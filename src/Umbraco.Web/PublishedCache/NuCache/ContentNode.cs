using System;
using System.Diagnostics;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    // represents a content "node" ie a pair of draft + published versions
    // internal, never exposed, to be accessed from ContentStore (only!)
    [DebuggerDisplay("Id: {Id}, Path: {Path}")]
    internal class ContentNode
    {
        // special ctor for root pseudo node
        public ContentNode()
        {
            FirstChildContentId = -1;
            LastChildContentId = -1;
            NextSiblingContentId = -1;
            PreviousSiblingContentId = -1;
        }

        // special ctor with no content data - for members
        public ContentNode(int id, Guid uid, IPublishedContentType contentType,
            int level, string path, int sortOrder,
            int parentContentId,
            DateTime createDate, int creatorId)
            : this()
        {
            Id = id;
            Uid = uid;
            ContentType = contentType;
            Level = level;
            Path = path;
            SortOrder = sortOrder;
            ParentContentId = parentContentId;
            CreateDate = createDate;
            CreatorId = creatorId;
        }

        public ContentNode(int id, Guid uid, IPublishedContentType contentType,
            int level, string path, int sortOrder,
            int parentContentId,
            DateTime createDate, int creatorId,
            ContentData draftData, ContentData publishedData,
            IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IVariationContextAccessor variationContextAccessor)
            : this(id, uid, level, path, sortOrder, parentContentId, createDate, creatorId)
        {
            SetContentTypeAndData(contentType, draftData, publishedData, publishedSnapshotAccessor, variationContextAccessor);
        }

        // 2-phases ctor, phase 1
        public ContentNode(int id, Guid uid,
            int level, string path, int sortOrder,
            int parentContentId,
            DateTime createDate, int creatorId)
        {
            Id = id;
            Uid = uid;
            Level = level;
            Path = path;
            SortOrder = sortOrder;
            ParentContentId = parentContentId;
            FirstChildContentId = -1;
            LastChildContentId = -1;
            NextSiblingContentId = -1;
            PreviousSiblingContentId = -1;
            CreateDate = createDate;
            CreatorId = creatorId;
        }

        // two-phase ctor, phase 2
        public void SetContentTypeAndData(IPublishedContentType contentType, ContentData draftData, ContentData publishedData, IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor)
        {
            ContentType = contentType;

            if (draftData == null && publishedData == null)
                throw new ArgumentException("Both draftData and publishedData cannot be null at the same time.");

            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _variationContextAccessor = variationContextAccessor;

            _draftData = draftData;
            _publishedData = publishedData;
        }

        // clone
        public ContentNode(ContentNode origin, IPublishedContentType contentType = null)
        {
            Id = origin.Id;
            Uid = origin.Uid;
            ContentType = contentType ?? origin.ContentType;
            Level = origin.Level;
            Path = origin.Path;
            SortOrder = origin.SortOrder;
            ParentContentId = origin.ParentContentId;
            FirstChildContentId = origin.FirstChildContentId;
            LastChildContentId = origin.LastChildContentId;
            NextSiblingContentId = origin.NextSiblingContentId;
            PreviousSiblingContentId = origin.PreviousSiblingContentId;
            CreateDate = origin.CreateDate;
            CreatorId = origin.CreatorId;

            _draftData = origin._draftData;
            _publishedData = origin._publishedData;
            _publishedSnapshotAccessor = origin._publishedSnapshotAccessor;
            _variationContextAccessor = origin._variationContextAccessor;
        }

        // everything that is common to both draft and published versions
        // keep this as small as possible

        
        public readonly int Id;
        public readonly Guid Uid;
        public IPublishedContentType ContentType;
        public readonly int Level;
        public readonly string Path;
        public readonly int SortOrder;
        public readonly int ParentContentId;

        // TODO: Can we make everything readonly?? This would make it easier to debug and be less error prone especially for new developers.
        // Once a Node is created and exists in the cache it is readonly so we should be able to make that happen at the API level too.
        public int FirstChildContentId;
        public int LastChildContentId;
        public int NextSiblingContentId;
        public int PreviousSiblingContentId;

        public readonly DateTime CreateDate;
        public readonly int CreatorId;

        private ContentData _draftData;
        private ContentData _publishedData;
        private IVariationContextAccessor _variationContextAccessor;
        private IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public bool HasPublished => _publishedData != null;
        public bool HasPublishedCulture(string culture) => _publishedData != null && _publishedData.CultureInfos.ContainsKey(culture);

        // draft and published version (either can be null, but not both)
        // are models not direct PublishedContent instances
        private IPublishedContent _draftModel;
        private IPublishedContent _publishedModel;

        private IPublishedContent GetModel(ref IPublishedContent model, ContentData contentData)
        {
            if (model != null) return model;
            if (contentData == null) return null;

            // create the model - we want to be fast, so no lock here: we may create
            // more than 1 instance, but the lock below ensures we only ever return
            // 1 unique instance - and locking is a nice explicit way to ensure this

            var m = new PublishedContent(this, contentData, _publishedSnapshotAccessor, _variationContextAccessor).CreateModel();

            // locking 'this' is not a best-practice but ContentNode is internal and
            // we know what we do, so it is fine here and avoids allocating an object
            lock (this)
            {
                return model = model ?? m;
            }
        }

        public IPublishedContent DraftModel => GetModel(ref _draftModel, _draftData);

        public IPublishedContent PublishedModel => GetModel(ref _publishedModel, _publishedData);

        public ContentNodeKit ToKit()
            => new ContentNodeKit
                {
                    Node = this,
                    ContentTypeId = ContentType.Id,

                    DraftData = _draftData,
                    PublishedData = _publishedData
                };
    }
}
