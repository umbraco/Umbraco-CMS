using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models.Validation;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "template", Namespace = "")]
    public class TemplateDisplay
    {

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [Required]
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [RequiredForPersistence]
        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }

        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "virtualPath")]
        public string VirtualPath { get; set; }

        [DataMember(Name = "masterTemplateAlias")]
        public string MasterTemplateAlias { get; set; }
    }
}
