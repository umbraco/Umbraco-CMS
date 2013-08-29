using System;
using ClientDependency.Core;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace umbraco.uicontrols.DatePicker
{

    [ClientDependency(ClientDependencyType.Css, "DateTimePicker/datetimepicker.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "ui/jquery.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "MaskedInput/jquery.maskedinput-1.3.min.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "ui/jqueryui.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "DateTimePicker/timepicker.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "DateTimePicker/umbDateTimePicker.js", "UmbracoClient")]
    public class DateTimePicker : Control
    {
        /// <summary>
        /// Constructor, set defaults.
        /// </summary>
        public DateTimePicker()
        {
            ShowTime = true;
            DateTime = DateTime.MinValue;
        }

        protected TextBox m_DateTextBox;
        protected HtmlGenericControl m_InfoDiv;
        protected HtmlAnchor m_ClearDate;

        public string Text
        {
            get
            {
                return m_DateTextBox.Text;
            }
        }

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
                if (value == DateTime.MinValue)
                {
                    m_DateTextBox.Text = "";
                }
                else
                {
                    m_DateTextBox.Text = value.ToString(ShowTime ? "yyyy-MM-dd HH:mm" : "yyyy-MM-dd");
                }
            }
        }

        public bool ShowTime { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnableViewState = false;
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            var div = new HtmlGenericControl("div");
            div.Attributes.Add("class", "umbDateTimePicker");

            m_DateTextBox = new TextBox();
            div.Controls.Add(m_DateTextBox);

            m_InfoDiv = new HtmlGenericControl("div");
            m_InfoDiv.InnerText = ui.Text("noDate"); ;
            div.Controls.Add(m_InfoDiv);

            m_ClearDate = new HtmlAnchor();
            m_ClearDate.InnerText = ui.Text("removeDate");
            div.Controls.Add(m_ClearDate);

            this.Controls.Add(div);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.DateTime != DateTime.MinValue)
            {
                m_InfoDiv.Visible = false;
            }
            else
            {
                m_ClearDate.Visible = false;
            }


            string js = @"
jQuery(document).ready(function() {  
    jQuery('#" + m_DateTextBox.ClientID + @"').umbDateTimePicker(" 
               + this.ShowTime.ToString().ToLower() + ",'" 
               + ui.Text("choose") + " " + ui.Text("date") + "','"
               + ui.Text("noDate") + "','"
               + ui.Text("removeDate") + "');"
               + "jQuery('#" + m_DateTextBox.ClientID + "').mask('"+ (this.ShowTime ? "9999-99-99 99:99" : "9999-99-99")  +"');" 
               + "});";
    
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
