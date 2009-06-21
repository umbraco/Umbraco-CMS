using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.Linq.DTMetal.CodeBuilder;
using umbraco.Linq.DTMetal.Engine;
using System.IO;
using System.Text;

namespace umbraco.presentation.umbraco.dialogs
{
    public partial class ExportCode : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            btnGenerate.Text = ui.Text("create");
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            GenerationLanguage language = (GenerationLanguage)Enum.Parse(typeof(GenerationLanguage), ddlLanguage.SelectedValue);
            string dataContextName = string.IsNullOrEmpty(txtDataContextName.Text) ? "Umbraco" : txtDataContextName.Text;
            var generator = new DTMLGenerator(GlobalSettings.DbDSN, dataContextName, false);
            var dtml = generator.GenerateDTMLStream();
            var cb = ClassGenerator.CreateBuilder(string.IsNullOrEmpty(txtNamespace.Text) ? "Umbraco" : txtNamespace.Text, language, dtml.DocTypeMarkupLanguage);
            cb.GenerateCode();
            cb.Save(UmbracoContext.Current.Response.OutputStream);
            UmbracoContext.Current.Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", dataContextName));
            UmbracoContext.Current.Response.ContentType = "application/octet-stream";
            UmbracoContext.Current.Response.End();
        }
    }
}
