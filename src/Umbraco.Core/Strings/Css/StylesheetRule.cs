using System;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Strings.Css
{
    internal class StylesheetRule
    {
        public string Name { get; set; }

        public string Selector { get; set; }

        public string Styles { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("/**");
            sb.AppendFormat("umb_name:{0}", Name);
            sb.Append("*/");
            sb.Append(Environment.NewLine);
            sb.Append(Selector);
            sb.Append("{");
            sb.Append(Styles.IsNullOrWhiteSpace() ? "" : Styles.Trim());
            sb.Append("}");

            return sb.ToString();
        }
    }
}