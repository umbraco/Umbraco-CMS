﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Membership;

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
    //       '+media/*',            // show for all media types
    //       '-member/foo'          // hide for member type 'foo'
    //       '+member/*'            // show for all member types
    //       '+role/admin'          // show for admin users. Role based permissions will override others.
    //     ]
    //   },
    //   ...
    // ]

    /// <summary>
    /// Represents a content app factory, for content apps parsed from the manifest.
    /// </summary>
    public class ManifestContentAppFactory : IContentAppFactory
    {
        private readonly ManifestContentAppDefinition _definition;

        public ManifestContentAppFactory(ManifestContentAppDefinition definition)
        {
            _definition = definition;
        }

        private ContentApp _app;
        private ShowRule[] _showRules;

        /// <inheritdoc />
        public ContentApp GetContentAppFor(object o,IEnumerable<IReadOnlyUserGroup> userGroups)
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
                case IMember member:
                    partA = "member";
                    partB = member.ContentType.Alias;
                    break;

                default:
                    return null;
            }

            var rules = _showRules ?? (_showRules = ShowRule.Parse(_definition.Show).ToArray());
            var userGroupsList = userGroups.ToList();

            var okRole = false;
            var hasRole = false;
            var okType = false;
            var hasType = false;

            foreach (var rule in rules)
            {
                if (rule.PartA.InvariantEquals("role"))
                {
                    // if roles have been ok-ed already, skip the rule
                    if (okRole)
                        continue;

                    // remember we have role rules
                    hasRole = true;

                    foreach (var group in userGroupsList)
                    {
                        // if the entry does not apply, skip
                        if (!rule.Matches("role", group.Alias))
                            continue;

                        // if the entry applies,
                        // if it's an exclude entry, exit, do not display the content app
                        if (!rule.Show)
                            return null;

                        // else ok to display, remember roles are ok, break from userGroupsList
                        okRole = rule.Show;
                        break;
                    }
                }
                else // it is a type rule
                {
                    // if type has been ok-ed already, skip the rule
                    if (okType)
                        continue;

                    // remember we have type rules
                    hasType = true;

                    // if the entry does not apply, skip it
                    if (!rule.Matches(partA, partB))
                        continue;

                    // if the entry applies,
                    // if it's an exclude entry, exit, do not display the content app
                    if (!rule.Show)
                        return null;

                    // else ok to display, remember type rules are ok
                    okType = true;
                }
            }

            // if roles rules are specified but not ok,
            // or if type roles are specified but not ok,
            // cannot display the content app
            if ((hasRole && !okRole) || (hasType && !okType))
                return null;

            // else
            // content app can be displayed
            return _app ?? (_app = new ContentApp
            {
                Alias = _definition.Alias,
                Name = _definition.Name,
                Icon = _definition.Icon,
                View = _definition.View,
                Weight = _definition.Weight
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
