using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "memberType", Namespace = "")]
    public class MemberTypeDisplay : ContentTypeBasic
    {
        public MemberTypeDisplay()
        {
            //initialize collections so at least their never null
            Groups = new List<PropertyGroupDisplay>();
        }

        //name, alias, icon, thumb, desc, inherited from basic

        //Tabs
        [DataMember(Name = "groups")]
        public IEnumerable<PropertyGroupDisplay> Groups { get; set; }
    }
}
