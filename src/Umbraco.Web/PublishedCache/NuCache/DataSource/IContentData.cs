using System;
using System.Collections.Generic;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    public interface IContentData
    {
        IReadOnlyDictionary<string, CultureVariation> CultureInfos { get; set; }
        string Name { get; set; }
        IDictionary<string, PropertyData[]> Properties { get; set; }
        bool Published { get; set; }
        int? TemplateId { get; set; }
        string UrlSegment { get; set; }
        DateTime VersionDate { get; set; }
        int VersionId { get; set; }
        int WriterId { get; set; }
    }
}
