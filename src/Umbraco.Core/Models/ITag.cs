using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public interface ITag : IAggregateRoot
    {
        [DataMember]
        string Text { get; set; }

        [DataMember]
        string Group { get; set; }

        //TODO: enable this at some stage
        //int ParentId { get; set; }
    }
}