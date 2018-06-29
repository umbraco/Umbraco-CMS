using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "richtexteditorplugin", Namespace = "")]
    public class RichTextEditorPlugin
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "useOnFrontend")]
        public bool UseOnFrontend { get; set; }
    }
}
