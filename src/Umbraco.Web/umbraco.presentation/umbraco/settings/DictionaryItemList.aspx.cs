using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace umbraco.presentation.settings {
    public partial class DictionaryItemList : BasePages.UmbracoEnsuredPage {
        public DictionaryItemList()
        {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();

        }

        private cms.businesslogic.language.Language[] languages = cms.businesslogic.language.Language.getAll;
        private cms.businesslogic.Dictionary.DictionaryItem[] topItems = cms.businesslogic.Dictionary.getTopMostItems;
        
        protected void Page_Load(object sender, EventArgs e) {
            
            
            string header = "<thead><tr><td>Key</td>";
            foreach (cms.businesslogic.language.Language lang in languages) {
                header += "<td>" + lang.FriendlyName + "</td>";
            }
            header += "</tr></thead>";

            lt_table.Text = header;

            lt_table.Text += "<tbody>";

            processKeys(topItems, 0);

            lt_table.Text += "</tbody>";

        }

        private void processKeys(cms.businesslogic.Dictionary.DictionaryItem[] items, int level) {

            string style = "style='padding-left: " + level * 10 + "px;'"; 

            foreach (cms.businesslogic.Dictionary.DictionaryItem di in items) {
                lt_table.Text += "<tr><th " + style + "><a href='editDictionaryItem.aspx?id=" + di.id.ToString() + "'>" + di.key + "</a></th>";
                foreach (cms.businesslogic.language.Language lang in languages) {
                    lt_table.Text += "<td>";

                    if (string.IsNullOrEmpty(di.Value(lang.id)))
                        lt_table.Text += "<i class='icon-alert'></i>";
                    else
                        lt_table.Text += "<i class='icon-check'></i>";

                    lt_table.Text += "</td>";
                }
                lt_table.Text += "</tr>";

                if (di.hasChildren)
                    processKeys(di.Children, (level+1));
            }

        }
    }
}
