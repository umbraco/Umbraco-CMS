using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;
using umbraco.interfaces;
using umbraco.cms.businesslogic.macro;
using umbraco.presentation.webservices;
using System.Text.RegularExpressions;
using ClientDependency.Core;
using System.Web;
using ClientDependency.Core.Controls;
using umbraco.presentation;

namespace umbraco.editorControls.macrocontainer
{

    [ClientDependency(ClientDependencyType.Javascript, "ui/jqueryui.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Css, "macroContainer/macroContainer.css", "UmbracoClient")]
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class Editor : UpdatePanel, IDataEditor
    {
        private IData _data;
        private List<string> _allowedMacros;
        private int _maxNumber, _preferedHeight, _preferedWidth;

        private LinkButton _addMacro;
        private Literal _limit;



        public Editor(IData data, List<string> allowedMacros, int maxNumber, int preferedHeight, int preferedWidth)
        {
            _data = data;
            _allowedMacros = allowedMacros;
            _maxNumber = maxNumber;
            _preferedHeight = preferedHeight;
            _preferedWidth = preferedWidth;

        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ajaxHelpers.EnsureLegacyCalls(base.Page);
            var sm = ScriptManager.GetCurrent(base.Page);
            var webservicePath = new ServiceReference(IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/webservices/MacroContainerService.asmx");

            if (!sm.Services.Contains(webservicePath))
                sm.Services.Add(webservicePath);

            _addMacro = new LinkButton();
            _addMacro.ID = ID + "_btnaddmacro";


            _addMacro.Click += _addMacro_Click;
            _addMacro.Text = ui.Text("insertMacro");
            _addMacro.CssClass = "macroContainerAdd";

            this.ContentTemplateContainer.Controls.Add(_addMacro);


            _limit = new Literal();
            _limit.Text = string.Format(" Only {0} macros are allowed", _maxNumber);
            _limit.ID = ID + "_litlimit";
            _limit.Visible = false;

            this.ContentTemplateContainer.Controls.Add(_limit);

            string widthHeight = "";
            if (_preferedHeight > 0 && _preferedWidth > 0)
            {
                widthHeight = String.Format(" style=\"min-width: {0}px; min-height: {1}px;\"", _preferedWidth, _preferedHeight);
            }

            this.ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<div id=\"" + ID + "container\" class=\"macrocontainer\"{0}>", widthHeight)));

            Regex tagregex = new Regex("<[^>]*(>|$)", RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
            MatchCollection tags = tagregex.Matches(_data.Value.ToString());

            List<int> editornumbers = new List<int>();
            string sortorder = string.Empty;


            for (int i = 0; i < _maxNumber; i++)
            {
                if (!editornumbers.Contains(i))
                {
                    string data = string.Empty;

                    if (tags.Count > i)
                        data = tags[i].Value;

                    MacroEditor macroEditor = new MacroEditor(data, _allowedMacros);
                    macroEditor.ID = ID + "macroeditor_" + i;

                    this.ContentTemplateContainer.Controls.Add(macroEditor);
                }


            }

            this.ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));

            if (tags.Count == _maxNumber)
            {
                _addMacro.Enabled = false;
                _limit.Visible = true;
            }



            MacroContainerEvent.Execute += new MacroContainerEvent.ExecuteHandler(MacroContainerEvent_Execute);

        }
        private void CheckLimit()
        {
            bool allowadd = false;
            for (int i = 0; i < _maxNumber; i++)
            {
                MacroEditor current = ((MacroEditor)this.ContentTemplateContainer.FindControl(ID + "macroeditor_" + i.ToString()));

                if (!current.Visible)
                {
                    allowadd = true;
                    break;
                };
            }

            if (!allowadd)
            {
                _addMacro.Enabled = false;
                _limit.Visible = true;
            }
            else
            {
                _addMacro.Enabled = true;
                _limit.Visible = false;
            }
        }

        private void MacroContainerEvent_Execute()
        {
            CheckLimit();

        }

        private void _addMacro_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < _maxNumber; i++)
            {
                MacroEditor current = ((MacroEditor)this.ContentTemplateContainer.FindControl(ID + "macroeditor_" + i.ToString()));

                if (!current.Visible)
                {
                    current.Visible = true;
                    MacroContainerEvent.Add();
                    break;
                };
            }
        }



        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // And a reference to the macro container calls 
            //ClientDependencyLoader.Instance.RegisterDependency("webservices/MacroContainerService.asmx/js", "UmbracoRoot", ClientDependencyType.Javascript);


            string script = "function " + ID + "makesortable(){   ";
            script += " jQuery('#" + ID + "container').sortable({ update : function () { ";
            script += " umbraco.presentation.webservices.MacroContainerService.SetSortOrder('" + ID + "', jQuery('#" + ID + "container').sortable('serialize'));";
            script += " }}); ";
            script += "  ";
            script += "}";

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), ID + "macrocontainersortable",
                script, true);

            if (!Page.IsPostBack)
                HttpContext.Current.Session[ID + "sortorder"] = null;

            ScriptManager.RegisterStartupScript(this, this.GetType(), ID + "initsort", ID + "makesortable();", true);

            string sortscript = string.Empty;

            string sortorder = string.Empty;
            if (HttpContext.Current.Session[ID + "sortorder"] != null)
            {
                sortorder = HttpContext.Current.Session[ID + "sortorder"].ToString();
            }
            if (sortorder != string.Empty)
            {

                foreach (string temp in sortorder.Split('&'))
                {
                    string number = temp.Substring(temp.LastIndexOf('=') + 1);


                    sortscript += "jQuery('#container" + ID + "macroeditor_" + number + "').appendTo('#" + ID + "container');";
                }
            }
            if (sortscript != string.Empty)
                ScriptManager.RegisterStartupScript(this, this.GetType(), ID + "resort", sortscript, true);

            EnsureChildControls();

        }





        #region IDataEditor Members

        public void Save()
        {



            string value = string.Empty;

            if (HttpContext.Current.Session[ID + "sortorder"] != null)
            {
                string sortorder = HttpContext.Current.Session[ID + "sortorder"].ToString();

                foreach (string temp in sortorder.Split('&'))
                {
                    string number = temp.Substring(temp.LastIndexOf('=') + 1);

                    if (this.ContentTemplateContainer.FindControl(ID + "macroeditor_" + number) != null)
                    {
                        MacroEditor current = ((MacroEditor)this.ContentTemplateContainer.FindControl(ID + "macroeditor_" + number));
                        if (current.Visible)
                            value += current.MacroTag;
                    }
                }
            }
            else
            {
                for (int i = 0; i < _maxNumber; i++)
                {
                    if (this.ContentTemplateContainer.FindControl(ID + "macroeditor_" + i.ToString()) != null)
                    {
                        MacroEditor current = ((MacroEditor)this.ContentTemplateContainer.FindControl(ID + "macroeditor_" + i.ToString()));
                        if (current.Visible)
                            value += current.MacroTag;
                    }
                }
            }
            _data.Value = value;

        }

        public bool ShowLabel
        {
            get { return true; }
        }

        public bool TreatAsRichTextEditor
        {
            get { return false; }
        }

        Control IDataEditor.Editor
        {
            get { return this; }
        }

        #endregion
    }
}
