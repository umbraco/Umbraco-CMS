using System;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    // represents a content "node" ie a pair of draft + published versions
    // internal, never exposed, to be accessed from ContentStore (only!)
    internal class ContentNode
    {
        // special ctor for root pseudo node
        public ContentNode()
        {
            FirstChildContentId = -1;
            NextSiblingContentId = -1;
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
            NextSiblingContentId = -1;
            CreateDate = createDate;
            CreatorId = creatorId;
        }

        // two-phase ctor, phase 2
        public void SetContentTypeAndData(IPublishedContentType contentType, ContentData draftData, ContentData publishedData, IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor)
        {
            ContentType = contentType;

            if (draftData == null && publishedData == null)
                throw new ArgumentException("Both draftData and publishedData cannot be null at the same time.");

            if (draftData != null)
            {
                DraftContent = new PublishedContent(this, draftData, publishedSnapshotAccessor, variationContextAccessor);
                DraftModel = DraftContent.CreateModel();
            }

            if (publishedData != null)
            {
                PublishedContent = new PublishedContent(this, publishedData, publishedSnapshotAccessor, variationContextAccessor);
                PublishedModel = PublishedContent.CreateModel();
            }
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
            NextSiblingContentId = origin.NextSiblingContentId;
            CreateDate = origin.CreateDate;
            CreatorId = origin.CreatorId;

            var originDraft = origin.DraftContent;
            var originPublished = origin.PublishedContent;

            DraftContent = originDraft == null ? null : new PublishedContent(this, originDraft);
            PublishedContent = originPublished == null ? null : new PublishedContent(this, originPublished);

            DraftModel = DraftContent?.CreateModel();
            PublishedModel = PublishedContent?.CreateModel();
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
        public int FirstChildContentId;
        public int NextSiblingContentId;
        public readonly DateTime CreateDate;
        public readonly int CreatorId;

        // draft and published version (either can be null, but not both)
        // are the direct PublishedContent instances
        public PublishedContent DraftContent;
        public PublishedContent PublishedContent;

        // draft and published version (either can be null, but not both)
        // are models not direct PublishedContent instances
        public IPublishedContent DraftModel;
        public IPublishedContent PublishedModel;

        public ContentNodeKit ToKit()
            => new ContentNodeKit
                {
                    Node = this,
                    ContentTypeId = ContentType.Id,

                    DraftData = DraftContent?.ContentData,
                    PublishedData = PublishedContent?.ContentData
                };
    }
}
