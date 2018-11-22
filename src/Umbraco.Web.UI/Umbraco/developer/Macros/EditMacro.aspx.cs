using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using System.Linq;
using Umbraco.Web.UI.Pages;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web._Legacy.Controls;

namespace Umbraco.Web.UI.Umbraco.Developer.Macros
{
    public partial class EditMacro : UmbracoEnsuredPage
    {
        public EditMacro()
        {
            CurrentApp = Constants.Applications.Packages.ToString();
        }

        protected PlaceHolder Buttons;
        protected Table MacroElements;

        public TabPage InfoTabPage;
        public TabPage Parameters;

        private IMacro _macro;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _macro = Services.MacroService.GetById(Convert.ToInt32(Request.QueryString["macroID"]));

            if (IsPostBack == false)
            {
                ClientTools
                    .SyncTree("-1," + _macro.Id, false);

                PopulateFieldsOnLoad(_macro);

                // Load elements from macro
                MacroPropertyBind();

                PopulatePartialViewFiles();

                // Load usercontrols
                PopulateUserControls(IOHelper.MapPath(SystemDirectories.UserControls));
                userControlList.Items.Insert(0, new ListItem("Browse usercontrols on server...", string.Empty));

            }
        }

        /// <summary>
        /// Populates the control (textbox) values on page load
        /// </summary>
        /// <param name="macro"></param>
        protected void PopulateFieldsOnLoad(IMacro macro)
        {
            macroName.Text = macro.Name;
            macroAlias.Text = macro.Alias;
            macroKey.Text = macro.Key.ToString();
            cachePeriod.Text = macro.CacheDuration.ToInvariantString();
            macroRenderContent.Checked = macro.DontRender == false;
            macroEditor.Checked = macro.UseInEditor;
            cacheByPage.Checked = macro.CacheByPage;
            cachePersonalized.Checked = macro.CacheByMember;
            macroUserControl.Text = macro.MacroType == MacroTypes.UserControl ? macro.MacroSource : null;
            SelectedPartialView.Text = macro.MacroType == MacroTypes.PartialView ? macro.MacroSource : null;
        }

        /// <summary>
        /// Sets the values on the Macro object from the values posted back before saving the macro
        /// </summary>
        protected void SetMacroValuesFromPostBack(IMacro macro, int macroCachePeriod, string userControlValue, string partialViewValue)
        {
            macro.UseInEditor = macroEditor.Checked;
            macro.DontRender = macroRenderContent.Checked == false;
            macro.CacheByPage = cacheByPage.Checked;
            macro.CacheByMember = cachePersonalized.Checked;
            macro.CacheDuration = macroCachePeriod;
            macro.Alias = macroAlias.Text;
            macro.Name = macroName.Text;
            macro.MacroSource = !userControlValue.IsNullOrWhiteSpace() ? userControlValue : partialViewValue;
            macro.MacroType = !userControlValue.IsNullOrWhiteSpace() ? MacroTypes.UserControl : !partialViewValue.IsNullOrWhiteSpace() ? MacroTypes.PartialView : MacroTypes.Unknown;
        }

        public void DeleteMacroProperty(object sender, EventArgs e)
        {
            var macroPropertyId = (HtmlInputHidden)((Control)sender).Parent.FindControl("macroPropertyID");

            var property = _macro.Properties.Values.Single(x => x.Id == int.Parse(macroPropertyId.Value));
            _macro.Properties.Remove(property);

            Services.MacroService.Save(_macro);

            MacroPropertyBind();
        }

        public void MacroPropertyBind()
        {
            macroProperties.DataSource = _macro.Properties.Values.OrderBy(x => x.SortOrder);
            macroProperties.DataBind();
        }

        public object CheckNull(object test)
        {
            return Convert.IsDBNull(test) ? 0 : test;
        }

        protected IEnumerable<IDataEditor> GetMacroParameterEditors()
        {
            // we need to show the depracated ones for backwards compatibility
            // FIXME not managing deprecated here?!
            return Current.ParameterEditors;
        }

