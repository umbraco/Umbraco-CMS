using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Reflection;
using System.Collections.Specialized;

using umbraco.cms.businesslogic.macro;
using System.Collections.Generic;
using umbraco.IO;

namespace umbraco.developer
{
    /// <summary>
    /// Summary description for assemblyBrowser.
    /// </summary>
    public partial class assemblyBrowser : BasePages.UmbracoEnsuredPage
    {

        private string _ConnString = GlobalSettings.DbDSN;
        protected void Page_Load(object sender, System.EventArgs e)
        {

            //			if (!IsPostBack) 
            //			{
            bool isUserControl = false;
            bool errorReadingControl = false;

            try
            {

                Type type = null;
                if (Request.QueryString["type"] == null)
                {
                    isUserControl = true;
                    string fileName = Request.QueryString["fileName"];

                    IOHelper.ValidateEditPath(fileName, SystemDirectories.Usercontrols);


                    if (System.IO.File.Exists(IOHelper.MapPath("~/" + fileName)))
                    {
                        UserControl oControl = (UserControl)LoadControl(@"~/" + fileName);

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
                    string currentAss = IOHelper.MapPath(SystemDirectories.Bin + "/" + Request.QueryString["fileName"] + ".dll");
                    Assembly asm = System.Reflection.Assembly.LoadFrom(currentAss);
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
                        foreach (PropertyInfo pi in type.GetProperties())
                        {
                            if (pi.CanWrite && fullControlAssemblyName == pi.DeclaringType.Namespace + "." + pi.DeclaringType.Name)
                            {
                                MacroProperties.Items.Add(new ListItem(pi.Name + " <span style=\"color: #99CCCC\">(" + pi.PropertyType.Name + ")</span>", pi.PropertyType.Name));

                                //						Response.Write("<li>" + pi.Name + ", " + pi.CanWrite.ToString() + ", " + pi.DeclaringType.Namespace+"."+pi.DeclaringType.Name + ", " + pi.PropertyType.Name + "</li>");
                            }

                            foreach (ListItem li in MacroProperties.Items)
                                li.Selected = true;
                        }
                    }
                    else if (type == null)
                    {
                        AssemblyName.Text = "Type '" + Request.QueryString["type"] + "' is null";
                    }
                }
            }
            catch (Exception err)
            {
                AssemblyName.Text = "Error reading " + Request["fileName"];
                Button1.Visible = false;
                ChooseProperties.Controls.Add(new LiteralControl("<p class=\"guiDialogNormal\" style=\"color: red;\">" + err.ToString() + "</p><p/><p class=\"guiDialogNormal\">"));
            }
            //			}


        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion

        protected void Button1_Click(object sender, System.EventArgs e)
        {
            string result = "";

            // Get the macro object
            umbraco.cms.businesslogic.macro.Macro macroObject =
                new umbraco.cms.businesslogic.macro.Macro(Convert.ToInt32(Request.QueryString["macroID"]));

            // Load all macroPropertyTypes
            Hashtable macroPropertyTypes = new Hashtable();
            Hashtable macroPropertyIds = new Hashtable();

            //            SqlDataReader dr = SqlHelper.ExecuteReader(_ConnString, CommandType.Text, "select id, macroPropertyTypeBaseType, macroPropertyTypeAlias from cmsMacroPropertyType");
            List<MacroPropertyType> macroPropTypes = MacroPropertyType.GetAll;
            foreach (MacroPropertyType mpt in macroPropTypes)
            {
                macroPropertyIds.Add(mpt.Alias, mpt.Id.ToString());

                macroPropertyTypes.Add(mpt.Alias, mpt.BaseType);
            }
            //            dr.Close();

            foreach (ListItem li in MacroProperties.Items)
            {
                if (li.Selected && !macrohasProperty(macroObject, li.Text.Substring(0, li.Text.IndexOf(" ")).ToLower()))
                {
                    result += "<li>Added: " + spaceCamelCasing(li.Text) + "</li>";
                    string _macroPropertyTypeAlias = findMacroType(macroPropertyTypes, li.Value);
                    if (_macroPropertyTypeAlias == "")
                        _macroPropertyTypeAlias = "text";

                    int macroPropertyTypeId = int.Parse(macroPropertyIds[_macroPropertyTypeAlias].ToString());

                    MacroProperty.MakeNew(macroObject,
                        true,
                        li.Text.Substring(0, li.Text.IndexOf(" ")),
                        spaceCamelCasing(li.Text),
                        macroPropTypes.Find(delegate(MacroPropertyType mpt) { return mpt.Id == macroPropertyTypeId; }));
                }
                else if (li.Selected)
                    result += "<li>Skipped: " + spaceCamelCasing(li.Text) + " (already exists as a parameter)</li>";
            }
            ChooseProperties.Visible = false;
            ConfigProperties.Visible = true;
            resultLiteral.Text = result;
        }

        private bool macrohasProperty(umbraco.cms.businesslogic.macro.Macro macroObject, string propertyAlias)
        {
            foreach (cms.businesslogic.macro.MacroProperty mp in macroObject.Properties)
                if (mp.Alias.ToLower() == propertyAlias)
                    return true;

            return false;
        }

        private string spaceCamelCasing(string text)
        {
            string _tempString = text.Substring(0, 1).ToUpper();
            for (int i = 1; i < text.Length; i++)
            {
                if (text.Substring(i, 1) == " ")
                    break;
                if (text.Substring(i, 1).ToUpper() == text.Substring(i, 1))
                    _tempString += " ";
                _tempString += text.Substring(i, 1);
            }
            return _tempString;
        }

        private string findMacroType(Hashtable macroPropertyTypes, string baseTypeName)
        {
            string _tempType = "";
            // Hard-code numeric values
            if (baseTypeName == "Int32")
                _tempType = "number";
            else if (baseTypeName == "Decimal")
                _tempType = "decimal";
            else if (baseTypeName == "String")
                _tempType = "text";
            else if (baseTypeName == "Boolean")
                _tempType = "bool";
            else
            {

                foreach (DictionaryEntry de in macroPropertyTypes)
                    if (de.Value.ToString() == baseTypeName)
                    {
                        _tempType = de.Key.ToString();
                        break;
                    }
            }

            return _tempType;
        }

    }

}
