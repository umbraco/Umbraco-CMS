using System;

namespace Umbraco.Web.Models.ContentEditing
{
    public class RollbackVersion
    {
        public int VersionId { get; set; }

        public DateTime VersionDate { get; set; }

        public string VersionAuthorName { get; set; }
    }
}
