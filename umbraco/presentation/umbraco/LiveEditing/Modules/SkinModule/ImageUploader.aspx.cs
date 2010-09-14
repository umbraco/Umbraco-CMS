using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;

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
                DirectoryInfo updir = new DirectoryInfo(IO.IOHelper.MapPath("~/media/upload/" + g));
               
                if (!updir.Exists)
                    updir.Create();

                FileName.Value = FileUpload1.FileName;

                FileUpload1.SaveAs(updir.FullName + "/" + FileUpload1.FileName);

                if (IsValidImage(updir.FullName + "/" + FileUpload1.FileName))
                {
                    Image1.ImageUrl = this.ResolveUrl("~/media/upload/" + g) + "/" + FileUpload1.FileName;
                    Image.Value = Image1.ImageUrl;

                    if (!string.IsNullOrEmpty(Request["w"]) && !string.IsNullOrEmpty(Request["h"]))
                    {
                        pnl_crop.Visible = true;
                        pnl_upload.Visible = false;
                    }
                    else
                    {
                        Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "setimage", " $(function () { setImage(); })", true);
                    }
                }
                else
                    throw new Exception("Not a valid image");
            }
        }


        protected void bt_crop_Click(object sender, EventArgs e)
        {
            System.Drawing.Image imgToResize = System.Drawing.Image.FromFile(IO.IOHelper.MapPath(Image.Value));

            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = Convert.ToSingle(Scale.Value) / (float)100;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(Convert.ToInt32(Request["w"]), Convert.ToInt32(Request["h"]));

            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;


            g.DrawImage(imgToResize, Convert.ToSingle(X.Value), Convert.ToSingle(Y.Value), destWidth, destHeight);

            g.Dispose();



            Guid id = Guid.NewGuid();
            DirectoryInfo updir = new DirectoryInfo(IO.IOHelper.MapPath("~/media/upload/" + id));

            if (!updir.Exists)
                updir.Create();


            ((System.Drawing.Image)b).Save(updir.FullName + "/" + FileName.Value);

            Image.Value = this.ResolveUrl("~/media/upload/" + id) + "/" + FileName.Value;
            Image1.ImageUrl = Image.Value;

            pnl_crop.Visible = true;

            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "setimage", " $(function () { setImage(); })", true);

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