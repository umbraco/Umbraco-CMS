using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;

namespace Umbraco.Core.Manifest
{
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
    //       '+media/*'             // show for all media types
    //     ]
    //   },
    //   ...
    // ]

    /// <summary>
    /// Represents a content app definition, parsed from a manifest.
    /// </summary>
    [DataContract(Name = "appdef", Namespace = "")]
    public class ManifestContentAppDefinition : IContentAppDefinition
    {
        private string _view;
        private ContentApp _app;
        private ShowRule[] _showRules;

        /// <summary>
        /// Gets or sets the name of the content app.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique alias of the content app.
        /// </summary>
        /// <remarks>
        /// <para>Must be a valid javascript identifier, ie no spaces etc.</para>
        /// </remarks>
        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the weight of the content app.
        /// </summary>
        [DataMember(Name = "weight")]
        public int Weight { get; set; }

        /// <summary>
        /// Gets or sets the icon of the content app.
        /// </summary>
        /// <remarks>
        /// <para>Must be a valid helveticons class name (see http://hlvticons.ch/).</para>
        /// </remarks>
        [DataMember(Name = "icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the view for rendering the content app.
        /// </summary>
        [DataMember(Name = "view")]
        public string View
        {
            get => _view;
            set => _view = IOHelper.ResolveVirtualUrl(value);
        }

        /// <summary>
        /// Gets or sets the list of 'show' conditions for the content app.
        /// </summary>
        [DataMember(Name = "show")]
        public string[] Show { get; set; } = Array.Empty<string>();

        /// <inheritdoc />
        public ContentApp GetContentAppFor(object o)
        {
            string partA, partB;

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

                default:
                    return null;
            }

            var rules = _showRules ?? (_showRules = ShowRule.Parse(Show).ToArray());

            // if no 'show' is specified, then always display the content app
            if (rules.Length > 0)
            {
                var ok = false;

                // else iterate over each entry
                foreach (var rule in rules)
                {
                    // if the entry does not apply, skip it
                    if (!rule.Matches(partA, partB))
                        continue;

                    // if the entry applies,
                    // if it's an exclude entry, exit, do not display the content app
                    if (!rule.Show)
                        return null;

                    // else break - ok to display
                    ok = true;
                    break;
                }

                // when 'show' is specified, default is to *not* show the content app
                if (!ok)
                    return null;
            }

            // content app can be displayed
            return _app ?? (_app = new ContentApp
            {
                Alias = Alias,
                Name = Name,
                Icon = Icon,
                View = View,
                Weight = Weight
            });
        }

        private class ShowRule
        {
            private static readonly Regex ShowRegex = new Regex("^([+-])?([a-z]+)/([a-z0-9_]+|\\*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            public bool Show { get; private set; }
            public string PartA { get; private set; }
            public string PartB { get; private set; }

            public bool Matches(string partA, string partB)
            {
                return (PartA == "*" || PartA.InvariantEquals(partA)) && (PartB == "*" || PartB.InvariantEquals(partB));
            }

            public static IEnumerable<ShowRule> Parse(string[] rules)
            {
                foreach (var rule in rules)
                {
                    var match = ShowRegex.Match(rule);
                    if (!match.Success)
                        throw new FormatException($"Illegal 'show' entry \"{rule}\" in manifest.");

                    yield return new ShowRule
                    {
                        Show = match.Groups[1].Value != "-",
                        PartA = match.Groups[2].Value,
                        PartB = match.Groups[3].Value
                    };
                }
            }
        }
    }
}
