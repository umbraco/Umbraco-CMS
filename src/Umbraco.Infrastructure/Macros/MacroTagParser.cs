using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Umbraco.Cms.Core.Xml;

namespace Umbraco.Cms.Infrastructure.Macros;

/// <summary>
///     Parses the macro syntax in a string and renders out it's contents
/// </summary>
public class MacroTagParser
{
    private static readonly Regex _macroRteContent = new(
        @"(<!--\s*?)(<\?UMBRACO_MACRO.*?/>)(\s*?-->)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    private static readonly Regex _macroPersistedFormat =
        new(
            @"(<\?UMBRACO_MACRO (?:.+?)??macroAlias=[""']([^""\'\n\r]+?)[""'].+?)(?:/>|>.*?</\?UMBRACO_MACRO>)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    /// <summary>
    ///     This formats the persisted string to something useful for the rte so that the macro renders properly since we
    ///     persist all macro formats like {?UMBRACO_MACRO macroAlias=\"myMacro\" /}
    /// </summary>
    /// <param name="persistedContent"></param>
    /// <param name="htmlAttributes">The HTML attributes to be added to the div</param>
    /// <returns></returns>
    /// <remarks>
    ///     This converts the persisted macro format to this:
    ///     {div class='umb-macro-holder'}
    ///     <!-- <?UMBRACO_MACRO macroAlias=\"myMacro\" /> -->
    ///     {ins}Macro alias: {strong}My Macro{/strong}{/ins}
    ///     {/div}
    /// </remarks>
    public static string FormatRichTextPersistedDataForEditor(
        string persistedContent,
        IDictionary<string, string> htmlAttributes) =>
        _macroPersistedFormat.Replace(persistedContent, match =>
        {
            if (match.Groups.Count >= 3)
            {
                // <div class="umb-macro-holder myMacro mceNonEditable">
                var alias = match.Groups[2].Value;
                var sb = new StringBuilder("<div class=\"umb-macro-holder ");

                // sb.Append(alias.ToSafeAlias());
                sb.Append("mceNonEditable\"");
                foreach (KeyValuePair<string, string> htmlAttribute in htmlAttributes)
                {
                    sb.Append(" ");
                    sb.Append(htmlAttribute.Key);
                    sb.Append("=\"");
                    sb.Append(htmlAttribute.Value);
                    sb.Append("\"");
                }

                sb.AppendLine(">");
                sb.Append("<!-- ");
                sb.Append(match.Groups[1].Value.Trim());
                sb.Append(" />");
                sb.AppendLine(" -->");
                sb.Append("<ins>");
                sb.Append("Macro alias: ");
                sb.Append("<strong>");
                sb.Append(alias);
                sb.Append("</strong></ins></div>");
                return sb.ToString();
            }

            // replace with nothing if we couldn't find the syntax for whatever reason
            return string.Empty;
        });

    /// <summary>
    ///     This formats the string content posted from a rich text editor that contains macro contents to be persisted.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     This is required because when editors are using the rte, the HTML that is contained in the editor might actually be
    ///     displaying
    ///     the entire macro content, when the data is submitted the editor will clear most of this data out but we'll still
    ///     need to parse it properly
    ///     and ensure the correct syntax is persisted to the db.
    ///     When a macro is inserted into the rte editor, the HTML will be:
    ///     {div class='umb-macro-holder'}
    ///     <!-- <?UMBRACO_MACRO macroAlias=\"myMacro\" /> -->
    ///     This could be some macro content
    ///     {/div}
    ///     What this method will do is remove the {div} and parse out the commented special macro syntax: {?UMBRACO_MACRO
    ///     macroAlias=\"myMacro\" /}
    ///     since this is exactly how we need to persist it to the db.
    /// </remarks>
    public static string FormatRichTextContentForPersistence(string rteContent)
    {
        if (string.IsNullOrEmpty(rteContent))
        {
            return string.Empty;
        }

        var html = new HtmlDocument();
        html.LoadHtml(rteContent);

        // get all the comment nodes we want
        HtmlNodeCollection? commentNodes = html.DocumentNode.SelectNodes("//comment()[contains(., '<?UMBRACO_MACRO')]");
        if (commentNodes == null)
        {
            // There are no macros found, just return the normal content
            return rteContent;
        }

        // replace each containing parent <div> with the comment node itself.
        foreach (HtmlNode? c in commentNodes)
        {
            HtmlNode? div = c.ParentNode;
            HtmlNode? divContainer = div.ParentNode;
            divContainer.ReplaceChild(c, div);
        }

        var parsed = html.DocumentNode.OuterHtml;

        // now replace all the <!-- and --> with nothing
        return _macroRteContent.Replace(parsed, match =>
        {
            if (match.Groups.Count >= 3)
            {
                // get the 3rd group which is the macro syntax
                return match.Groups[2].Value;
            }

            // replace with nothing if we couldn't find the syntax for whatever reason
            return string.Empty;
        });
    }

    /// <summary>
    ///     This will accept a text block and search/parse it for macro markup.
    ///     When either a text block or a a macro is found, it will call the callback method.
    /// </summary>
    /// <param name="text"> </param>
    /// <param name="textFoundCallback"></param>
    /// <param name="macroFoundCallback"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This method  simply parses the macro contents, it does not create a string or result,
    ///     this is up to the developer calling this method to implement this with the callbacks.
    /// </remarks>
    public static void ParseMacros(
        string text,
        Action<string> textFoundCallback,
        Action<string, Dictionary<string, string>> macroFoundCallback)
    {
        if (textFoundCallback == null)
        {
            throw new ArgumentNullException("textFoundCallback");
        }

        if (macroFoundCallback == null)
        {
            throw new ArgumentNullException("macroFoundCallback");
        }

        var elementText = text;

        var fieldResult = new StringBuilder(elementText);

        // NOTE: This is legacy code, this is definitely not the correct way to do a while loop! :)
        var stop = false;
        while (!stop)
        {
            var tagIndex = fieldResult.ToString().ToLower().IndexOf("<?umbraco", StringComparison.InvariantCulture);
            if (tagIndex > -1)
            {
                var tempElementContent = string.Empty;

                // text block found, call the call back method
                textFoundCallback(fieldResult.ToString().Substring(0, tagIndex));

                fieldResult.Remove(0, tagIndex);

                var tag = fieldResult.ToString().Substring(0, fieldResult.ToString().IndexOf(">", StringComparison.InvariantCulture) + 1);
                Dictionary<string, string> attributes = XmlHelper.GetAttributesFromElement(tag);

                // Check whether it's a single tag (<?.../>) or a tag with children (<?..>...</?...>)
                if (tag.Substring(tag.Length - 2, 1) != "/" && tag.IndexOf(" ", StringComparison.InvariantCulture) > -1)
                {
                    var closingTag = "</" + (tag.Substring(1, tag.IndexOf(" ", StringComparison.InvariantCulture) - 1)) + ">";

                    // Tag with children are only used when a macro is inserted by the umbraco-editor, in the
                    // following format: "<?UMBRACO_MACRO ...><IMG SRC="..."..></?UMBRACO_MACRO>", so we
                    // need to delete extra information inserted which is the image-tag and the closing
                    // umbraco_macro tag
                    if (fieldResult.ToString().IndexOf(closingTag, StringComparison.InvariantCulture) > -1)
                    {
                        fieldResult.Remove(0, fieldResult.ToString().IndexOf(closingTag, StringComparison.InvariantCulture));
                    }
                }

                var macroAlias = attributes.ContainsKey("macroalias") ? attributes["macroalias"] : attributes["alias"];

                // call the callback now that we have the macro parsed
                macroFoundCallback(macroAlias, attributes);

                fieldResult.Remove(0, fieldResult.ToString().IndexOf(">", StringComparison.InvariantCulture) + 1);
                fieldResult.Insert(0, tempElementContent);
            }
            else
            {
                // text block found, call the call back method
                textFoundCallback(fieldResult.ToString());

                stop = true; // break;
            }
        }
    }
}
