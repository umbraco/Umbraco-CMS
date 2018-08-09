using System.ComponentModel;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract(Name = "task", Namespace = "")]
    [ReadOnly(true)]
    public class ImportTaskResult
    {
        [DataMember(Name = "taskId")]
        [ReadOnly(true)]
        public int TaskId { get; set; }

        [DataMember(Name = "entityId")]
        [ReadOnly(true)]
        public int? EntityId { get; set; }
    }
}
