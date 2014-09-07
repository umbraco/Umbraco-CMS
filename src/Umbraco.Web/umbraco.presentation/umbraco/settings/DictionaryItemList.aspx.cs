using System;
using System.Collections.Generic;
using umbraco.cms.businesslogic;
using Umbraco.Core;

namespace umbraco.presentation.settings {

    [WebformsPageTreeAuthorize(Constants.Trees.Dictionary)]
    public partial class DictionaryItemList : BasePages.UmbracoEnsuredPage {
        

        private readonly cms.businesslogic.language.Language[] _languages = cms.businesslogic.language.Language.getAll;
        private readonly cms.businesslogic.Dictionary.DictionaryItem[] _topItems = Dictionary.getTopMostItems;
        
        protected void Page_Load(object sender, EventArgs e) {
            
            
            string header = "<thead><tr><td>Key</td>";
            foreach (cms.businesslogic.language.Language lang in _languages) {
                header += "<td>" + lang.FriendlyName + "</td>";
            }
            header += "</tr></thead>";

            lt_table.Text = header;

            lt_table.Text += "<tbody>";

            ProcessKeys(_topItems, 0);

            lt_table.Text += "</tbody>";

        }

        private void ProcessKeys(IEnumerable<Dictionary.DictionaryItem> items, int level) {

            string style = "style='padding-left: " + level * 10 + "px;'"; 

            foreach (Dictionary.DictionaryItem di in items) {
                lt_table.Text += "<tr><th " + style + "><a href='editDictionaryItem.aspx?id=" + di.id.ToString() + "'>" + di.key + "</a></th>";
                foreach (cms.businesslogic.language.Language lang in _languages) {
                    lt_table.Text += "<td>";

                    if (string.IsNullOrEmpty(di.Value(lang.id)))
                        lt_table.Text += "<i class='icon-alert'></i>";
                    else
                        lt_table.Text += "<i class='icon-check'></i>";

                    lt_table.Text += "</td>";
                }
                lt_table.Text += "</tr>";

                if (di.hasChildren)
                    ProcessKeys(di.Children, (level+1));
            }

        }
    }
}
