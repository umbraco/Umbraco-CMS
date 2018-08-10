using System.ComponentModel;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract(Name = "task", Namespace = "")]
    [ReadOnly(true)]
    public class TaskFile
    {
        [DataMember(Name = "entityId")]
        [ReadOnly(true)]
        public int? EntityId { get; set; }

        [DataMember(Name = "content")]
        [ReadOnly(true)]
        public string Content { get; set; }

    }
}
