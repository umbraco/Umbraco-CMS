using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Services.Description;
using Newtonsoft.Json.Linq;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.IO;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for retrieving available stylesheets
    /// </summary>
    [PluginController("UmbracoApi")]
    [DisableBrowserCache]
    public class StylesheetController : UmbracoAuthorizedJsonController
    {
        public IEnumerable<Stylesheet> GetAll()
        {
            return StyleSheet.GetAll()
                .Select(x => 
                    new Stylesheet() {
                        Name = x.Text,
                        Id = x.Id,
                        Path = SystemDirectories.Css + "/" + x.Text + ".css" 
                    });
        }

        public IEnumerable<StylesheetRule> GetRules(int id)
        {
            var css = StyleSheet.GetStyleSheet(id, true, true);
            if (css == null)
                return Enumerable.Empty<StylesheetRule>();

            return css.Properties.Select(x => new StylesheetRule() { Id = x.Id, Name = x.Text, Selector = x.Alias });
        }
        
        public IEnumerable<StylesheetRule> GetRulesByName(string name)
        {
            var css = StyleSheet.GetByName(name);

            if (css == null)
                return Enumerable.Empty<StylesheetRule>();

            return css.Properties.Select(x => new StylesheetRule() { Id = x.Id, Name = x.Text, Selector = x.Alias });
        }
    }

}
