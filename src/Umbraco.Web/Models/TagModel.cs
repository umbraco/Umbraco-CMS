﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models
{
    [DataContract(Name = "tag", Namespace = "")]
    public class TagModel
    {
        [DataMember(Name = "id", IsRequired = true)]
        public int Id { get; set; }

        [DataMember(Name = "text", IsRequired = true)]
        public string Text { get; set; }

        [DataMember(Name = "group")]
        public string Group { get; set; }

        [DataMember(Name = "nodeCount")]
        public int NodeCount { get; set; }
    }
}
