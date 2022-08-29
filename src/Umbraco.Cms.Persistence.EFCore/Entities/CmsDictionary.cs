using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class CmsDictionary
    {
        public CmsDictionary()
        {
            CmsLanguageTexts = new HashSet<CmsLanguageText>();
            InverseParentNavigation = new HashSet<CmsDictionary>();
        }

        public int Pk { get; set; }
        public Guid Id { get; set; }
        public Guid? Parent { get; set; }
        public string Key { get; set; } = null!;

        public virtual CmsDictionary? ParentNavigation { get; set; }
        public virtual ICollection<CmsLanguageText> CmsLanguageTexts { get; set; }
        public virtual ICollection<CmsDictionary> InverseParentNavigation { get; set; }
    }
}
