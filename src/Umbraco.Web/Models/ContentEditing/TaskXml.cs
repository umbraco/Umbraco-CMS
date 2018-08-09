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
        [DataMember(Name = "id")]
        [ReadOnly(true)]
        public int Id { get; set; }

        [DataMember(Name = "nodeId")]
        [ReadOnly(true)]
        public int NodeId { get; set; }

        [DataMember(Name = "filename")]
        [ReadOnly(true)]
        public string Filename { get; set; }

        [DataMember(Name = "content")]
        [ReadOnly(true)]
        public string Content { get; set; }

    }
}
