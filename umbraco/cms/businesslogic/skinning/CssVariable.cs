using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.skinning
{
    public class CssVariable
    {
        public string Name { get; set; }
        public List<CssVariableProperty> Properties { get; set; }

        public CssVariable(string Name)
        {
            this.Name = Name;
            Properties = new List<CssVariableProperty>();
        }
    }
}
