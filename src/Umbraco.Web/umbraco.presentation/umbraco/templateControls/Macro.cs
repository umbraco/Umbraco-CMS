using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Macros;

namespace umbraco.presentation.templateControls
{

    [DefaultProperty("Alias")]
    [ToolboxData("<{0}:Macro runat=server></{0}:Macro>")]
    [PersistChildren(false)]
    [ParseChildren(true, "Text")]
    public class Macro : WebControl, ITextControl
    {
        public Hashtable MacroAttributes { get; set; } = new Hashtable();

        [Bindable(true)]
        [Category("Umbraco")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Alias
        {
            get
            {
                var s = (string)ViewState["Alias"];
                return s ?? string.Empty;
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
                var s = (string)ViewState["Language"];
                return s ?? string.Empty;
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
                var s = (string)ViewState["FileLocation"];
                return s ?? string.Empty;
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
                var renderEvent = RenderEvents.Init;
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
            if (RenderEvent == RenderEvents.Init)
                EnsureChildControls();
        }

        // Create child controls when told to - new option to render at PreRender
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (RenderEvent == RenderEvents.PreRender)
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

            if (MacroAttributes.ContainsKey("macroalias") == false && MacroAttributes.ContainsKey("macroAlias") == false)
                MacroAttributes.Add("macroalias", Alias);

            // set pageId to int.MinValue if no pageID was found,
            // e.g. if the macro was rendered on a custom (non-Umbraco) page
            var pageId = UmbracoContext.Current.PageId == null ? int.MinValue : UmbracoContext.Current.PageId.Value;

            if ((string.IsNullOrEmpty(Language) == false && Text != "") || string.IsNullOrEmpty(FileLocation) == false) {
                var tempMacro = new MacroModel();
                MacroRenderer.GenerateMacroModelPropertiesFromAttributes(tempMacro, MacroAttributes);

                // executing an inline macro?
                // ie the code of the macro is in the control's text body
                // ok, this is not supported in v8 anymore
                if (string.IsNullOrEmpty(FileLocation))
                    throw new NotSupportedException("Inline macros are not supported anymore.");

                // executing an on-disk macro
                // it has to be a partial (cshtml or vbhtml) macro in v8
                var extension = System.IO.Path.GetExtension(FileLocation);
                if (extension.InvariantEndsWith(".cshtml") == false && extension.InvariantEndsWith(".vbhtml") == false)
                    throw new NotSupportedException("");

                tempMacro.MacroSource = FileLocation;
                tempMacro.MacroType = MacroTypes.PartialView;

                if (string.IsNullOrEmpty(Attributes["Cache"]) == false)
                {
                    int cacheDuration;
                    if (int.TryParse(Attributes["Cache"], out cacheDuration))
                        tempMacro.CacheDuration = cacheDuration;
                    else
                        Context.Trace.Warn("Template", "Cache attribute is in incorect format (should be an integer).");
                }

                var renderer = new MacroRenderer(Current.ProfilingLogger);
                var c = renderer.Render(tempMacro, (Hashtable) Context.Items["pageElements"], pageId).GetAsControl();
                if (c != null)
                {
                    Exceptions = renderer.Exceptions;
                    Controls.Add(c);
                }
                else
                {
                    Context.Trace.Warn("Template", "Result of inline macro scripting is null");
                }
            }
            else
            {
                var m = Current.Services.MacroService.GetByAlias(Alias);
                if (m == null) return;

                var tempMacro = new MacroModel(m);
                try
                {
                    var renderer = new MacroRenderer(Current.ProfilingLogger);
                    var c = renderer.Render(tempMacro, (Hashtable)Context.Items["pageElements"], pageId, MacroAttributes).GetAsControl();
                    if (c != null)
                        Controls.Add(c);
                    else
                        Context.Trace.Warn("Template", "Result of macro " + tempMacro.Name + " is null");
                }
                catch (Exception ee)
                {
                    Context.Trace.Warn("Template", "Error adding macro " + tempMacro.Name, ee);
                    throw;
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

            var isDebug = GlobalSettings.DebugMode && (Context.Request.GetItemAsString("umbdebugshowtrace") != "" || Context.Request.GetItemAsString("umbdebug") != "");
            if (isDebug)
            {
                writer.Write("<div title=\"Macro Tag: '{0}'\" style=\"border: 1px solid #009;\">", Alias);
            }
            RenderChildren(writer);
            if (isDebug)
            {
                writer.Write("</div>");
            }
        }

        public string Text { get; set; } = string.Empty;
    }
}
