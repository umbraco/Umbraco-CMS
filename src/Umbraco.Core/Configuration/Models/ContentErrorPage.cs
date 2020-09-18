using System;

namespace Umbraco.Core.Configuration.Models
{
    public class ContentErrorPage
    {
        public int ContentId { get; set; }

        public Guid ContentKey { get; set; }

        public string ContentXPath { get; set; }

        public bool HasContentId => ContentId != 0;

        public bool HasContentKey => ContentKey != Guid.Empty;

        public bool HasContentXPath => !string.IsNullOrEmpty(ContentXPath);

        public string Culture { get; set; }

        public bool IsValid()
        {
            // Entry is valid if Culture and one and only one of ContentId, ContentKey or ContentXPath is provided.
            return !string.IsNullOrWhiteSpace(Culture) &&
                ((HasContentId ? 1 : 0) + (HasContentKey ? 1 : 0) + (HasContentXPath ? 1 : 0) == 1);
        }
    }
}
