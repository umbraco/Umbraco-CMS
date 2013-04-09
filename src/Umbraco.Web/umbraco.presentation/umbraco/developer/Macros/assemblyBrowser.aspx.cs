using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Reflection;
using System.Collections.Specialized;
using Umbraco.Core.IO;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.macro;
using System.Collections.Generic;

namespace umbraco.developer
{
    /// <summary>
    /// Summary description for assemblyBrowser.
    /// </summary>
    public partial class assemblyBrowser : BasePages.UmbracoEnsuredPage
    {
        public assemblyBrowser()
        {
            CurrentApp = DefaultApps.developer.ToString();
        }
        protected void Page_Load(object sender, EventArgs e)
        {

            var isUserControl = false;
            var errorReadingControl = false;

            try
            {

                Type type = null;
                if (Request.QueryString["type"] == null)
                {
                    isUserControl = true;
                    var fileName = Request.QueryString["fileName"];
                    if (!fileName.StartsWith("~"))
                    {
                        if (fileName.StartsWith("/"))
                        {
                            fileName = "~" + fileName;
                        }
                        else
                        {
                            fileName = "~/" + fileName;
                        }
                    }
                    IOHelper.ValidateEditPath(fileName, SystemDirectories.UserControls);
                    
                    if (System.IO.File.Exists(IOHelper.MapPath(fileName)))
                    {
                        var oControl = (UserControl)LoadControl(fileName);

                        type = oControl.GetType();
                    }
                    else
                    {
                        errorReadingControl = true;
                        ChooseProperties.Visible = false;
                        AssemblyName.Text = "<span style=\"color: red;\">User control doesn't exist</span><br /><br />Please verify that you've copied the file to:<br />" + IOHelper.MapPath("~/" + fileName);
                    }
                }
                else
                {
                    var currentAss = IOHelper.MapPath(SystemDirectories.Bin + "/" + Request.QueryString["fileName"] + ".dll");
                    var asm = Assembly.LoadFrom(currentAss);
                    type = asm.GetType(Request.QueryString["type"]);
                }

                if (!errorReadingControl)
                {
                    string fullControlAssemblyName;
                    if (isUserControl)
                    {
                        AssemblyName.Text = "Choose Properties from " + type.BaseType.Name;
                        fullControlAssemblyName = type.BaseType.Namespace + "." + type.BaseType.Name;
                    }
                    else
                    {
                        AssemblyName.Text = "Choose Properties from " + type.Name;
                        fullControlAssemblyName = type.Namespace + "." + type.Name;
                    }


                    if (!IsPostBack && type != null)
                    {
                        MacroProperties.Items.Clear();
                        foreach (var pi in type.GetProperties())
                        {
                            if (pi.CanWrite && fullControlAssemblyName == pi.DeclaringType.Namespace + "." + pi.DeclaringType.Name)
                            {
                                MacroProperties.Items.Add(new ListItem(pi.Name + " <span style=\"color: #99CCCC\">(" + pi.PropertyType.Name + ")</span>", pi.PropertyType.Name));
                            }

                            foreach (ListItem li in MacroProperties.Items)
                                li.Selected = true;
                        }
                    }
        
                }
            }
            catch (Exception err)
            {
                AssemblyName.Text = "Error reading " + Request["fileName"];
                Button1.Visible = false;
                ChooseProperties.Controls.Add(new LiteralControl("<p class=\"guiDialogNormal\" style=\"color: red;\">" + err.ToString() + "</p><p/><p class=\"guiDialogNormal\">"));
            }

        }
        
        protected void Button1_Click(object sender, EventArgs e)
        {
            var result = "";

            // Get the macro object
            var macroObject = new Macro(Convert.ToInt32(Request.QueryString["macroID"]));

            // Load all macroPropertyTypes
            var macroPropertyTypes = new Hashtable();
            var macroPropertyIds = new Hashtable();

            var macroPropTypes = MacroPropertyType.GetAll;
            foreach (var mpt in macroPropTypes)
            {
                macroPropertyIds.Add(mpt.Alias, mpt.Id.ToString());

                macroPropertyTypes.Add(mpt.Alias, mpt.BaseType);
            }

            foreach (ListItem li in MacroProperties.Items)
            {
                if (li.Selected && !MacroHasProperty(macroObject, li.Text.Substring(0, li.Text.IndexOf(" ")).ToLower()))
                {
                    result += "<li>Added: " + SpaceCamelCasing(li.Text) + "</li>";
                    var macroPropertyTypeAlias = FindMacroType(macroPropertyTypes, li.Value);
                    if (macroPropertyTypeAlias == "")
                        macroPropertyTypeAlias = "text";
                    var propertyId = macroPropertyIds[macroPropertyTypeAlias];
                    if (propertyId != null)
                    {
                        var macroPropertyTypeId = 0;
                        if(int.TryParse(string.Format("{0}", propertyId), out macroPropertyTypeId))
                        {
                            MacroProperty.MakeNew(macroObject,
                                true,
                                li.Text.Substring(0, li.Text.IndexOf(" ")),
                                SpaceCamelCasing(li.Text),
                                macroPropTypes.Find(mpt => mpt.Id == macroPropertyTypeId));
                        }
                    }
                }
                else if (li.Selected)
                    result += "<li>Skipped: " + SpaceCamelCasing(li.Text) + " (already exists as a parameter)</li>";
            }
            ChooseProperties.Visible = false;
            ConfigProperties.Visible = true;
            resultLiteral.Text = result;
        }

        private static bool MacroHasProperty(Macro macroObject, string propertyAlias)
        {
            return macroObject.Properties.Any(mp => mp.Alias.ToLower() == propertyAlias);
        }

        private static string SpaceCamelCasing(string text)
        {
            var tempString = text.Substring(0, 1).ToUpper();
            for (var i = 1; i < text.Length; i++)
            {
                if (text.Substring(i, 1) == " ")
                    break;
                if (text.Substring(i, 1).ToUpper() == text.Substring(i, 1))
                    tempString += " ";
                tempString += text.Substring(i, 1);
            }
            return tempString;
        }

        private static string FindMacroType(Hashtable macroPropertyTypes, string baseTypeName)
        {
            var tempType = "";
            // Hard-code numeric values
            if (baseTypeName == "Int32")
                tempType = "number";
            else if (baseTypeName == "Decimal")
                tempType = "decimal";
            else if (baseTypeName == "String")
                tempType = "text";
            else if (baseTypeName == "Boolean")
                tempType = "bool";
            else
            {
                foreach (DictionaryEntry de in macroPropertyTypes)
                {
                    if (de.Value.ToString() == baseTypeName)
                    {
                        tempType = de.Key.ToString();
                        break;
                    }
                }
            }

            return tempType;
        }

    }

}
