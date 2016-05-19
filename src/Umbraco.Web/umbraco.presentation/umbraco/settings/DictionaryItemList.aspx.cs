using System;
using System.Collections.Generic;
using System.Linq;
using umbraco.cms.businesslogic;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace umbraco.presentation.settings {

    [WebformsPageTreeAuthorize(Constants.Trees.Dictionary)]
    public partial class DictionaryItemList : Umbraco.Web.UI.Pages.UmbracoEnsuredPage {
        

        private readonly cms.businesslogic.language.Language[] _languages = cms.businesslogic.language.Language.getAll;
        
        
        protected void Page_Load(object sender, EventArgs e) {
            
            
            string header = "<thead><tr><td>Key</td>";
            foreach (cms.businesslogic.language.Language lang in _languages) {
                header += "<td>" + lang.FriendlyName + "</td>";
            }
            header += "</tr></thead>";

            lt_table.Text = header;

            lt_table.Text += "<tbody>";

            ProcessKeys(Services.LocalizationService.GetRootDictionaryItems(), 0);

            lt_table.Text += "</tbody>";

        }

        private void ProcessKeys(IEnumerable<IDictionaryItem> dictionaryItems, int level) {

            string style = "style='padding-left: " + level * 10 + "px;'"; 

            foreach (var di in dictionaryItems) {
                lt_table.Text += "<tr><th " + style + "><a href='editDictionaryItem.aspx?id=" + di.Id.ToString() + "'>" + di.ItemKey + "</a></th>";

                foreach (var lang in _languages) {
                    lt_table.Text += "<td>";

                    var trans = di.Translations.FirstOrDefault(x => x.LanguageId == lang.id);
                    
                    if (trans == null || string.IsNullOrEmpty(trans.Value))
                        lt_table.Text += "<i class='icon-alert'></i>";
                    else
                        lt_table.Text += "<i class='icon-check'></i>";

                    lt_table.Text += "</td>";
                }
                lt_table.Text += "</tr>";

                var children = Services.LocalizationService.GetDictionaryItemChildren(di.Key);
                if (children.Any())
                    ProcessKeys(children, (level+1));
            }

        }
    }
}
