using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class RedirectUrl : Entity, IRedirectUrl
    {
        public RedirectUrl()
        {
            CreateDateUtc = DateTime.UtcNow;
        }

        public int ContentId { get; set; }

        public DateTime CreateDateUtc { get; set; }

        public string Url { get; set; }
    }
}