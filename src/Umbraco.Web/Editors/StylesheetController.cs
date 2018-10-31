using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Strings.Css;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using StylesheetRule = Umbraco.Web.Models.ContentEditing.StylesheetRule;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for retrieving available stylesheets
    /// </summary>
    [PluginController("UmbracoApi")]
    public class StylesheetController : UmbracoAuthorizedJsonController
    {
        public IEnumerable<Stylesheet> GetAll()
        {
            return Services.FileService.GetStylesheets()
                .Select(x =>
                    new Stylesheet() {
                        Name = x.Alias,
                        Path = x.VirtualPath
                    });
        }

        public IEnumerable<StylesheetRule> GetRulesByName(string name)
        {
            var css = Services.FileService.GetStylesheetByName(name.EnsureEndsWith(".css"));
            if (css == null)
                return Enumerable.Empty<StylesheetRule>();

            return css.Properties.Select(x => new StylesheetRule() { Name = x.Name, Selector = x.Alias });
        }

        public StylesheetRule[] PostExtractStylesheetRules(StylesheetData data)
        {
            if (data.Content.IsNullOrWhiteSpace())
            {
                return new StylesheetRule[0];
            }

            return StylesheetHelper.ParseRules(data.Content)?.Select(rule => new StylesheetRule
            {
                Name = rule.Name,
                Selector = rule.Selector,
                Styles = rule.Styles
            }).ToArray();
        }

        public string PostInterpolateStylesheetRules(StylesheetData data)
        {
            // first remove all existing rules
            var existingRules = data.Content.IsNullOrWhiteSpace()
                ? new Core.Strings.Css.StylesheetRule[0]
                : StylesheetHelper.ParseRules(data.Content).ToArray();
            foreach (var rule in existingRules)
            {
                data.Content = StylesheetHelper.ReplaceRule(data.Content, rule.Name, null);
            }

            data.Content = data.Content.TrimEnd('\n', '\r');

            // now add all the posted rules
            if (data.Rules != null && data.Rules.Any())
            {
                foreach (var rule in data.Rules)
                {
                    data.Content = StylesheetHelper.AppendRule(data.Content, new Core.Strings.Css.StylesheetRule
                    {
                        Name = rule.Name,
                        Selector = rule.Selector,
                        Styles = rule.Styles
                    });
                }

                data.Content += Environment.NewLine;
            }

            return data.Content;
        }

        // this is an internal class for passing stylesheet data from the client to the controller while editing
        public class StylesheetData
        {
            public string Content { get; set; }

            public StylesheetRule[] Rules { get; set; }
        }
    }
}
