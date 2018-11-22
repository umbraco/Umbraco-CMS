using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using umbraco.interfaces;
using umbraco.editorControls;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.datatype;
using Umbraco.Core.IO;

namespace umbraco.editorControls.relatedlinks
{
    [ValidationProperty("IsValid")]
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class RelatedLinksDataEditor : UpdatePanel, IDataEditor
    {
        private umbraco.interfaces.IData _data;
        string _configuration;

        private ListBox _listboxLinks;
        private Button _buttonUp;
        private Button _buttonDown;
        private Button _buttonDelete;
        private TextBox _textboxLinkTitle;
        private CheckBox _checkNewWindow;
        private TextBox _textBoxExtUrl;
        private Button _buttonAddExtUrl;
        private Button _buttonAddIntUrlCP;
        private XmlDocument _xml;

        private pagePicker _pagePicker;
        private PagePickerDataExtractor _pagePickerExtractor;

        public RelatedLinksDataEditor(umbraco.interfaces.IData Data, string Configuration)
        {
            _data = Data;
            _configuration = Configuration;
        }

        public virtual bool TreatAsRichTextEditor
        {
            get { return false; }
        }

        /// <summary>
        /// Internal logic for validation controls to detect whether or not it's valid (has to be public though) 
        /// </summary>
        /// <value>Am I valid?</value>
        public string IsValid
        {
            get {
                if (_listboxLinks != null) {
                    if (_listboxLinks.Items.Count > 0)
                        return "Valid";
                }
                return "";
            }
        }

        public bool ShowLabel
        {
            get { return true; }
        }

        public Control Editor { get { return this; } }

        //Creates and saves a xml format of the content of the _listboxLinks
        // <links>
        //    <link type="external" title="google" link="http://google.com" newwindow="1" />
        //    <link type="internal" title="home" link="1234" newwindow="0" />
        // </links>
        //We could adapt the global xml at every adjustment, but this implementation is easier
        // (and possibly more efficient).
        public void Save()
        {
            XmlDocument doc = createBaseXmlDocument();
            XmlNode root = doc.DocumentElement;
            foreach (ListItem item in _listboxLinks.Items)
            {
                string value = item.Value;

                XmlNode newNode = doc.CreateElement("link");

                XmlNode titleAttr = doc.CreateNode(XmlNodeType.Attribute, "title", null);
                titleAttr.Value = item.Text;
                newNode.Attributes.SetNamedItem(titleAttr);

                XmlNode linkAttr = doc.CreateNode(XmlNodeType.Attribute, "link", null);
                linkAttr.Value = value.Substring(2);
                newNode.Attributes.SetNamedItem(linkAttr);

                XmlNode typeAttr = doc.CreateNode(XmlNodeType.Attribute, "type", null);
                if (value.Substring(0, 1).Equals("i"))
                    typeAttr.Value = "internal";
                else
                    typeAttr.Value = "external";
                newNode.Attributes.SetNamedItem(typeAttr);

                XmlNode windowAttr = doc.CreateNode(XmlNodeType.Attribute, "newwindow", null);
                if (value.Substring(1, 1).Equals("n"))
                    windowAttr.Value = "1";
                else
                    windowAttr.Value = "0";
                newNode.Attributes.SetNamedItem(windowAttr);

                root.AppendChild(newNode);
            }

            this._data.Value = doc.InnerXml;
        }

        //Draws the controls, only gets called for the first drawing of the page, not for each postback
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {
                _xml = new XmlDocument();
                _xml.LoadXml(_data.Value.ToString());

            }
            catch
            {
                _xml = createBaseXmlDocument();
            }

            _listboxLinks = new ListBox();
            _listboxLinks.ID = "links" + base.ID;
            _listboxLinks.Width = 400;
            _listboxLinks.Height = 140;
            foreach (XmlNode node in _xml.DocumentElement.ChildNodes)
            {
                string text = node.Attributes["title"].Value.ToString();
                string value = (node.Attributes["type"].Value.ToString().Equals("internal") ? "i" : "e")
                    + (node.Attributes["newwindow"].Value.ToString().Equals("1") ? "n" : "o")
                    + node.Attributes["link"].Value.ToString();
                _listboxLinks.Items.Add(new ListItem(text, value));
            }

            _buttonUp = new Button();
            _buttonUp.ID = "btnUp" + base.ID;
            _buttonUp.Text = umbraco.ui.GetText("relatedlinks", "modeUp");
            _buttonUp.Width = 80;
            _buttonUp.Click += new EventHandler(this.buttonUp_Click);

           
            _buttonDown = new Button();
            _buttonDown.ID = "btnDown" + base.ID;
            _buttonDown.Attributes.Add("style", "margin-top: 5px;");
            _buttonDown.Text = umbraco.ui.GetText("relatedlinks", "modeDown");
            _buttonDown.Width = 80;
            _buttonDown.Click += new EventHandler(this.buttonDown_Click);

            _buttonDelete = new Button();
            _buttonDelete.ID = "btnDel" + base.ID;
            _buttonDelete.Text = umbraco.ui.GetText("relatedlinks", "removeLink");
            _buttonDelete.Width = 80;
            _buttonDelete.Click += new EventHandler(this.buttonDel_Click);

            _textboxLinkTitle = new TextBox();
            _textboxLinkTitle.Width = 400;
            _textboxLinkTitle.ID = "linktitle" + base.ID;

            _checkNewWindow = new CheckBox();
            _checkNewWindow.ID = "checkNewWindow" + base.ID;
            _checkNewWindow.Checked = false;
            _checkNewWindow.Text = umbraco.ui.GetText("relatedlinks", "newWindow");

            _textBoxExtUrl = new TextBox();
            _textBoxExtUrl.Width = 400;
            _textBoxExtUrl.ID = "exturl" + base.ID;

            _buttonAddExtUrl = new Button();
            _buttonAddExtUrl.ID = "btnAddExtUrl" + base.ID;
            _buttonAddExtUrl.Text = umbraco.ui.GetText("relatedlinks", "addlink");
            _buttonAddExtUrl.Width = 80;
            _buttonAddExtUrl.Click += new EventHandler(this.buttonAddExt_Click);

            _buttonAddIntUrlCP = new Button();
            _buttonAddIntUrlCP.ID = "btnAddIntUrl" + base.ID;
            _buttonAddIntUrlCP.Text = umbraco.ui.GetText("relatedlinks", "addlink");
            _buttonAddIntUrlCP.Width = 80;
            _buttonAddIntUrlCP.Click += new EventHandler(this.buttonAddIntCP_Click);

            _pagePickerExtractor = new PagePickerDataExtractor();
            _pagePicker = new pagePicker(_pagePickerExtractor);
            _pagePicker.ID = "pagePicker" + base.ID;
            
            ContentTemplateContainer.Controls.Add(new LiteralControl("<div class=\"relatedlinksdatatype\" style=\"text-align: left;  padding: 5px;\"><table><tr><td rowspan=\"2\">"));
            ContentTemplateContainer.Controls.Add(_listboxLinks);
            ContentTemplateContainer.Controls.Add(new LiteralControl("</td><td style=\"vertical-align: top\">"));
            ContentTemplateContainer.Controls.Add(_buttonUp);
            ContentTemplateContainer.Controls.Add(new LiteralControl("<br />"));
            ContentTemplateContainer.Controls.Add(_buttonDown);
            ContentTemplateContainer.Controls.Add(new LiteralControl("</td></tr><tr><td style=\"vertical-align: bottom\">"));
            ContentTemplateContainer.Controls.Add(_buttonDelete);
            ContentTemplateContainer.Controls.Add(new LiteralControl("<br />"));
            ContentTemplateContainer.Controls.Add(new LiteralControl("</td></tr></table>"));

            // Add related links container
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<a href=\"javascript:;\" onClick=\"document.getElementById('{0}_addExternalLinkPanel').style.display='none';document.getElementById('{0}_addExternalLinkButton').style.display='none';document.getElementById('{0}_addLinkContainer').style.display='block';document.getElementById('{0}_addInternalLinkPanel').style.display='block';document.getElementById('{0}_addInternalLinkButton').style.display='block';\"><strong>{1}</strong></a>", ClientID, umbraco.ui.GetText("relatedlinks", "addInternal"))));
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format(" | <a href=\"javascript:;\" onClick=\"document.getElementById('{0}_addInternalLinkPanel').style.display='none';document.getElementById('{0}_addInternalLinkButton').style.display='none';document.getElementById('{0}_addLinkContainer').style.display='block';document.getElementById('{0}_addExternalLinkPanel').style.display='block';document.getElementById('{0}_addExternalLinkButton').style.display='block';\"><strong>{1}</strong></a>", ClientID, umbraco.ui.GetText("relatedlinks", "addExternal"))));

            // All urls
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<div id=\"{0}_addLinkContainer\" style=\"display: none; padding: 4px; border: 1px solid #ccc; margin-top: 5px;margin-right:10px;\">", ClientID)));
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<a href=\"javascript:;\" onClick=\"document.getElementById('{0}_addLinkContainer').style.display='none';\" style=\"border: none;\"><img src=\"{1}/images/close.png\" style=\"float: right\" /></a>", ClientID,  this.Page.ResolveUrl(SystemDirectories.Umbraco))));
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("{0}:<br />", umbraco.ui.GetText("relatedlinks", "caption"))));
            ContentTemplateContainer.Controls.Add(_textboxLinkTitle);
            ContentTemplateContainer.Controls.Add(new LiteralControl("<br />"));
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<div id=\"{0}_addExternalLinkPanel\" style=\"display: none; margin: 3px 0\">", ClientID)));
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("{0}:<br />", umbraco.ui.GetText("relatedlinks", "linkurl"))));
            ContentTemplateContainer.Controls.Add(_textBoxExtUrl);
            ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));

            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<div id=\"{0}_addInternalLinkPanel\" style=\"display: none; margin: 3px 0\">", ClientID)));
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("{0}:<br />", umbraco.ui.GetText("relatedlinks", "internalPage"))));
            ContentTemplateContainer.Controls.Add(_pagePicker);
            ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));

            ContentTemplateContainer.Controls.Add(new LiteralControl("<div style=\"margin: 5px 0\">"));
            ContentTemplateContainer.Controls.Add(_checkNewWindow);
            ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));

            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<div id=\"{0}_addInternalLinkButton\" style=\"display: none;\">", ClientID)));
            ContentTemplateContainer.Controls.Add(_buttonAddIntUrlCP);
            ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));

            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<div id=\"{0}_addExternalLinkButton\" style=\"display: none;\">", ClientID)));
            ContentTemplateContainer.Controls.Add(_buttonAddExtUrl);
            ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));

            ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));

            ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));

            resetInputMedia();
        }


        private XmlDocument createBaseXmlDocument()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("links");
            doc.AppendChild(root);
            return doc;
        }

        private void buttonUp_Click(Object o, EventArgs ea)
        {
            int index = _listboxLinks.SelectedIndex;
            if (index > 0) //not the first item
            {
                ListItem temp = _listboxLinks.SelectedItem;
                _listboxLinks.Items.RemoveAt(index);
                _listboxLinks.Items.Insert(index - 1, temp);
                _listboxLinks.SelectedIndex = index - 1;
            }
        }

        private void buttonDown_Click(Object o, EventArgs ea)
        {
            int index = _listboxLinks.SelectedIndex;
            if (index > -1 && index < _listboxLinks.Items.Count - 1) //not the last item
            {
                ListItem temp = _listboxLinks.SelectedItem;
                _listboxLinks.Items.RemoveAt(index);
                _listboxLinks.Items.Insert(index + 1, temp);
                _listboxLinks.SelectedIndex = index + 1;
            }
        }

        private void buttonDel_Click(Object o, EventArgs ea)
        {
            int index = _listboxLinks.SelectedIndex;
            if (index > -1)
            {
                _listboxLinks.Items.RemoveAt(index);
            }
        }

        private void buttonAddExt_Click(Object o, EventArgs ea)
        {
            string url = _textBoxExtUrl.Text.Trim();
            if (url.Length > 0 && _textboxLinkTitle.Text.Length > 0)
            {
                // use default HTTP protocol if no protocol was specified
                if (!(url.Contains("://")))
                {
                    url = "http://" + url;
                }

                string value = "e" + (_checkNewWindow.Checked ? "n" : "o") + url;
                _listboxLinks.Items.Add(new ListItem(_textboxLinkTitle.Text, value));
                resetInputMedia();
            }
        }

        private void buttonAddIntCP_Click(Object o, EventArgs ea)
        {
            _pagePicker.Save();
            if (!String.IsNullOrEmpty(_textboxLinkTitle.Text)
                && _pagePickerExtractor.Value != null
                && _pagePickerExtractor.Value.ToString() != "")
            {
                string value = "i" + (_checkNewWindow.Checked ? "n" : "o") + _pagePickerExtractor.Value.ToString();
                _listboxLinks.Items.Add(new ListItem(_textboxLinkTitle.Text, value));
                resetInputMedia();
                ScriptManager.RegisterClientScriptBlock(_pagePicker, _pagePicker.GetType(), "clearPagePicker", _pagePicker.ClientID + "_clear();", true);
            }
        }

        private void resetInputMedia()
        {
            _textBoxExtUrl.Text = "http://";
            _textboxLinkTitle.Text = "";
            _pagePickerExtractor.Value = "";
            
        }

    }

}
