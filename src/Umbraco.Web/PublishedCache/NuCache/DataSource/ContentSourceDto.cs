using System;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    // read-only dto
    internal class ContentSourceDto
    {
        public int Id { get; set; }
        public Guid Uid { get; set; }
        public int ContentTypeId { get; set; }

        public int Level { get; set; }
        public string Path { get; set; }
        public int SortOrder { get; set; }
        public int ParentId { get; set; }

        public bool Published { get; set; }
        public bool Edited { get; set; }

        public DateTime CreateDate { get; set; }
        public int CreatorId { get; set; }

        // edited data
        public int VersionId { get; set; }
        public string EditName { get; set; }
        public DateTime EditVersionDate { get; set; }
        public int EditWriterId { get; set; }
        public int EditTemplateId { get; set; }
        public string EditData { get; set; }

        // published data
        public int PublishedVersionId { get; set; }
        public string PubName { get; set; }
        public DateTime PubVersionDate { get; set; }
        public int PubWriterId { get; set; }
        public int PubTemplateId { get; set; }
        public string PubData { get; set; }
    }
}