        public void macroPropertyCreate(object sender, EventArgs e)
        {
            //enable add validators
            var val1 = (RequiredFieldValidator)((Control)sender).Parent.FindControl("RequiredFieldValidator1");
            var val2 = (RequiredFieldValidator)((Control)sender).Parent.FindControl("RequiredFieldValidator4");
            var val3 = (RequiredFieldValidator)((Control)sender).Parent.FindControl("RequiredFieldValidator5");
            val1.Enabled = true;
            val2.Enabled = true;
            val3.Enabled = true;

            Page.Validate();

            if (Page.IsValid == false)
            {
                return;
            }

            var macroPropertyAliasNew = (TextBox)((Control)sender).Parent.FindControl("macroPropertyAliasNew");
            var macroPropertyNameNew = (TextBox)((Control)sender).Parent.FindControl("macroPropertyNameNew");
            var macroPropertyTypeNew = (DropDownList)((Control)sender).Parent.FindControl("macroPropertyTypeNew");

            if (macroPropertyAliasNew.Text != Services.TextService.Localize("general/new") + " " + Services.TextService.Localize("general/alias"))
            {
                if (_macro.Properties.ContainsKey(macroPropertyAliasNew.Text.Trim()))
                {
                    //don't continue
                    return;
                }

                _macro.Properties.Add(new MacroProperty(
                                          macroPropertyAliasNew.Text.Trim(),
                                          macroPropertyNameNew.Text.Trim(),
                                          _macro.Properties.Values.Any() ? _macro.Properties.Values.Max(x => x.SortOrder) + 1 : 0,
                                          macroPropertyTypeNew.SelectedValue));

                Services.MacroService.Save(_macro);

                MacroPropertyBind();
            }
        }

        public bool macroIsVisible(object isChecked)
        {
            return Convert.ToBoolean(isChecked);
        }

        public void AddChooseList(object sender, EventArgs e)
        {
            if (IsPostBack == false)
            {
                var dropDown = (DropDownList)sender;
                dropDown.Items.Insert(0, new ListItem("Choose...", string.Empty));
            }
        }

        private void PopulateUserControls(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.Exists == false) return;

            var rootDir = IOHelper.MapPath(SystemDirectories.UserControls);

            foreach (var uc in directoryInfo.GetFiles("*.ascx"))
            {
                userControlList.Items.Add(
                    new ListItem(SystemDirectories.UserControls +
                                 uc.FullName.Substring(rootDir.Length).Replace(IOHelper.DirSepChar, '/')));

            }
            foreach (var dir in directoryInfo.GetDirectories())
                PopulateUserControls(dir.FullName);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Tab setup
            InfoTabPage = TabView1.NewTabPage("Macro Properties");
            InfoTabPage.Controls.Add(Pane1);
            InfoTabPage.Controls.Add(Pane1_2);
            InfoTabPage.Controls.Add(Pane1_3);
            InfoTabPage.Controls.Add(Pane1_4);

            Parameters = TabView1.NewTabPage("Parameters");
            Parameters.Controls.Add(Panel2);

