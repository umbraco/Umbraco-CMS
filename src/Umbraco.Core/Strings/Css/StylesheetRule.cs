using System.Text;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Strings.Css;

public class StylesheetRule
{
    public string Name { get; set; } = null!;

    public string Selector { get; set; } = null!;

    public string Styles { get; set; } = null!;

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
            foreach (var style in
                     Styles?.Split(Constants.CharArrays.Semicolon, StringSplitOptions.RemoveEmptyEntries) ??
                     Array.Empty<string>())
            {
                sb.Append("\t").Append(style.StripNewLines().Trim()).Append(";").Append(Environment.NewLine);
            }
        }

        sb.Append("}");

        return sb.ToString();
    }
}
