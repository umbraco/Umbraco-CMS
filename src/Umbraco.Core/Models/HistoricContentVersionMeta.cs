using System;

namespace Umbraco.Core.Models
{
    public class HistoricContentVersionMeta
    {
        public int ContentId { get; }
        public int ContentTypeId { get; }
        public int VersionId { get; }
        public DateTime VersionDate { get; }

        public HistoricContentVersionMeta() { }

        public HistoricContentVersionMeta(int contentId, int contentTypeId, int versionId, DateTime versionDate)
        {
            ContentId = contentId;
            ContentTypeId = contentTypeId;
            VersionId = versionId;
            VersionDate = versionDate;
        }

        public override string ToString() => $"HistoricContentVersionMeta(versionId: {VersionId}, versionDate: {VersionDate:s}";
    }
}
