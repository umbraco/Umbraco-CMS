using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoContentSchedule
    {
        public Guid Id { get; set; }
        public int NodeId { get; set; }
        public int? LanguageId { get; set; }
        public DateTime Date { get; set; }
        public string Action { get; set; } = null!;

        public virtual UmbracoLanguage? Language { get; set; }
        public virtual UmbracoContent Node { get; set; } = null!;
    }
}
