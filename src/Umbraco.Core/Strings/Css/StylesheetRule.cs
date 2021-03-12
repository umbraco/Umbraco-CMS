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
            sb.Append(" {");
            sb.Append(Environment.NewLine);
            // append nicely formatted style rules
            // - using tabs because the back office code editor uses tabs
            if (Styles.IsNullOrWhiteSpace() == false)
            {
                // since we already have a string builder in play here, we'll append to it the "hard" way
                // instead of using string interpolation (for increased performance)
                foreach (var style in Styles.Split(Constants.CharArrays.Semicolon, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.Append("\t").Append(style.StripNewLines().Trim()).Append(";").Append(Environment.NewLine);
                }
            }
            sb.Append("}");

            return sb.ToString();
        }
    }
}
