using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.HybridCache.Persistence
{
    // read-only dto
    internal class ContentSourceDto : IReadOnlyContentBase
    {
        public int Id { get; init; }

        public Guid Key { get; init; }

        public int ContentTypeId { get; init; }

        public int Level { get; init; }

        public string Path { get; init; } = string.Empty;

        public int SortOrder { get; init; }

        public int ParentId { get; init; }

        public bool Published { get; init; }

        public bool Edited { get; init; }

        public DateTime CreateDate { get; init; }

        public int CreatorId { get; init; }

        // edited data
        public int VersionId { get; init; }

        public string? EditName { get; init; }

        public DateTime EditVersionDate { get; init; }

        public int EditWriterId { get; init; }

        public int EditTemplateId { get; init; }

        public string? EditData { get; init; }

        public byte[]? EditDataRaw { get; init; }

        // published data
        public int PublishedVersionId { get; init; }

        public string? PubName { get; init; }

        public DateTime PubVersionDate { get; init; }

        public int PubWriterId { get; init; }

        public int PubTemplateId { get; init; }

        public string? PubData { get; init; }

        public byte[]? PubDataRaw { get; init; }

        // Explicit implementation
        DateTime IReadOnlyContentBase.UpdateDate => EditVersionDate;

        string? IReadOnlyContentBase.Name => EditName;

        int IReadOnlyContentBase.WriterId => EditWriterId;
    }
}
