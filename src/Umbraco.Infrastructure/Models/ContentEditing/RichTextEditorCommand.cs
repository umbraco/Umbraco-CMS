using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "richtexteditorcommand", Namespace = "")]
    public class RichTextEditorCommand
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        
        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "mode")]
        public RichTextEditorCommandMode Mode { get; set; }
    }

    public enum RichTextEditorCommandMode
    {
        Insert,
        Selection,
        All
    }
}
