using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.macro;
using System.Reflection;
using System.Collections;

namespace umbraco.controls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:macroParameterControl runat=server></{0}:macroParameterControl>")]
    public class macroParameterControl : WebControl
    {
        #region private variables

        private Macro m_macro = null;
        private bool m_parameterValuesEnsured = false;
        private Hashtable m_parameterValues = new Hashtable();
        #endregion

        /// <summary>
        /// Gets the parameter values.
        /// </summary>
        /// <value>The parameter values.</value>
        public Hashtable ParameterValues
        {
            get {
                ensureMacroParameterList();
                return m_parameterValues; }
            set { m_parameterValues = value; }
        }

        [Bindable(true)]
        [Category("Umbraco")]
        [DefaultValue("")]
        [Localizable(true)]
        public string MacroAlias
        {
            get
            {
                String s = (String)ViewState["MacroAlias"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["MacroAlias"] = value;
            }
        }

        /// <summary>
        /// Updates the macro parameter.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="value">The value.</param>
        public void UpdateParameter(string alias, string value)
        {
            if (!m_parameterValues.ContainsKey(alias))
            {
                m_parameterValues.Add(alias, "");
            }

            m_parameterValues[alias] = value;
        }

        /// <summary>
        /// Gets the macro tag.
        /// </summary>
        /// <returns>The correct syntax for the macro including all parameters</returns>
        public string GetMacroTag() {
            string tag = "";
            if (UmbracoConfig.For.UmbracoSettings().Templates.UseAspNetMasterPages)
            {
                tag = "<umbraco:Macro runat=\"server\"";
            }
            else
            {
                tag = "<?UMBRACO_MACRO";
            }
            tag += String.Format(" macroAlias=\"{0}\"", MacroAlias);

            IDictionaryEnumerator ide = ParameterValues.GetEnumerator();
            while (ide.MoveNext())
            {
                tag += String.Format(" {0}=\"{1}\"", ide.Key.ToString(), ide.Value.ToString());
            }

            tag += " />";
            return tag;
        }

        private void ensureMacroParameterList()
        {
            if (!m_parameterValuesEnsured)
            {
                foreach (MacroProperty mp in m_macro.Properties)
                {
                    if (!m_parameterValues.ContainsKey(mp.Alias))
                        m_parameterValues.Add(mp.Alias, "");
                    m_parameterValues[mp.Alias] = (((umbraco.interfaces.IMacroGuiRendering)this.FindControl(mp.Alias)).Value);
                }
                m_parameterValuesEnsured = true;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            loadMacro();

            AddParameterControls();

        }

        private void AddParameterControls()
        {
            Controls.Add(new LiteralControl("<table border=\"0\" cellspacing=\"0\" cellpadding=\"0\">"));
            foreach (MacroProperty mp in m_macro.Properties)
            {
                string macroAssembly = mp.Type.Assembly;
                string macroType = mp.Type.Type;
                try
                {

                    Assembly assembly = Assembly.LoadFrom( IOHelper.MapPath(SystemDirectories.Bin + "/" + macroAssembly + ".dll"));

                    Type type = assembly.GetType(macroAssembly + "." + macroType);
                    var typeInstance = Activator.CreateInstance(type) as interfaces.IMacroGuiRendering;
                    if (typeInstance != null)
                    {
                        ((Control) typeInstance).ID = mp.Alias;
                        if (!Page.IsPostBack)
                        {
                            if (m_parameterValues.ContainsKey(mp.Alias))
                            {
                                typeInstance.Value = m_parameterValues[mp.Alias].ToString();
                            }
                            else
                            {
                                m_parameterValues.Add(mp.Alias, typeInstance.Value);
                            }
                        }

                        // register alias
                        //                        Controls.Add(new LiteralControl("<script>\nregisterAlias('" + control.ID + "');\n</script>"));
                        Controls.Add(new LiteralControl("<tr><th class=\"propertyHeader\" width=\"30%\">" + mp.Name + "</td><td class=\"propertyContent\">"));
                        Controls.Add((Control) typeInstance);
                        Controls.Add(new LiteralControl("</td></tr>"));
                    }
                }
                catch (Exception)
                {
                }
            }
            Controls.Add(new LiteralControl("</table>"));
        }

        private void loadMacro()
        {
            if (m_macro == null && MacroAlias != "")
            {
                m_macro = Macro.GetByAlias(MacroAlias);
                if (m_macro == null)
                {
                    throw new ArgumentException(String.Format("No macro with alias '{0}' found", MacroAlias));
                }
            }
        }

    }
}
