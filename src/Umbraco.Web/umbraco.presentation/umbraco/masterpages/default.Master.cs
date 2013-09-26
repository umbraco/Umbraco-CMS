using System;
using System.Web.Mvc;
using System.Web.UI;
using Umbraco.Web;
using System.IO;

namespace umbraco.presentation.masterpages
{
    public partial class _default : MasterPage
    {
        
        protected override void Render(HtmlTextWriter writer)
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
