using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule
{
    public partial class ImageUploader : BasePages.UmbracoEnsuredPage
    {
        //max width and height is used to make sure the cropper doesn't grow bigger then the modal window
        public int MaxWidth = 700;
        public int MaxHeight = 480;
        public string scaleWidth = "500px";


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

                    if ((!string.IsNullOrEmpty(Request["w"]) && Request["w"].ToString() != "0") && (!string.IsNullOrEmpty(Request["h"]) && Request["h"].ToString() != "0"))
                    {

                        if (Convert.ToInt32(Request["w"]) > MaxWidth || Convert.ToInt32(Request["h"]) > MaxHeight)
                        {
                            System.Drawing.Image img = System.Drawing.Image.FromFile(IO.IOHelper.MapPath(Image.Value));

                            Image1.Width = img.Width / 2;
                            Image1.Height = img.Height / 2;

                            fb_feedback1.Text = "<strong>Notice:</strong> The below exemple is scaled down 50% as the image is quite large, do not worry, it will be the right size on your website";
                        }

                        if (Image1.Width.Value > 250)
                            scaleWidth = (Image1.Width.Value).ToString() + "px";


                        pnl_crop.Visible = true;
                        pnl_upload.Visible = false;
                    }
                    else
                    {
                        Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "setimage", " $(function () { setImage(); })", true);
                    }
                }
                else
                {
                    fb_feedback1.Text = "Please choose a valid image file";
                    fb_feedback1.type = uicontrols.Feedback.feedbacktype.error;
                }
            }
        }

        protected void bt_uploadother_Click(object sender, EventArgs e)
        {
            pnl_crop.Visible = false;
            pnl_upload.Visible = true;
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

            float x = Convert.ToSingle(X.Value);
            float y = Convert.ToSingle(Y.Value);

            if (Convert.ToInt32(Request["w"]) > MaxWidth || Convert.ToInt32(Request["h"]) > MaxHeight)
            {
                x = x * 2;
                y = y * 2;
            }
            g.DrawImage(imgToResize, x, y, destWidth, destHeight);

            g.Dispose();



            Guid id = Guid.NewGuid();
            DirectoryInfo updir = new DirectoryInfo(IO.IOHelper.MapPath("~/media/upload/" + id));

            if (!updir.Exists)
                updir.Create();

            // Copy metadata
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo codec = null;
            for (int i = 0; i < codecs.Length; i++)
            {
                if (codecs[i].MimeType.Equals("image/jpeg"))
                    codec = codecs[i];
            }

            // Set compresion ratio to 90%
            EncoderParameters ep = new EncoderParameters();
            ep.Param[0] = new EncoderParameter(Encoder.Quality, 90L);


            ((System.Drawing.Image)b).Save(updir.FullName + "/" + FileName.Value, codec, ep);

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