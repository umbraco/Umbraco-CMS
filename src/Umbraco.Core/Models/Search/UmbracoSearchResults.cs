﻿using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Search;
[DataContract(Name = "umbracoSearchResults", Namespace = "")]
public class UmbracoSearchResults {
    public UmbracoSearchResults(long totalRecords, IEnumerable<UmbracoSearchResult> results)
    {
        TotalRecords = totalRecords;
        Results = results;
    }
    [DataMember(Name = "totalRecords")]
    public long TotalRecords { get; set; }
    [DataMember(Name = "results")]
    public IEnumerable<UmbracoSearchResult> Results { get; set; }

    public static UmbracoSearchResults Empty()
    {
        return new UmbracoSearchResults(0, new List<UmbracoSearchResult>());
    }
}