            //var save = TabView1.Menu.NewButton();
            //save.ButtonType = MenuButtonType.Primary;
            //save.Text = Services.TextService.Localize("save");
            //save.ID = "save";
            //save.Click += Save_Click;
        }

        void Save_Click(object sender, EventArgs e)
        {

            Page.Validate();

            ClientTools
                .SyncTree("-1," + _macro.Id.ToInvariantString(), true); //true forces the reload


            var tempCachePeriod = cachePeriod.Text;
            if (tempCachePeriod == string.Empty)
                tempCachePeriod = "0";
            
            SetMacroValuesFromPostBack(_macro, Convert.ToInt32(tempCachePeriod), macroUserControl.Text, SelectedPartialView.Text);

            // save elements
            // this is oh so completely broken
            var aliases = new Dictionary<string, string>();
            foreach (RepeaterItem item in macroProperties.Items)
            {
                var macroPropertyId = (HtmlInputHidden)item.FindControl("macroPropertyID");
                var macroElementName = (TextBox)item.FindControl("macroPropertyName");
                var macroElementAlias = (TextBox)item.FindControl("macroPropertyAlias");
                var macroElementSortOrder = (TextBox)item.FindControl("macroPropertySortOrder");
                var macroElementType = (DropDownList)item.FindControl("macroPropertyType");

                var prop = _macro.Properties.Values.Single(x => x.Id == int.Parse(macroPropertyId.Value));
                var sortOrder = 0;
                int.TryParse(macroElementSortOrder.Text, out sortOrder);

                var alias = macroElementAlias.Text.Trim();
                if (prop.Alias != alias) // changing the alias
                {
                    // use a temp alias to avoid collision if eg swapping aliases
                    var tempAlias = Guid.NewGuid().ToString("N").Substring(0, 8);
                    aliases[tempAlias] = alias;
                    alias = tempAlias;
                }

                _macro.Properties.UpdateProperty(
                    prop.Alias,
                    macroElementName.Text.Trim(),
                    sortOrder,
                    macroElementType.SelectedValue,
                    alias);
            }

            // now apply the real aliases, should not collide
            foreach (var kvp in aliases)
                _macro.Properties.UpdateProperty(kvp.Key, newAlias: kvp.Value);

            Services.MacroService.Save(_macro);

            ClientTools.ShowSpeechBubble(SpeechBubbleIcon.Save, "Macro saved", "");

            MacroPropertyBind();
        }
        
        /// <summary>
        /// Populate the drop down list for partial view files
        /// </summary>
        private void PopulatePartialViewFiles()
        {
            var partialsDir = IOHelper.MapPath(SystemDirectories.MvcViews + "/MacroPartials");
            //get all the partials in the normal /MacroPartials folder
            var foundMacroPartials = GetPartialViewFiles(partialsDir, partialsDir, SystemDirectories.MvcViews + "/MacroPartials");
            //now try to find all of them int he App_Plugins/[PackageName]/Views/MacroPartials folder
            var appPluginsFolder = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.AppPlugins));
            if (appPluginsFolder.Exists)
            {
                foreach (var d in appPluginsFolder.GetDirectories())
                {
                    var viewsFolder = d.GetDirectories("Views");
                    if (viewsFolder.Any())
                    {
                        var macroPartials = viewsFolder.First().GetDirectories("MacroPartials");
                        if (macroPartials.Any())
                        {
                            foundMacroPartials = foundMacroPartials.Concat(
                                GetPartialViewFiles(macroPartials.First().FullName, macroPartials.First().FullName, SystemDirectories.AppPlugins + "/" + d.Name + "/Views/MacroPartials"));
                        }
                    }
                }
            }


            PartialViewList.DataSource = foundMacroPartials;
            PartialViewList.DataBind();
            PartialViewList.Items.Insert(0, new ListItem("Browse partial view files on server...", string.Empty));
        }

        /// <summary>
        /// Get the list of partial view files in the ~/Views/MacroPartials folder and in all
        /// folders of ~/App_Plugins/[PackageName]/Views/MacroPartials
        /// </summary>
        /// <param name="orgPath"></param>
        /// <param name="path"></param>
        /// <param name="prefixVirtualPath"> </param>
        /// <returns></returns>
        private IEnumerable<string> GetPartialViewFiles(string orgPath, string path, string prefixVirtualPath)
        {
            var files = new List<string>();
            var dirInfo = new DirectoryInfo(path);

            // Populate subdirectories
            var dirInfos = dirInfo.GetDirectories();
            foreach (var dir in dirInfos)
            {
                files.AddRange(GetPartialViewFiles(orgPath, path + "/" + dir.Name, prefixVirtualPath));
            }

            var fileInfo = dirInfo.GetFiles("*.*");

            files.AddRange(
                fileInfo.Select(file =>
                    prefixVirtualPath.TrimEnd('/') + "/" + (path.Replace(orgPath, string.Empty).Trim('/') + "/" + file.Name).Trim('/')));
            return files;
        }

        /// <summary>
        /// Binds the drop down list but ensures that the macro param type exists if it doesn't the drop down will be left blank
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MacroPropertiesOnItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var propertyTypes = (DropDownList)e.Item.FindControl("macroPropertyType");

                var editors = GetMacroParameterEditors();
                propertyTypes.DataSource = editors;
                propertyTypes.DataBind();
                var macroProp = (IMacroProperty)e.Item.DataItem;
                if (editors.Any(x => x.Alias == macroProp.EditorAlias))
                {
                    propertyTypes.SelectedValue = macroProp.EditorAlias;
                }
            }

        }
    }
}
