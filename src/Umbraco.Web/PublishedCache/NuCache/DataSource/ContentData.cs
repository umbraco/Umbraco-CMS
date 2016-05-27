using System;
using System.Collections.Generic;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    // represents everything that is specific to draft or published version
    class ContentData
    {
        public bool Published { get; set; }

        public string Name { get; set; }
        public Guid Version { get; set; }
        public DateTime VersionDate { get; set; }
        public int WriterId { get; set; }
        public int TemplateId { get; set; }

        public IDictionary<string, object> Properties { get; set; }
    }
}
