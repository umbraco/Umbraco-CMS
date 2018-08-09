using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract(Name = "task", Namespace = "")]
    [ReadOnly(true)]
    public class TaskDisplay
    {
        [DataMember(Name = "id")]
        [ReadOnly(true)]
        public int Id { get; set; }

        [DataMember(Name = "assignedBy")]
        [ReadOnly(true)]
        public UserDisplay AssignedBy { get; set; }

        [DataMember(Name = "assignedTo")]
        [ReadOnly(true)]
        public UserDisplay AssignedTo { get; set; }

        [DataMember(Name = "createdDate")]
        [ReadOnly(true)]
        public DateTime CreatedDate { get; set; }

        [DataMember(Name = "closed")]
        [ReadOnly(true)]
        public bool Closed { get; set; }
        
        [DataMember(Name = "comment")]
        [ReadOnly(true)]
        public string Comment { get; set; }

        [DataMember(Name = "nodeId")]
        [ReadOnly(true)]
        public int NodeId{ get; set; }

        [DataMember(Name = "totalWords")]
        [ReadOnly(true)]
        public int TotalWords{ get; set; }

        [DataMember(Name = "properties")]
        [ReadOnly(true)]
        public List<PropertyDisplay> Properties { get; set; } = new List<PropertyDisplay>();

    }
}
