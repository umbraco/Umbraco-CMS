using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using umbraco.cms.businesslogic.Files;
using umbraco.IO;
using System.Text.RegularExpressions;

namespace umbraco.editorControls
{
    [ValidationProperty("IsValid")]
    public class uploadField : System.Web.UI.HtmlControls.HtmlInputFile, interfaces.IDataEditor
    {
        private String _text;
        private cms.businesslogic.datatype.DefaultData _data;
        private String _thumbnails;

        const String _thumbnailext = ".jpg";

        public uploadField(interfaces.IData Data, string ThumbnailSizes)
        {
            _data = (cms.businesslogic.datatype.DefaultData)Data;
            _thumbnails = ThumbnailSizes;
        }

        public Control Editor { get { return this; } }

        public virtual bool TreatAsRichTextEditor
        {
            get { return false; }
        }
        public bool ShowLabel
        {
            get { return true; }
        }

        public string SafeUrl(string url)
        {
            if (!String.IsNullOrEmpty(url))
                return Regex.Replace(url, @"[^a-zA-Z0-9\-\.\/\:]{1}", "_");
            else
                return String.Empty;
        }

        /// <summary>
        /// Internal logic for validation controls to detect whether or not it's valid (has to be public though) 
        /// </summary>
        /// <value>Am I valid?</value>
        public string IsValid
        {
            get
            {
                string tempText = Text;
                bool isEmpty = this.PostedFile == null || String.IsNullOrEmpty(this.PostedFile.FileName);
                // checkbox, if it's used the file will be deleted and we should throw a validation error
                if (Page.Request[this.ClientID + "clear"] != null && Page.Request[this.ClientID + "clear"].ToString() != "")
                    return "";
                else if (!isEmpty)
                    return this.PostedFile.FileName;
                else if (!String.IsNullOrEmpty(tempText))
                    return tempText;
                else
                    return "";
            }
        }

