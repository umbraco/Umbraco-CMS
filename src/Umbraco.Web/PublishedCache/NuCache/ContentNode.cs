using System;
using System.Collections.Generic;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    // represents a content "node" ie a pair of draft + published versions
    // internal, never exposed, to be accessed from ContentStore (only!)
    internal class ContentNode
    {
        // special ctor with no content data - for members
        public ContentNode(int id, Guid uid, PublishedContentType contentType,
            int level, string path, int sortOrder,
            int parentContentId,
            DateTime createDate, int creatorId)
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

            ChildContentIds = new List<int>();
        }

        public ContentNode(int id, Guid uid, PublishedContentType contentType,
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
            CreateDate = createDate;
            CreatorId = creatorId;

            ChildContentIds = new List<int>();
        }

        // two-phase ctor, phase 2
        public void SetContentTypeAndData(PublishedContentType contentType, ContentData draftData, ContentData publishedData, IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor)
        {
            ContentType = contentType;

            if (draftData == null && publishedData == null)
                throw new ArgumentException("Both draftData and publishedData cannot be null at the same time.");

            if (draftData != null)
                DraftModel = new PublishedContent(this, draftData, publishedSnapshotAccessor, variationContextAccessor).CreateModel();
            if (publishedData != null)
                PublishedModel = new PublishedContent(this, publishedData, publishedSnapshotAccessor, variationContextAccessor).CreateModel();
        }

        // clone parent
        private ContentNode(ContentNode origin)
        {
            // everything is the same, except for the child items
            // list which is a clone of the original list

            Id = origin.Id;
            Uid = origin.Uid;
            ContentType = origin.ContentType;
            Level = origin.Level;
            Path = origin.Path;
            SortOrder = origin.SortOrder;
            ParentContentId = origin.ParentContentId;
            CreateDate = origin.CreateDate;
            CreatorId = origin.CreatorId;

            var originDraft = origin.DraftModel == null ? null : PublishedContent.UnwrapIPublishedContent(origin.DraftModel);
            var originPublished = origin.PublishedModel == null ? null : PublishedContent.UnwrapIPublishedContent(origin.PublishedModel);

            DraftModel = originDraft == null ? null : new PublishedContent(this, originDraft).CreateModel();
            PublishedModel = originPublished == null ? null : new PublishedContent(this, originPublished).CreateModel();

            ChildContentIds = new List<int>(origin.ChildContentIds); // needs to be *another* list
        }

        // clone with new content type
        public ContentNode(ContentNode origin, PublishedContentType contentType, IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor)
        {
            Id = origin.Id;
            Uid = origin.Uid;
            ContentType = contentType; // change!
            Level = origin.Level;
            Path = origin.Path;
            SortOrder = origin.SortOrder;
            ParentContentId = origin.ParentContentId;
            CreateDate = origin.CreateDate;
            CreatorId = origin.CreatorId;

            var originDraft = origin.DraftModel == null ? null : PublishedContent.UnwrapIPublishedContent(origin.DraftModel);
            var originPublished = origin.PublishedModel == null ? null : PublishedContent.UnwrapIPublishedContent(origin.PublishedModel);

            DraftContent = originDraft == null ? null : new PublishedContent(this, originDraft._contentData, publishedSnapshotAccessor, variationContextAccessor);
            DraftModel = DraftContent?.CreateModel();
            PublishedContent = originPublished == null ? null : new PublishedContent(this, originPublished._contentData, publishedSnapshotAccessor, variationContextAccessor);
            PublishedModel = PublishedContent?.CreateModel();

            ChildContentIds = origin.ChildContentIds; // can be the *same* list
        }

        // everything that is common to both draft and published versions
        // keep this as small as possible
        public readonly int Id;
        public readonly Guid Uid;
        public PublishedContentType ContentType;
        public readonly int Level;
        public readonly string Path;
        public readonly int SortOrder;
        public readonly int ParentContentId;
        public List<int> ChildContentIds;
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

        public ContentNode CloneParent(IPublishedSnapshotAccessor publishedSnapshotAccessor)
        {
            return new ContentNode(this);
        }

        public ContentNodeKit ToKit()
        {
            var draft = DraftModel is PublishedContentModel draftModel
                ? (PublishedContent) draftModel.Unwrap()
                : (PublishedContent) DraftModel;

            var published = PublishedModel is PublishedContentModel publishedModel
                ? (PublishedContent) publishedModel.Unwrap()
                : (PublishedContent) PublishedModel;

            return new ContentNodeKit
            {
                Node = this,
                ContentTypeId = ContentType.Id,

                DraftData = draft?._contentData,
                PublishedData = published?._contentData
            };
        }
    }
}
