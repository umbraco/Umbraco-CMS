using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "scriptFile", Namespace = "")]
    public class CodeFileDisplay : INotificationModel
    {
        [DataMember(Name = "virtualPath", IsRequired = true)]
        public string VirtualPath { get; set; }

        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }

        [DataMember(Name = "content", IsRequired = true)]
        public string Content { get; set; }

        [DataMember(Name = "fileType", IsRequired = true)]
        public string FileType { get; set; }

        [DataMember(Name = "snippet")]
        [ReadOnly(true)]
        public string Snippet { get; set; }

        public List<Notification> Notifications { get; private set; }
    }
}
