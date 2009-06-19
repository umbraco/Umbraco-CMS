using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;

using umbraco.cms.businesslogic;

namespace umbraco.presentation.umbraco.dialogs {
    public partial class insertMasterpageContent : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {

            //Add a default Item
            ListItem li = new ListItem("Choose ID...");
            li.Selected = true;
            dd_detectedAlias.Items.Add(li);

            cms.businesslogic.template.Template t = new cms.businesslogic.template.Template(int.Parse(Request["id"]) );
            //string masterPageFile = Server.MapPath(t.MasterPageFile);

            if (t.MasterTemplate > 0) {
                t = new cms.businesslogic.template.Template(t.MasterTemplate);
                //masterPageFile = Server.MapPath(t.MasterPageFile);    
            }
            
            foreach(string cpId in t.contentPlaceholderIds()){
                dd_detectedAlias.Items.Add(cpId);
            }
            
            //string mp = System.IO.File.ReadAllText(masterPageFile);
           
            //string pat = "<asp:ContentPlaceHolder+(\\s+[a-zA-Z]+\\s*=\\s*(\"([^\"]*)\"|'([^']*)'))*\\s*/?>";
            
            /* Instantiate the regular expression object.
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);
     
            // Match the regular expression pattern against a text string.
            Match m = r.Match(mp);
            
            while (m.Success) {
                
                CaptureCollection cc = m.Groups[3].Captures;

                foreach (Capture c in cc) {
                    if(c.Value != "server")
                        dd_detectedAlias.Items.Add(c.Value);
                }

                m = m.NextMatch();
            }
            
            //just to be sure that they have something to select, we will add the default placeholder....
            
             * 
             */ 
            
            if(dd_detectedAlias.Items.Count == 1)
                dd_detectedAlias.Items.Add("ContentPlaceHolderDefault");


            
        } 

        protected override void OnPreInit(EventArgs e) {
            base.OnPreInit(e);
        }
    }
}
