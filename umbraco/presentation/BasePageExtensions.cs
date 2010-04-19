using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.BasePages;
using umbraco.uicontrols;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using ClientDependency.Core;

namespace umbraco.presentation
{
    /// <summary>
    /// Extension methods for the Umbraco BasePage
    /// </summary>
    public static class BasePageExtensions
    {

        /// <summary>
        /// Used to display an error message to the user and disable further execution.
        /// This will remove all controls from being rendered and show a feedback control with an error
        /// </summary>
        /// <param name="msg"></param>
        public static void DisplayFatalError(this BasePage page, string msg)
        {
            foreach (var ctl in page.Controls.Cast<Control>())
            {
                if (!HideControls(ctl))
                {
                    var ctls = ctl.FlattenChildren();
                    foreach (var c in ctls)
                    {
                        HideControls(c);
                    }
                }
            }
            var feedback = new Feedback();
            feedback.type = Feedback.feedbacktype.error;
            feedback.Text = string.Format("<b>{0}</b><br/><br/>{1}", ui.GetText("error"), msg);
            page.Controls.Add(feedback);
        }

        private static bool HideControls(this Control c)
        {
            if (c is MasterPage) 
            { 
                return false;            
            }
            else if (c is UserControl || c is WebControl || c is HtmlForm)
            {
                c.Visible = false;
                return true;
            }
            return false;

        }

    }
}
