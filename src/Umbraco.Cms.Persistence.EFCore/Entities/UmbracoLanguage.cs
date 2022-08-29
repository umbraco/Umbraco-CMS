using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoLanguage
    {
        public UmbracoLanguage()
        {
            CmsLanguageTexts = new HashSet<CmsLanguageText>();
            CmsTags = new HashSet<CmsTag>();
            InverseFallbackLanguage = new HashSet<UmbracoLanguage>();
            UmbracoContentSchedules = new HashSet<UmbracoContentSchedule>();
            UmbracoContentVersionCultureVariations = new HashSet<UmbracoContentVersionCultureVariation>();
            UmbracoDocumentCultureVariations = new HashSet<UmbracoDocumentCultureVariation>();
            UmbracoPropertyData = new HashSet<UmbracoPropertyDatum>();
            UserGroups = new HashSet<UmbracoUserGroup>();
        }

        public int Id { get; set; }
        public string? LanguageIsocode { get; set; }
        public string? LanguageCultureName { get; set; }
        public bool? IsDefaultVariantLang { get; set; }
        public bool? Mandatory { get; set; }
        public int? FallbackLanguageId { get; set; }

        public virtual UmbracoLanguage? FallbackLanguage { get; set; }
        public virtual ICollection<CmsLanguageText> CmsLanguageTexts { get; set; }
        public virtual ICollection<CmsTag> CmsTags { get; set; }
        public virtual ICollection<UmbracoLanguage> InverseFallbackLanguage { get; set; }
        public virtual ICollection<UmbracoContentSchedule> UmbracoContentSchedules { get; set; }
        public virtual ICollection<UmbracoContentVersionCultureVariation> UmbracoContentVersionCultureVariations { get; set; }
        public virtual ICollection<UmbracoDocumentCultureVariation> UmbracoDocumentCultureVariations { get; set; }
        public virtual ICollection<UmbracoPropertyDatum> UmbracoPropertyData { get; set; }

        public virtual ICollection<UmbracoUserGroup> UserGroups { get; set; }
    }
}
