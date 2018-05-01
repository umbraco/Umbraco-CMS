using System;
using System.Collections.Generic;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    // represents everything that is specific to edited or published version
    internal class ContentData
    {
        public bool Published { get; set; }

        /// <summary>
        /// The collection of language Id to name for the content item
        /// </summary>
        public IReadOnlyDictionary<string, CultureVariation> CultureInfos { get; set; }

        public string Name { get; set; }
        public int VersionId { get; set; }
        //TODO: This will not make a lot of sense since we'll have dates for each variant publishing, need to wait on Stephane
        public DateTime VersionDate { get; set; }
        public int WriterId { get; set; }
        public int TemplateId { get; set; }

        public IDictionary<string, PropertyData[]> Properties { get; set; }
    }
}
