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

        public DateTime CreateDate { get; set; }
        public int CreatorId { get; set; }

        public string DraftName { get; set; }
        public Guid DraftVersion { get; set; }
        public DateTime DraftVersionDate { get; set; }
        public int DraftWriterId { get; set; }
        public int DraftTemplateId { get; set; }
        public string DraftData { get; set; }

        public string PubName { get; set; }
        public Guid PubVersion { get; set; }
        public DateTime PubVersionDate { get; set; }
        public int PubWriterId { get; set; }
        public int PubTemplateId { get; set; }
        public string PubData { get; set; }
    }
}
