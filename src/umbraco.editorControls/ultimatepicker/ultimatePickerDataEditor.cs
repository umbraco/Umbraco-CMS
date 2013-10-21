using System;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic;
using umbraco.interfaces;
using ClientDependency.Core.Controls;
using ClientDependency.Core;

namespace umbraco.editorControls.ultimatepicker
{
    [ValidationProperty("IsValid")]
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class ultimatePickerDataEditor : UpdatePanel, IDataEditor
    {
        private IData _data;
        string _configuration;

        private string[] config;
        private string controlType;

        private string[] dataValues;

        private TextBox childtxt;
        private DropDownList dropdownlistNodes;
        private CheckBoxList checkboxlistNodes;
        private RadioButtonList radiobuttonlistNodes;
        private ListBox listboxNodes;
        private Button clearRadiobuttonlist;
        private CheckBox clearRadiobuttons;

        public ultimatePickerDataEditor(umbraco.interfaces.IData Data, string Configuration)
        {
            _data = Data;
            _configuration = Configuration;

            config = _configuration.Split("|".ToCharArray());
            controlType = config[0];

            RenderMode = UpdatePanelRenderMode.Inline;
        }

        public virtual bool TreatAsRichTextEditor
        {
            get { return false; }
        }

        public bool ShowLabel
        {
            get { return true; }
        }

        /// <summary>
        /// Internal logic for validation controls to detect whether or not it's valid (has to be public though) 
        /// </summary>
        /// <value>Am I valid?</value>
        public string IsValid
        {
            get
            {
                switch (controlType.ToLower())
                {
                    case "autocomplete":
                        if (childtxt.Text.Contains("|"))
                        {
                            return childtxt.Text;
                        }

                        break;
                    case "checkboxlist":
                        foreach (ListItem item in checkboxlistNodes.Items)
                        {
                            if (item.Selected)
                            {
                                return "valid";
                            }
                        }
                        break;
                    case "dropdownlist":
                        return dropdownlistNodes.SelectedValue;
                    case "listbox":
                        foreach (ListItem item in listboxNodes.Items)
                        {
                            if (item.Selected)
                            {
                                return "valid";
                            }
                        }
                        break;
                    case "radiobuttonlist":
                        foreach (ListItem item in radiobuttonlistNodes.Items)
                        {
                            if (item.Selected)
                            {
                                return "valid";
                            }
                        }

                        break;

                }
                return "";
            }
        }

        public Control Editor { get { return this; } }

        public void Save()
        {
            string dataToSave = string.Empty;

            switch (controlType)
            {
                case "AutoComplete":
                    if (childtxt.Text.Contains("[") && childtxt.Text.Contains("]"))
                    {
                        dataToSave = childtxt.Text.Replace("]","").Split("[".ToCharArray())[1];
                    }

                    
                    break;
                case "auto-suggest":
                    goto case "AutoComplete";
                case "CheckBoxList":

                    foreach (ListItem item in checkboxlistNodes.Items)
                    {
                        if (item.Selected)
                        {
                            dataToSave += item.Value + ",";
                        }
                    }

                    if (dataToSave.Length > 0)
                    {
                        dataToSave = dataToSave.Substring(0, dataToSave.Length - 1);
                    }


                    break;
                case "checkbox":
                    goto case "CheckBoxList";
                case "DropDownList":
                    dataToSave = dropdownlistNodes.SelectedValue;
                    break;
                case "dropdown":
                    goto case "DropDownList";
                case "ListBox":
                    dataToSave = string.Empty;
                    foreach (ListItem item in listboxNodes.Items)
                    {
                        if (item.Selected)
                        {
                            dataToSave += item.Value + ",";
                        }
                    }

                    if (dataToSave.Length > 0)
                    {
                        dataToSave = dataToSave.Substring(0, dataToSave.Length - 1);
                    }
                    break;
                case "listbox":
                    goto case "ListBox";
                case "RadioButtonList":
                    dataToSave = string.Empty;

                    if (!clearRadiobuttons.Checked)
                    {
                        foreach (ListItem item in radiobuttonlistNodes.Items)
                        {
                            if (item.Selected)
                            {
                                dataToSave += item.Value + ",";
                            }
                        }

                        if (dataToSave.Length > 0)
                        {
                            dataToSave = dataToSave.Substring(0, dataToSave.Length - 1);
                        }
                    }
                    else
                    {
                        foreach (ListItem radiobutton in radiobuttonlistNodes.Items)
                        {
                            radiobutton.Selected = false;
                        }

                        clearRadiobuttons.Checked = false;
                    }

                    break;
                case "radiobox":
                    goto case "RadioButtonList";

            }
            if (controlType != "auto-suggest" && controlType != "AutoComplete")
            {
                this._data.Value = dataToSave;

            }
            else
            {
                if (dataToSave.Length > 0)
                {
                    this._data.Value = dataToSave;
                }else
                {
                    if (childtxt.Text.Trim().Length == 0)
                    {
                        this._data.Value = "";
                    }
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {
                int parentNodeId = Convert.ToInt32(config[1]);
                umbraco.cms.businesslogic.Content parentNode = new umbraco.cms.businesslogic.Content(parentNodeId);
                string documentAliasFilter = config[2];
                string[] documentAliasFilters = documentAliasFilter.Split(",".ToCharArray());

                bool showChildren = Convert.ToBoolean(config[3]);

                string[] datavalues = _data.Value.ToString().Split(",".ToCharArray());

                switch (controlType)
                {
                    case "AutoComplete":
                        setupAutoComplete(parentNodeId);
                        break;
                    case "auto-suggest":
                        goto case "AutoComplete";
                    case "CheckBoxList":
                        checkboxlistNodes = new CheckBoxList();
                        //checkboxlistNodes.ID = "nodes";
                        addListControlNode(parentNode, 1, showChildren, checkboxlistNodes, documentAliasFilters);
                        base.ContentTemplateContainer.Controls.Add(checkboxlistNodes);

                        break;
                    case "checkbox":
                        goto case "CheckBoxList";
                    case "DropDownList":
                        dropdownlistNodes = new DropDownList();
                        //dropdownlistNodes.ID = "nodes";
                        ListItem empty = new ListItem("");
                        dropdownlistNodes.Items.Add(empty);
                        addListControlNode(parentNode, 1, showChildren, dropdownlistNodes, documentAliasFilters);
                        foreach (string datavalue in datavalues)
                        {
                            dropdownlistNodes.SelectedValue = datavalue;
                        }
                        base.ContentTemplateContainer.Controls.Add(dropdownlistNodes);
                        break;
                    case "dropdown":
                        goto case "DropDownList";
                    case "ListBox":
                        listboxNodes = new ListBox();
                        //listboxNodes.ID = "nodes";
                        listboxNodes.SelectionMode = ListSelectionMode.Multiple;
                        listboxNodes.Width = 300;
                        listboxNodes.Height = 200;

                        addListControlNode(parentNode, 1, showChildren, listboxNodes, documentAliasFilters);
                        base.ContentTemplateContainer.Controls.Add(listboxNodes);
                        break;
                    case "listbox":
                        goto case "ListBox";
                    case "RadioButtonList":
                        radiobuttonlistNodes = new RadioButtonList();
                        radiobuttonlistNodes.AutoPostBack = true;
                        radiobuttonlistNodes.SelectedIndexChanged += new EventHandler(radiobuttonlistNodes_SelectedIndexChanged);
                        //radiobuttonlistNodes.ID = "nodes";
                        addListControlNode(parentNode, 1, showChildren, radiobuttonlistNodes, documentAliasFilters);

                        clearRadiobuttonlist = new Button();
                        clearRadiobuttonlist.Click += new EventHandler(clearRadiobuttonlist_Click);
                        clearRadiobuttonlist.Text = "Clear";

                        clearRadiobuttons = new CheckBox();
                        clearRadiobuttons.Visible = false;

                        base.ContentTemplateContainer.Controls.Add(radiobuttonlistNodes);
                        base.ContentTemplateContainer.Controls.Add(clearRadiobuttonlist);
                        base.ContentTemplateContainer.Controls.Add(clearRadiobuttons);
                        break;
                    case "radiobox":
                        goto case "RadioButtonList";
                }
            }
            catch { }
        }

        void radiobuttonlistNodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            clearRadiobuttons.Checked = false;
        }

        void clearRadiobuttonlist_Click(object sender, EventArgs e)
        {
           

            foreach (ListItem radiobutton in radiobuttonlistNodes.Items)
            {
                radiobutton.Selected = false;
            }

            clearRadiobuttons.Checked = true;
        }

        

        /// <summary>
        /// Adds sub nodes to the ListControl object passed into the method, based on the Content node passed in
        /// </summary>
        /// <param name="node">The node whos sub nodes are to be added to the ListControl</param>
        /// <param name="level">The level of the current node</param>
        /// <param name="showGrandChildren">Boolean determining if grand children should be displayed as well</param>
        /// <param name="control">The ListControl the nodes must be added to</param>
        /// <param name="documentAliasFilter">String representing the documentTypeAlias that should be filtered for. If empty no filter is applied</param>
        private void addListControlNode(umbraco.cms.businesslogic.Content node, int level, bool showGrandChildren, ListControl control, string[] documentAliasFilters)
        {
            if (node.HasChildren)
            {
                //store children array here because iterating over an Array property object is very inneficient.
                var c = node.Children;
                foreach (CMSNode child in c)
                {
                    umbraco.cms.businesslogic.Content doc = new umbraco.cms.businesslogic.Content(child.Id);
                    string preText = string.Empty;

                    for (int i = 1; i < level; i++)
                    {
                        preText += "- ";
                    }

                    //Run through the filters passed in
                    if (documentAliasFilters.Length > 0)
                    {
                        foreach (string filter in documentAliasFilters)
                        {
                            string trimmedFilter = filter.TrimStart(" ".ToCharArray());
                            trimmedFilter = trimmedFilter.TrimEnd(" ".ToCharArray());

                            if (doc.ContentType.Alias == trimmedFilter || trimmedFilter == string.Empty)
                            {
                                ListItem item = new ListItem(preText + doc.Text, doc.Id.ToString());
                                if (_data.Value.ToString().Contains(doc.Id.ToString()))
                                {
                                    item.Selected = true;
                                }
                                control.Items.Add(item);
                            }
                        }
                    }
                    else
                    {
                        ListItem item = new ListItem(preText + doc.Text, doc.Id.ToString());
                        if (_data.Value.ToString().Contains(doc.Id.ToString()))
                        {
                            item.Selected = true;
                        }
                        control.Items.Add(item);
                    }

                    if (showGrandChildren)
                    {
                        addListControlNode(doc, level + 1, showGrandChildren, control, documentAliasFilters);
                    }
                }
            }
        }

        /// <summary>
        /// Sets up the autocomplete functionality
        /// </summary>
        private void setupAutoComplete(int parentNodeId)
        {
            ClientDependencyLoader.Instance.RegisterDependency("Application/JQuery/jquery.autocomplete.js", "UmbracoClient", ClientDependencyType.Javascript);
            ClientDependencyLoader.Instance.RegisterDependency("css/umbracoGui.css", "UmbracoRoot", ClientDependencyType.Css);

            
            childtxt = new TextBox();
            childtxt.ID = "ultimatePickerBox" + base.ID;
            childtxt.AutoCompleteType = AutoCompleteType.Disabled;
            childtxt.CssClass = "umbEditorTextField";

            if (_data.Value.ToString().Length > 3)
            {
                try
                {
                    CMSNode firstSaved = new CMSNode(Convert.ToInt32(_data.Value.ToString()));
                    childtxt.Text = firstSaved.Text;
                }
                catch
                {

                }
            }

            base.ContentTemplateContainer.Controls.Add(childtxt);


            string autoCompleteScript =
                 "jQuery(\"#"
                 + childtxt.ClientID + "\").autocomplete(\""
                 + Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco)
                 + "/webservices/UltimatePickerAutoCompleteHandler.ashx\",{minChars: 2,max: 100, extraParams:{id:\"" + parentNodeId.ToString() + "\",showchildren:\"" + config[3] + "\",filter:\"" + config[2] + "\",rnd:\"" + DateTime.Now.Ticks + "\"}});";


            string autoCompleteInitScript =
                "jQuery(document).ready(function(){"
                + autoCompleteScript
                + "});";

            Page.ClientScript.RegisterStartupScript(GetType(), ClientID + "_ultimatepickerinit", autoCompleteInitScript, true);

            if (Page.IsPostBack)
            {
                ScriptManager.RegisterClientScriptBlock(this, GetType(), ClientID + "_ultimatepicker", autoCompleteScript, true);

            }

        }
    }
}
