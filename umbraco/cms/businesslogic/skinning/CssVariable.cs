using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.skinning
{
    public class CssVariable
    {
        public string Name { get; set; }
        public string DefaultValue { get; set; }
        public List<CssVariableProperty> Properties { get; set; }

        public CssVariable(string name)
        {
            this.Name = name;
            Properties = new List<CssVariableProperty>();
        }

        public CssVariable(string name, string defaultValue)
        {
            this.Name = name;
            this.DefaultValue = defaultValue;

            Properties = new List<CssVariableProperty>();
        }
    }
}
