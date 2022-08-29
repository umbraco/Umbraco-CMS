using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoContentVersion
    {
        public UmbracoContentVersion()
        {
            UmbracoContentVersionCultureVariations = new HashSet<UmbracoContentVersionCultureVariation>();
            UmbracoPropertyData = new HashSet<UmbracoPropertyDatum>();
        }

        public int Id { get; set; }
        public int NodeId { get; set; }
        public DateTime VersionDate { get; set; }
        public int? UserId { get; set; }
        public bool Current { get; set; }
        public string? Text { get; set; }
        public bool? PreventCleanup { get; set; }

        public virtual UmbracoContent Node { get; set; } = null!;
        public virtual UmbracoUser? User { get; set; }
        public virtual UmbracoDocumentVersion? UmbracoDocumentVersion { get; set; }
        public virtual UmbracoMediaVersion? UmbracoMediaVersion { get; set; }
        public virtual ICollection<UmbracoContentVersionCultureVariation> UmbracoContentVersionCultureVariations { get; set; }
        public virtual ICollection<UmbracoPropertyDatum> UmbracoPropertyData { get; set; }
    }
}
