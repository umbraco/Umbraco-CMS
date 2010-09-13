using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule
{
    public partial class ImageUploader : BasePages.UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void bt_upload_Click(object sender, EventArgs e)
        {
            if (FileUpload1.HasFile)
            {
                Guid g = Guid.NewGuid();
                DirectoryInfo updir = new DirectoryInfo(Server.MapPath("~/media/upload/" + g));
               
                if (!updir.Exists)
                    updir.Create();

                FileUpload1.SaveAs(updir.FullName + "/" + FileUpload1.FileName);

                if (IsValidImage(updir.FullName + "/" + FileUpload1.FileName))
                {
                    Image1.ImageUrl = this.ResolveUrl("~/media/upload/" + g) + "/" + FileUpload1.FileName;
                    Image.Value = Image1.ImageUrl;
                }
                else
                    throw new Exception("Not a valid image");
            }
        }

        private bool IsValidImage(string filename)
        {
            try
            {
                System.Drawing.Image newImage = System.Drawing.Image.FromFile(filename);
            }
            catch (OutOfMemoryException ex)
            {
                // Image.FromFile will throw this if file is invalid.

                return false;
            }
            return true;
        }
    }
}