        public String Text
        {
            get { return _text; }
            set { _text = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (_data != null && _data.Value != null)
                this.Text = _data.Value.ToString();
        }

        public void Save()
        {
            // Clear data
            if (helper.Request(this.ClientID + "clear") == "1")
            {
                // delete file
                deleteFile(_text);

                // set filename in db to nothing
                _text = "";
                _data.Value = _text;

                //also clear umbracoWidth, umbracoHeight, umbracoExtension, umbracoBytes

                cms.businesslogic.Content content = cms.businesslogic.Content.GetContentFromVersion(this._data.Version);

                foreach (string prop in "umbracoExtension,umbracoBytes,umbracoWidth,umbracoHeight".Split(','))
                {
                    try
                    {
                        content.getProperty(prop).Value = string.Empty;
                        noEdit bytesControl = uploadField.FindControlRecursive<noEdit>(this.Page, "prop_" + prop);
                        if (bytesControl != null)
                        {
                            bytesControl.RefreshLabel(string.Empty);
                        }
                    }
                    catch
                    {
                        //if first one fails we can assume that props don't exist
                        break;
                    }
                }
            }

            if (this.PostedFile != null)
            {
                _data.Value = this.PostedFile;

                // we update additional properties post image upload
                if (_data.Value != DBNull.Value && !string.IsNullOrEmpty(_data.Value.ToString()))
                {
                    var fullFilePath = IO.IOHelper.MapPath(_data.Value.ToString());
                    UmbracoFile uf = new UmbracoFile(fullFilePath);

                    cms.businesslogic.Content content = cms.businesslogic.Content.GetContentFromVersion(this._data.Version);

                    // Save extension
                    string orgExt = uf.Extension.ToLower();
                    try
                    {
                        //cms.businesslogic.Content.GetContentFromVersion(_data.Version).getProperty("umbracoExtension").Value = ext;
                        content.getProperty("umbracoExtension").Value = orgExt;
                        noEdit extensionControl = uploadField.FindControlRecursive<noEdit>(this.Page, "prop_umbracoExtension");
                        if (extensionControl != null)
                        {
                            extensionControl.RefreshLabel(content.getProperty("umbracoExtension").Value.ToString());
                        }
                    }
                    catch { }



                    // Save file size
                    try
                    {
                        //cms.businesslogic.Content.GetContentFromVersion(_data.Version).getProperty("umbracoBytes").Value = fi.Length.ToString();
                        content.getProperty("umbracoBytes").Value = uf.Length.ToString();
                        noEdit bytesControl = uploadField.FindControlRecursive<noEdit>(this.Page, "prop_umbracoBytes");
                        if (bytesControl != null)
                        {
                            bytesControl.RefreshLabel(content.getProperty("umbracoBytes").Value.ToString());
                        }
                    }
                    catch { }

                    // Check if image and then get sizes, make thumb and update database
                    if (uf.SupportsResizing)
                    {
                        System.Tuple<int, int> dimensions = uf.GetDimensions();
                        int fileWidth = dimensions.Item1;
                        int fileHeight = dimensions.Item2;

                        
                        try
                        {
                            //cms.businesslogic.Content.GetContentFromVersion(_data.Version).getProperty("umbracoWidth").Value = fileWidth.ToString();
                            //cms.businesslogic.Content.GetContentFromVersion(_data.Version).getProperty("umbracoHeight").Value = fileHeight.ToString();
                            content.getProperty("umbracoWidth").Value = fileWidth.ToString();
                            noEdit widthControl = uploadField.FindControlRecursive<noEdit>(this.Page, "prop_umbracoWidth");
                            if (widthControl != null)
                            {
                                widthControl.RefreshLabel(content.getProperty("umbracoWidth").Value.ToString());
                            }
                            content.getProperty("umbracoHeight").Value = fileHeight.ToString();
                            noEdit heightControl = uploadField.FindControlRecursive<noEdit>(this.Page, "prop_umbracoHeight");
                            if (heightControl != null)
                            {
                                heightControl.RefreshLabel(content.getProperty("umbracoHeight").Value.ToString());
                            }
                        }
                        catch { }


                    }
                }
                this.Text = _data.Value.ToString();
            }
        }

        private void deleteFile(string file)
        {
            if (file.Length > 0)
            {
                // delete old file
                if (System.IO.File.Exists(IOHelper.MapPath(file)))
                    System.IO.File.Delete(IOHelper.MapPath(file));

                string extension = ((string)file.Substring(file.LastIndexOf(".") + 1, file.Length - file.LastIndexOf(".") - 1));
                extension = extension.ToLower();

                //check for thumbnails
                if (",jpeg,jpg,gif,bmp,png,tiff,tif,".IndexOf("," + extension + ",") > -1)
                {


                    //delete thumbnails
                    string thumbnailfile = file.Replace("." + extension, "_thumb");

                    try
                    {
                        if (System.IO.File.Exists(IOHelper.MapPath(thumbnailfile + _thumbnailext)))
                            System.IO.File.Delete(IOHelper.MapPath(thumbnailfile + _thumbnailext));
                    }
                    catch { }

                    if (_thumbnails != "")
                    {
                        string[] thumbnailSizes = _thumbnails.Split(";".ToCharArray());
                        foreach (string thumb in thumbnailSizes)
                        {
                            if (thumb != "")
                            {
                                string thumbnailextra = thumbnailfile + "_" + thumb + _thumbnailext;

                                try
                                {
                                    if (System.IO.File.Exists(IOHelper.MapPath(thumbnailextra)))
                                        System.IO.File.Delete(IOHelper.MapPath(thumbnailextra));
                                }
                                catch { }
                            }
                        }

                    }
                }
            }
        }

        private void generateThumbnail(System.Drawing.Image image, int maxWidthHeight, int fileWidth, int fileHeight, string fullFilePath, string ext, string thumbnailFileName)
        {
            // Generate thumbnail
            float fx = (float)fileWidth / (float)maxWidthHeight;
            float fy = (float)fileHeight / (float)maxWidthHeight;
            // must fit in thumbnail size
            float f = Math.Max(fx, fy); //if (f < 1) f = 1;
            int widthTh = (int)Math.Round((float)fileWidth / f); int heightTh = (int)Math.Round((float)fileHeight / f);

            // fixes for empty width or height
            if (widthTh == 0)
                widthTh = 1;
            if (heightTh == 0)
                heightTh = 1;

            // Create new image with best quality settings
            Bitmap bp = new Bitmap(widthTh, heightTh);
            Graphics g = Graphics.FromImage(bp);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Copy the old image to the new and resized
            Rectangle rect = new Rectangle(0, 0, widthTh, heightTh);
            g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

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

            // Save the new image
            bp.Save(thumbnailFileName, codec, ep);
            bp.Dispose();
            g.Dispose();

        }

        /// <summary>
        /// Recursively finds a control with the specified identifier.
        /// </summary>
        /// <typeparam name="T">
        /// The type of control to be found.
        /// </typeparam>
        /// <param name="parent">
        /// The parent control from which the search will start.
        /// </param>
        /// <param name="id">
        /// The identifier of the control to be found.
        /// </param>
        /// <returns>
        /// The control with the specified identifier, otherwise <see langword="null"/> if the control 
        /// is not found.
        /// </returns>
        private static T FindControlRecursive<T>(Control parent, string id) where T : Control
        {
            if ((parent is T) && (parent.ID == id))
            {
                return (T)parent;
            }

            foreach (Control control in parent.Controls)
            {
                T foundControl = uploadField.FindControlRecursive<T>(control, id);
                if (foundControl != null)
                {
                    return foundControl;
                }
            }
            return default(T);
        }

        /// <summary> 
        /// Render this control to the output parameter specified.
        /// </summary>
        /// <param name="output"> The HTML writer to write out to </param>
        protected override void Render(HtmlTextWriter output)
        {
            if (this.Text != null && this.Text != "")
            {
                string ext = _text.Substring(_text.LastIndexOf(".") + 1, _text.Length - _text.LastIndexOf(".") - 1);
                string fileNameThumb = _text.Replace("." + ext, "_thumb.jpg");
                bool hasThumb = false;
                try
                {
                    hasThumb = File.Exists(IOHelper.MapPath(IOHelper.FindFile(fileNameThumb)));
                }
                catch { }
                if (hasThumb)
                {
                    System.Web.UI.WebControls.Image thumb = new System.Web.UI.WebControls.Image();
                    thumb.ImageUrl = fileNameThumb;
                    thumb.BorderStyle = BorderStyle.None;

                    output.WriteLine("<a href=\"" + IOHelper.FindFile(_text) + "\" target=\"_blank\">");
                    thumb.RenderControl(output);
                    output.WriteLine("</a><br/>");
                }
                else
                    output.WriteLine("<a href=\"" + IOHelper.FindFile(this.Text) + "\" target=\"_blank\">" + IOHelper.FindFile(this.Text) + "</a><br/>");

                output.WriteLine("<input type=\"checkbox\" id=\"" + this.ClientID + "clear\" name=\"" + this.ClientID + "clear\" value=\"1\"/> <label for=\"" + this.ClientID + "clear\">" + ui.Text("uploadClear") + "</label><br/>");
            }
            base.Render(output);
        }
    }
}
