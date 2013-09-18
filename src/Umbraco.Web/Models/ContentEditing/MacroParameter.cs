using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a macro parameter with an editor
    /// </summary>
    [DataContract(Name = "macroParameter", Namespace = "")]
    public class MacroParameter
    {
        [DataMember(Name = "alias", IsRequired = true)]
        [Required]
        public string Alias { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
        
        /// <summary>
        /// The editor view to render for this parameter
        /// </summary>
        [DataMember(Name = "view", IsRequired = true)]
        [Required(AllowEmptyStrings = false)]
        public string View { get; set; }

    }
}
