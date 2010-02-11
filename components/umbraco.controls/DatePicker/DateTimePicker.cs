using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.IO;
using System.Web.UI.HtmlControls;

namespace umbraco.uicontrols.DatePicker
{

    [ClientDependency(ClientDependencyType.Css, "DateTimePicker/jquery-ui-1.7.2.custom.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "ui/jquery.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "ui/jqueryui.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "DateTimePicker/timepicker.js", "UmbracoClient")]
    public class DateTimePicker : Control
    {
        /// <summary>
        /// Constructor, set defaults.
        /// </summary>
        public DateTimePicker()
        {
            ShowTime = true;
        }

        protected TextBox m_DateTextBox;

        public DateTime DateTime
        {
            get
            {
                EnsureChildControls();
                DateTime d = DateTime.MinValue;
                DateTime.TryParse(m_DateTextBox.Text, out d);
                return d;
            }
            set
            {
                EnsureChildControls();
                m_DateTextBox.Text = value.ToString();
            }
        }

        public bool ShowTime { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            var div = new HtmlGenericControl("div");
            div.Attributes.Add("class", "umbDateTimePicker");

            m_DateTextBox = new TextBox();
            div.Controls.Add(m_DateTextBox);

            this.Controls.Add(div);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            string js = @"
jQuery(document).ready(function() {  
     $('#" + m_DateTextBox.ClientID + @"').datepicker({  
         duration: '',  
         showTime: " + this.ShowTime.ToString().ToLower() + @",  
         constrainInput: false,
        buttonImage: '" + IOHelper.ResolveUrl(SystemDirectories.Umbraco_client) + "/DateTimePicker/images/calPickerIcon.png" + @"',
        buttonImageOnly: true,
        buttonText: 'Select date',
        showButtonPanel: false,
        showOn: 'button',
        changeYear: true,
        dateFormat: 'yy-mm-dd',
        time24h: true  
      });  
}); 
";

            try
            {
                if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "DateTime_" + this.ClientID, js, true);
                else
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "DateTime_" + this.ClientID, js, true);
            }
            catch
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "DateTime_" + this.ClientID, js, true);
            }

        }

    }
}
