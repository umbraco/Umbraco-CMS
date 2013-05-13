using System;
using System.Diagnostics;
using System.Web.UI;
using StackExchange.Profiling;
using Umbraco.Core.Profiling;
using umbraco.presentation.LiveEditing;
using umbraco.presentation.LiveEditing.Controls;
using System.IO;

namespace umbraco.presentation.masterpages
{
    public partial class _default : System.Web.UI.MasterPage
    {
        protected ILiveEditingContext m_LiveEditingContext = UmbracoContext.Current.LiveEditingContext;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                AddLiveEditingSupport();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error adding Canvas support.", ex);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (m_LiveEditingContext.Enabled == false)
            {
                // profiling
                if (string.IsNullOrEmpty(Request.QueryString["umbDebug"]) == false && GlobalSettings.DebugMode)
                {
                    var baseWriter = new StringWriter();
                    base.Render(new HtmlTextWriter(baseWriter));
                    var baseOutput = baseWriter.ToString();

                    baseOutput = baseOutput.Replace("</body>", MiniProfiler.RenderIncludes() + "</body>");
                    writer.Write(baseOutput);
                }

                else
                {

                    base.Render(writer);
                }
            }
            else
            {
                // get base output
                StringWriter baseWriter = new StringWriter();
                base.Render(new HtmlTextWriter(baseWriter));
                string baseOutput = baseWriter.ToString();

                // add custom umbraco namespace (required for events on custom tags in IE)
                baseOutput = baseOutput.Replace("<html", "<html xmlns:umbraco=\"http://umbraco.org\"");

                // write modified output
                writer.Write(baseOutput);
            }
        }

        protected virtual void AddLiveEditingSupport()
        {
            // add Live Editing support if it is enabled
            if (m_LiveEditingContext.Enabled)
            {
                // require an ASP.Net form
                if (Page.Form == null)
                {
                    //turn live editing off so it won't annoying the hell out of people who doesn't have a form and try to refresh the page... 
                    UmbracoContext.Current.LiveEditingContext.Enabled = false;
                    throw new ApplicationException("Umbraco Canvas requires an ASP.Net form to function properly. Live editing has been turned off.");
                }
                else
                {
                    // add a ScriptManager to the form if not present
                    if (ScriptManager.GetCurrent(Page) == null)
                    {
                        Page.Form.Controls.Add(new ScriptManager());
                    }

                    // add the Live Editing manager
                    Page.Form.Controls.Add(new LiveEditingManager(m_LiveEditingContext));
                }
            }
        }

        /// <summary>
        /// ContentPlaceHolderDefault control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.ContentPlaceHolder ContentPlaceHolderDefault;
    }
}
