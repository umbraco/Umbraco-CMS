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

                    cms.businesslogic.Content content = cms.businesslogic.Content.GetContentFromVersion(this._data.Version);

                    // update extension in UI
                    try
                    {
                        noEdit extensionControl = uploadField.FindControlRecursive<noEdit>(this.Page, "prop_umbracoExtension");
                        if (extensionControl != null)
                        {
                            extensionControl.RefreshLabel(content.getProperty("umbracoExtension").Value.ToString());
                        }
                    }
                    catch { }



                    // update file size in UI
                    try
                    {
                        noEdit bytesControl = uploadField.FindControlRecursive<noEdit>(this.Page, "prop_umbracoBytes");
                        if (bytesControl != null)
                        {
                            bytesControl.RefreshLabel(content.getProperty("umbracoBytes").Value.ToString());
                        }
                    }
                    catch { }

                    try
                    {
                        noEdit widthControl = uploadField.FindControlRecursive<noEdit>(this.Page, "prop_umbracoWidth");
                        if (widthControl != null)
                        {
                            widthControl.RefreshLabel(content.getProperty("umbracoWidth").Value.ToString());
                        }
                        noEdit heightControl = uploadField.FindControlRecursive<noEdit>(this.Page, "prop_umbracoHeight");
                        if (heightControl != null)
                        {
                            heightControl.RefreshLabel(content.getProperty("umbracoHeight").Value.ToString());
                        }
                    }
                    catch { }

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
            if (!string.IsNullOrEmpty(this.Text))
            {
                string ext = _text.Substring(_text.LastIndexOf(".") + 1, _text.Length - _text.LastIndexOf(".") - 1);
                string fileNameThumb = _text.Replace("." + ext, "_thumb.jpg");
                bool hasThumb = false;
                try
                {
                    hasThumb = File.Exists(IOHelper.MapPath(IOHelper.FindFile(fileNameThumb)));
                    // 4.8.0 added support for png thumbnails
                    if (!hasThumb && (ext == "gif" || ext == "png"))
                    {
                        fileNameThumb = _text.Replace("." + ext, "_thumb.png");
                        hasThumb = File.Exists(IOHelper.MapPath(IOHelper.FindFile(fileNameThumb)));
                    }
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
