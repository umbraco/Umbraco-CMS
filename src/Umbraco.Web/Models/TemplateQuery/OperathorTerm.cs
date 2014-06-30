using System.Collections.Generic;

namespace Umbraco.Web.Models.TemplateQuery
{
    public class OperathorTerm
    {
        public OperathorTerm()
        {
            Name = "is";
            Operathor = Operathor.Equals;
            AppliesTo = new [] { "string" };
        }

        public OperathorTerm(string name, Operathor operathor, IEnumerable<string> appliesTo)
        {
            Name = name;
            Operathor = operathor;
            AppliesTo = appliesTo;
        }

        public string Name { get; set; }
        public Operathor Operathor { get; set; }
        public IEnumerable<string> AppliesTo { get; set; }
    }
}