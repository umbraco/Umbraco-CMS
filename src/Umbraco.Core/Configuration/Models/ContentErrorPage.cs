using System;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Configuration.Models.Validation;

namespace Umbraco.Core.Configuration.Models
{
    public class ContentErrorPage : ValidatableEntryBase
    {
        public int ContentId { get; set; }

        public Guid ContentKey { get; set; }

        public string ContentXPath { get; set; }

        public bool HasContentId => ContentId != 0;

        public bool HasContentKey => ContentKey != Guid.Empty;

        public bool HasContentXPath => !string.IsNullOrEmpty(ContentXPath);

        [Required]
        public string Culture { get; set; }

        internal override bool IsValid()
        {
            return base.IsValid() &&
                ((HasContentId ? 1 : 0) + (HasContentKey ? 1 : 0) + (HasContentXPath ? 1 : 0) == 1);
        }
    }
}
