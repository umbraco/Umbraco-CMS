using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Umbraco.Core.PropertyEditors
{
    public class SearchDataValue
    {
        public string FieldName { get; set; }
        public string Value { get; set; }
        public bool Analyze { get; set; }
        public bool Store { get; set; }

        public SearchDataValue()
        {
        }

        public SearchDataValue(string fieldName, string value, bool analyze = true, bool store = true)
        {
            FieldName = fieldName;
            Value = value;
            Analyze = analyze;
            Store = store;
        }

        public XElement ToXml()
        {
            var xElement = new XElement(FieldName, Value);
            xElement.Add(new XAttribute("analyze", Analyze));
            xElement.Add(new XAttribute("store", Store));
            return xElement;
        }
    }
}
