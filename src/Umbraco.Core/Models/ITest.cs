using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models
{
    public interface ITest : IEntity, IRememberBeingDirty
    {
        [DataMember]
        string Name { get; set; }
    }
}
