using System;

namespace Umbraco.Core.Models
{
    public class ContentUrlRule
    {
        public ContentUrlRule()
        {
            CreateDateUtc = DateTime.UtcNow;
        }

        public int Id { get; internal set; }

        public int ContentId { get; set; }

        public DateTime CreateDateUtc { get; set; }

        public string Url { get; set; }
    }
}