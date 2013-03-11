using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using umbraco.cms.businesslogic.macro;
using System.Web;

namespace umbraco.presentation.templateControls
{

    [DefaultProperty("Alias")]
    [ToolboxData("<{0}:Macro runat=server></{0}:Macro>")]
    [PersistChildren(false)]
    [ParseChildren(true, "Text")]
    public class Macro : WebControl, ITextControl
    {

        private Hashtable m_Attributes = new Hashtable();
        private string m_Code = String.Empty;

        public Hashtable MacroAttributes
        {
            get
            {
                //                Hashtable attributes = (Hashtable)ViewState["Attributes"];
                return m_Attributes;
            }
            set
            {
                m_Attributes = value;
            }
        }


        [Bindable(true)]
        [Category("Umbraco")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Alias
        {
            get
            {
                String s = (String)ViewState["Alias"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["Alias"] = value;
            }
        }

        [Bindable(true)]
        [Category("Umbraco")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Language {
            get {
                var s = (String)ViewState["Language"];
                return (s ?? String.Empty);
            }
            set {
                ViewState["Language"] = value.ToLower();
            }
        }

        [Bindable(true)]
        [Category("Umbraco")]
        [DefaultValue("")]
        [Localizable(true)]
        public string FileLocation {
            get {
                var s = (String)ViewState["FileLocation"];
                return (s ?? String.Empty);
            }
            set {
                ViewState["FileLocation"] = value.ToLower();
            }
        }
		[Bindable(true)]
		[Category("Umbraco")]
		[DefaultValue(RenderEvents.Init)]
		[Localizable(true)]
		public RenderEvents RenderEvent
		{
			get
			{
				RenderEvents renderEvent = RenderEvents.Init;
				if (ViewState["RenderEvent"] != null)
					renderEvent = (RenderEvents)ViewState["RenderEvent"];
				return renderEvent;
			}
			set
			{
				ViewState["RenderEvent"] = value;
			}
		}

		// Indicates where to run EnsureChildControls and effectively render the macro
		public enum RenderEvents
		{
			Init,
			PreRender,
			Render
		}

        public IList<Exception> Exceptions = new List<Exception>();

		/// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e) {
            base.OnInit(e);

            // Create child controls when told to - this is the default
			if (this.RenderEvent == RenderEvents.Init)
	            EnsureChildControls();
        }

		// Create child controls when told to - new option to render at PreRender
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			if (this.RenderEvent == RenderEvents.PreRender)
				EnsureChildControls();
		}

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls() {
            // collect all attributes set on the control
            var keys = Attributes.Keys;
            foreach (string key in keys)
                MacroAttributes.Add(key.ToLower(), HttpUtility.HtmlDecode(Attributes[key]));

            if (!MacroAttributes.ContainsKey("macroalias") && !MacroAttributes.ContainsKey("macroAlias"))
                MacroAttributes.Add("macroalias", Alias);

            // set pageId to int.MinValue if no pageID was found,
            // e.g. if the macro was rendered on a custom (non-Umbraco) page
            int pageId = Context.Items["pageID"] == null ? int.MinValue : int.Parse(Context.Items["pageID"].ToString());

            if ((!String.IsNullOrEmpty(Language) && Text != "") || !string.IsNullOrEmpty(FileLocation)) {
                var tempMacro = new macro();
                tempMacro.GenerateMacroModelPropertiesFromAttributes(MacroAttributes);
                if (string.IsNullOrEmpty(FileLocation)) {
                    tempMacro.Model.ScriptCode = Text;
                    tempMacro.Model.ScriptLanguage = Language;
                } else {
                    tempMacro.Model.ScriptName = FileLocation;
                }
                tempMacro.Model.MacroType = MacroTypes.Script;
                if (!String.IsNullOrEmpty(Attributes["Cache"])) {
                    var cacheDuration = 0;
                    if (int.TryParse(Attributes["Cache"], out cacheDuration))
                        tempMacro.Model.CacheDuration = cacheDuration;
                    else
                        System.Web.HttpContext.Current.Trace.Warn("Template", "Cache attribute is in incorect format (should be an integer).");
                }
                var c = tempMacro.renderMacro((Hashtable)Context.Items["pageElements"], pageId);
                if (c != null)
                {
                    Exceptions = tempMacro.Exceptions;

                    Controls.Add(c);
                }
                else
                    System.Web.HttpContext.Current.Trace.Warn("Template", "Result of inline macro scripting is null");
            
            } else {
                var tempMacro = macro.GetMacro(Alias);
                if (tempMacro != null) {
                    try {
                        var c = tempMacro.renderMacro(MacroAttributes, (Hashtable)Context.Items["pageElements"], pageId);
                        if (c != null)
                            Controls.Add(c);
                        else
                            System.Web.HttpContext.Current.Trace.Warn("Template", "Result of macro " + tempMacro.Name + " is null");
                    } catch (Exception ee) {
                        System.Web.HttpContext.Current.Trace.Warn("Template", "Error adding macro " + tempMacro.Name, ee);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
			// Create child controls when told to - do it here anyway as it has to be done
            EnsureChildControls();

            bool isDebug = GlobalSettings.DebugMode && (helper.Request("umbdebugshowtrace") != "" || helper.Request("umbdebug") != "");
            if (isDebug)
            {
                writer.Write("<div title=\"Macro Tag: '{0}'\" style=\"border: 1px solid #009;\">", Alias);
            }
            base.RenderChildren(writer);
            if (isDebug)
            {
                writer.Write("</div>");
            }
        }


        public string Text
        {
            get { return m_Code; }
            set { m_Code = value; }
        }
    }
}
