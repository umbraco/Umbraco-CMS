using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public interface IRedirectUrl : IAggregateRoot, IRememberBeingDirty
    {
        [DataMember]
        int ContentId { get; set; }

        [DataMember]
        DateTime CreateDateUtc { get; set; }

        [DataMember]
        string Url { get; set; }
    }
}
