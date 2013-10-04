using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "richtexteditorcommand", Namespace = "")]
    public class RichTextEditorCommand
    {
        [DataMember(Name = "icon")]
        public string Icon { get; set; }

        [DataMember(Name = "command")]
        public string Command { get; set; }
        
        [DataMember(Name = "alias")]
        public string Alias { get; set; }
        
        [DataMember(Name = "userInterface")]
        public string UserInterface { get; set; }
        
        [DataMember(Name = "frontEndCommand")]
        public string FrontEndCommand { get; set; }
        
        [DataMember(Name = "value")]
        public string Value { get; set; }
        
        [DataMember(Name = "priority")]
        public int Priority { get; set; }
        
        [DataMember(Name = "isStylePicker")]
        public bool IsStylePicker { get; set; }
    }
}
