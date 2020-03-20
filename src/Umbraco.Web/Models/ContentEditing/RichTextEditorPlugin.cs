using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "richtexteditorplugin", Namespace = "")]
    public class RichTextEditorPlugin
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
