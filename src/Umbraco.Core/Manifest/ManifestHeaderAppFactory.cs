using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Models.Header;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Manifest
{
    public class ManifestHeaderAppFactory : IHeaderAppFactory
    {
        private readonly ManifestHeaderAppDefinition _definition;

        private HeaderApp _app;
        private ShowRule[] _showRules;

        public ManifestHeaderAppFactory(ManifestHeaderAppDefinition definition)
        {
            _definition = definition;
        }

        public HeaderApp GetHeaderAppFor(IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            var rules = _showRules ?? (_showRules = ShowRule.Parse(_definition.Show).ToArray());
            var userGroupArray = userGroups.ToArray();

            if (!HasPermissionToApp(rules, userGroupArray))
                return null;

            if (_app is null)
            {
                _app = !string.IsNullOrWhiteSpace(_definition.View) ? new HeaderApp { View = _definition.View } : new DefaultHeaderApp { Action = _definition.Action };
                _app.Alias = _definition.Alias;
                _app.Name = _definition.Name;
                _app.Icon = _definition.Icon;
                _app.View = _definition.View;
                _app.Weight = _definition.Weight;
            }

            return _app;
        }

        private bool HasPermissionToApp(ShowRule[] rules, IReadOnlyUserGroup[] userGroups)
        {
            foreach (var rule in rules)
            {
                foreach (var group in userGroups)
                {
                    if (!rule.Matches("role", group.Alias))
                        continue;

                    if (!rule.Show)
                        return false;

                    return true;
                }
            }
            return false;
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
