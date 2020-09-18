using System;

namespace Umbraco.Core.Configuration.Models
{
    public class ContentErrorPage
    {
        //TODO introduce validation, to check only one of key/id/xPath is used.
        public int ContentId { get; }
        public Guid ContentKey { get; }
        public string ContentXPath { get; }
        public bool HasContentId { get; }
        public bool HasContentKey { get; }
        public string Culture { get; set; }
    }
}
