using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.skinning
{
    public class CssVariableProperty
    {
        public string Name { get; set; }
        public List<string> Selectors { get; set; }

        public CssVariableProperty(string Name)
        {
            this.Name = Name;
            Selectors = new List<string>();
        }
    }
}
