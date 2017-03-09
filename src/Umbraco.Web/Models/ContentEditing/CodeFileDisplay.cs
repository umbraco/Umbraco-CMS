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
        /// <summary>
        /// VirtualPath is the path to the file on disk
        /// /views/partials/file.cshtml
        /// </summary>
        [DataMember(Name = "virtualPath", IsRequired = true)]
        public string VirtualPath { get; set; }

        /// <summary>
        /// Path represents the path used by the backoffice tree
        /// For files stored on disk, this is a urlencoded, comma seperated
        /// path to the file, always starting with -1.
        /// 
        /// -1,Partials,Parials%2FFolder,Partials%2FFolder%2FFile.cshtml 
        /// </summary>
        [DataMember(Name = "path")]
        [ReadOnly(true)]
        public string Path { get; set; }

        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }

        [DataMember(Name = "content", IsRequired = true)]
        public string Content { get; set; }

        [DataMember(Name = "fileType", IsRequired = true)]
        public string FileType { get; set; }

        [DataMember(Name = "snippet")]
        [ReadOnly(true)]
        public string Snippet { get; set; }

        [DataMember(Name = "id")]
        [ReadOnly(true)]
        public string Id { get; set; }

        public List<Notification> Notifications { get; private set; }
    }
}
