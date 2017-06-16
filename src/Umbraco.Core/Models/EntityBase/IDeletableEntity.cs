using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.EntityBase
{
    public interface IDeletableEntity : IEntity
    {
        [DataMember]
        DateTime? DeletedDate { get; set; }
    }
}