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
            IVariationContextAccessor variationContextAccessor,
            IUmbracoContextAccessor umbracoContextAccessor)
            : this(id, uid, level, path, sortOrder, parentContentId, createDate, creatorId)
        {
            SetContentTypeAndData(contentType, draftData, publishedData, publishedSnapshotAccessor, variationContextAccessor, umbracoContextAccessor);
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
        public void SetContentTypeAndData(PublishedContentType contentType, ContentData draftData, ContentData publishedData, IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor, IUmbracoContextAccessor umbracoContextAccessor)
        {
            ContentType = contentType;

            if (draftData == null && publishedData == null)
                throw new ArgumentException("Both draftData and publishedData cannot be null at the same time.");

            if (draftData != null)
            {
                DraftContent = new PublishedContent(this, draftData, publishedSnapshotAccessor, variationContextAccessor, umbracoContextAccessor);
                DraftModel = DraftContent.CreateModel();
            }

            if (publishedData != null)
            {
                PublishedContent = new PublishedContent(this, publishedData, publishedSnapshotAccessor, variationContextAccessor, umbracoContextAccessor);
                PublishedModel = PublishedContent.CreateModel();
            }
        }

        // clone parent
        private ContentNode(ContentNode origin, IUmbracoContextAccessor umbracoContextAccessor)
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

            var originDraft = origin.DraftContent;
            var originPublished = origin.PublishedContent;


            DraftContent = originDraft == null ? null : new PublishedContent(this, originDraft, umbracoContextAccessor);
            PublishedContent = originPublished == null ? null : new PublishedContent(this, originPublished, umbracoContextAccessor);
            DraftModel = DraftContent?.CreateModel();
            PublishedModel = PublishedContent?.CreateModel();

            ChildContentIds = new List<int>(origin.ChildContentIds); // needs to be *another* list
        }

        // clone with new content type
        public ContentNode(ContentNode origin, PublishedContentType contentType, IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor, IUmbracoContextAccessor umbracoContextAccessor)
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

            var originDraft = origin.DraftContent;
            var originPublished = origin.PublishedContent;

            DraftContent = originDraft == null ? null : new PublishedContent(this, originDraft.ContentData, publishedSnapshotAccessor, variationContextAccessor, umbracoContextAccessor);
            DraftModel = DraftContent?.CreateModel();
            PublishedContent = originPublished == null ? null : new PublishedContent(this, originPublished.ContentData, publishedSnapshotAccessor, variationContextAccessor, umbracoContextAccessor);
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

        public ContentNode CloneParent(
            IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IUmbracoContextAccessor umbracoContextAccessor)
        {
            return new ContentNode(this, umbracoContextAccessor);
        }

        public ContentNodeKit ToKit()
        {
            return new ContentNodeKit
            {
                Node = this,
                ContentTypeId = ContentType.Id,

                DraftData = DraftContent?.ContentData,
                PublishedData = PublishedContent?.ContentData
            };
        }
    }
}
