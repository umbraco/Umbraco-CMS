using System.Text.RegularExpressions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest;

// contentApps: [
//   {
//     name: 'App Name',        // required
//     alias: 'appAlias',       // required
//     weight: 0,               // optional, default is 0, use values between -99 and +99
//     icon: 'icon.app',        // required
//     view: 'path/view.htm',   // required
//     show: [                  // optional, default is always show
//       '-content/foo',        // hide for content type 'foo'
//       '+content/*',          // show for all other content types
//       '+media/*',            // show for all media types
//       '-member/foo'          // hide for member type 'foo'
//       '+member/*'            // show for all member types
//       '+role/admin'          // show for admin users. Role based permissions will override others.
//     ]
//   },
//   ...
// ]

/// <summary>
///     Represents a content app factory, for content apps parsed from the manifest.
/// </summary>
public class ManifestContentAppFactory : IContentAppFactory
{
    private readonly ManifestContentAppDefinition _definition;
    private readonly IIOHelper _ioHelper;

    private ContentApp? _app;
    private ShowRule[]? _showRules;

    public ManifestContentAppFactory(ManifestContentAppDefinition definition, IIOHelper ioHelper)
    {
        _definition = definition;
        _ioHelper = ioHelper;
    }

    /// <inheritdoc />
    public ContentApp? GetContentAppFor(object o, IEnumerable<IReadOnlyUserGroup> userGroups)
    {
        string? partA, partB;

        switch (o)
        {
            case IContent content:
                partA = "content";
                partB = content.ContentType.Alias;
                break;

            case IMedia media:
                partA = "media";
                partB = media.ContentType.Alias;
                break;
            case IMember member:
                partA = "member";
                partB = member.ContentType.Alias;
                break;
            case IContentType contentType:
                partA = "contentType";
                partB = contentType?.Alias;
                break;
            case IDictionaryItem _:
                partA = "dictionary";
                partB = "*"; // Not really a different type for dictionary items
                break;

            default:
                return null;
        }

        ShowRule[] rules = _showRules ??= ShowRule.Parse(_definition.Show).ToArray();
        var userGroupsList = userGroups.ToList();

        var okRole = false;
        var hasRole = false;
        var okType = false;
        var hasType = false;

        foreach (ShowRule rule in rules)
        {
            if (rule.PartA?.InvariantEquals("role") ?? false)
            {
                // if roles have been ok-ed already, skip the rule
                if (okRole)
                {
                    continue;
                }

                // remember we have role rules
                hasRole = true;

                foreach (IReadOnlyUserGroup group in userGroupsList)
                {
                    // if the entry does not apply, skip
                    if (!rule.Matches("role", group.Alias))
                    {
                        continue;
                    }

                    // if the entry applies,
                    // if it's an exclude entry, exit, do not display the content app
                    if (!rule.Show)
                    {
                        return null;
                    }

                    // else ok to display, remember roles are ok, break from userGroupsList
                    okRole = rule.Show;
                    break;
                }
            }

            // it is a type rule
            else
            {
                // if type has been ok-ed already, skip the rule
                if (okType)
                {
                    continue;
                }

                // remember we have type rules
                hasType = true;

                // if the entry does not apply, skip it
                if (!rule.Matches(partA, partB))
                {
                    continue;
                }

                // if the entry applies,
                // if it's an exclude entry, exit, do not display the content app
                if (!rule.Show)
                {
                    return null;
                }

                // else ok to display, remember type rules are ok
                okType = true;
            }
        }

        // if roles rules are specified but not ok,
        // or if type roles are specified but not ok,
        // cannot display the content app
        if ((hasRole && !okRole) || (hasType && !okType))
        {
            return null;
        }

        // else
        // content app can be displayed
        return _app ??= new ContentApp
        {
            Alias = _definition.Alias,
            Name = _definition.Name,
            Icon = _definition.Icon,
            View = _ioHelper.ResolveRelativeOrVirtualUrl(_definition.View),
            Weight = _definition.Weight,
        };
    }

    private class ShowRule
    {
        private static readonly Regex ShowRegex = new(
            "^([+-])?([a-z]+)/([a-z0-9_]+|\\*)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public bool Show { get; private set; }

        public string? PartA { get; private set; }

        public string? PartB { get; private set; }

        public static IEnumerable<ShowRule> Parse(string[] rules)
        {
            foreach (var rule in rules)
            {
                Match match = ShowRegex.Match(rule);
                if (!match.Success)
                {
                    throw new FormatException($"Illegal 'show' entry \"{rule}\" in manifest.");
                }

                yield return new ShowRule
                {
                    Show = match.Groups[1].Value != "-",
                    PartA = match.Groups[2].Value,
                    PartB = match.Groups[3].Value,
                };
            }
        }

        public bool Matches(string? partA, string? partB) =>
            (PartA == "*" || (PartA?.InvariantEquals(partA) ?? false)) &&
            (PartB == "*" || (PartB?.InvariantEquals(partB) ?? false));
    }
}
