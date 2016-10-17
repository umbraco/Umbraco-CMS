﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "redirectUrlSearchResult", Namespace = "")]
    public class RedirectUrlSearchResult
    {
        [DataMember(Name = "searchResults")]
        public IEnumerable<ContentRedirectUrl> SearchResults { get; set; }
        
        [DataMember(Name = "totalCount")]
        public long TotalCount { get; set; }

        [DataMember(Name = "pageCount")]
        public int PageCount { get; set; }

        [DataMember(Name = "currentPage")]
        public int CurrentPage { get; set; }     
    }
}