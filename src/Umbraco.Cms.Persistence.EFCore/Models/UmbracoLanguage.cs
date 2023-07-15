using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoLanguage
{
    public int Id { get; set; }

    public string? LanguageIsocode { get; set; }

    public string? LanguageCultureName { get; set; }

    public bool? IsDefaultVariantLang { get; set; }

    public bool? Mandatory { get; set; }

    public int? FallbackLanguageId { get; set; }

    public virtual ICollection<CmsLanguageText> CmsLanguageTexts { get; set; } = new List<CmsLanguageText>();

    public virtual ICollection<CmsTag> CmsTags { get; set; } = new List<CmsTag>();

    public virtual UmbracoLanguage? FallbackLanguage { get; set; }

    public virtual ICollection<UmbracoLanguage> InverseFallbackLanguage { get; set; } = new List<UmbracoLanguage>();

    public virtual ICollection<UmbracoContentSchedule> UmbracoContentSchedules { get; set; } = new List<UmbracoContentSchedule>();

    public virtual ICollection<UmbracoContentVersionCultureVariation> UmbracoContentVersionCultureVariations { get; set; } = new List<UmbracoContentVersionCultureVariation>();

    public virtual ICollection<UmbracoDocumentCultureVariation> UmbracoDocumentCultureVariations { get; set; } = new List<UmbracoDocumentCultureVariation>();

    public virtual ICollection<UmbracoPropertyDatum> UmbracoPropertyData { get; set; } = new List<UmbracoPropertyDatum>();

    public virtual ICollection<UmbracoUserGroup> UserGroups { get; set; } = new List<UmbracoUserGroup>();
}
