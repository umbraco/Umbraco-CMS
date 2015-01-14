using System;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Strings.Css
{
    public class StylesheetRule
    {
        public StylesheetRule()
        { }

        //public HiveId StylesheetId { get; set; }

        //public HiveId RuleId { get; set; }

        public string Name { get; set; }

        public string Selector { get; set; }

        public string Styles { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("/*" + Environment.NewLine);
            sb.AppendFormat("   Name: {0}" + Environment.NewLine, Name);
            sb.Append("*/" + Environment.NewLine);
            sb.AppendFormat("{0} {{" + Environment.NewLine, Selector);
            sb.Append(string.Join(Environment.NewLine, string.IsNullOrWhiteSpace(Styles) == false ? string.Join(Environment.NewLine, Styles.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Select(x => "\t" + x)) + Environment.NewLine : ""));
            sb.Append("}");

            return sb.ToString();
        }
    }
}