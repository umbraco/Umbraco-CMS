using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Infrastructure.WebAssets;

/// <summary>
///     Creates a JavaScript block to initialize the back office
/// </summary>
public class BackOfficeJavaScriptInitializer
{
    // deal with javascript functions inside of json (not a supported json syntax)
    private const string PrefixJavaScriptObject = "@@@@";

    private static readonly Regex _jsFunctionParser = new(
        $"(\"{PrefixJavaScriptObject}(.*?)\")+",
        RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    // replace tokens in the js main
    private static readonly Regex _token = new("(\"##\\w+?##\")", RegexOptions.Compiled);

    /// <summary>
    ///     Gets the JS initialization script to boot the back office application
    /// </summary>
    /// <param name="scripts"></param>
    /// <param name="angularModule">
    ///     The angular module name to boot
    /// </param>
    /// <param name="globalSettings"></param>
    /// <param name="hostingEnvironment"></param>
    /// <returns></returns>
    public static string GetJavascriptInitialization(IEnumerable<string> scripts, string angularModule, GlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
    {
        var jarray = new StringBuilder();
        jarray.AppendLine("[");
        var first = true;
        foreach (var file in scripts)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                jarray.AppendLine(",");
            }

            jarray.Append("\"");
            jarray.Append(file);
            jarray.Append("\"");
        }

        jarray.Append("]");

        return WriteScript(jarray.ToString(), hostingEnvironment.ToAbsolute(globalSettings.UmbracoPath), angularModule);
    }

    /// <summary>
    ///     Parses the JsResources.Main and replaces the replacement tokens accordingly
    /// </summary>
    /// <param name="scripts"></param>
    /// <param name="umbracoPath"></param>
    /// <param name="angularModule"></param>
    /// <returns></returns>
    internal static string WriteScript(string scripts, string umbracoPath, string angularModule)
    {
        var count = 0;
        var replacements = new[] { scripts, umbracoPath, angularModule };

        // replace, catering for the special syntax when we have
        // js function() objects contained in the json
        return _token.Replace(Resources.Main, match =>
        {
            var replacement = replacements[count++];
            return _jsFunctionParser.Replace(replacement, "$2");
        });
    }
}
