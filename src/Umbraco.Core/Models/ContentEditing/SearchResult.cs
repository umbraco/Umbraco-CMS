﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "result", Namespace = "")]
    public class SearchResult
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "score")]
        public float Score { get; set; }

        [DataMember(Name = "fieldCount")]
        public int FieldCount => Values?.Count ?? 0;

        [DataMember(Name = "values")]
        public IReadOnlyDictionary<string, string> Values { get; set; }
    }
}
