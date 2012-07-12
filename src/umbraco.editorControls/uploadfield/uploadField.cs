using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using umbraco.interfaces;
using umbraco.IO;
using Content = umbraco.cms.businesslogic.Content;

namespace umbraco.editorControls
{
    [ValidationProperty("IsValid")]
    public class uploadField : HtmlInputFile, IDataEditor
    {
        private const String _thumbnailext = ".jpg";
        private readonly cms.businesslogic.datatype.DefaultData _data;
        private readonly String _thumbnails;
        private String _text;

        public uploadField(IData Data, string ThumbnailSizes)
        {
            _data = (cms.businesslogic.datatype.DefaultData) Data;
            _thumbnails = ThumbnailSizes;
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
                bool isEmpty = PostedFile == null || String.IsNullOrEmpty(PostedFile.FileName);
                // checkbox, if it's used the file will be deleted and we should throw a validation error
                if (Page.Request[ClientID + "clear"] != null && Page.Request[ClientID + "clear"] != "")
                    return "";
                else if (!isEmpty)
                    return PostedFile.FileName;
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

        #region IDataEditor Members

        public Control Editor
        {
            get { return this; }
        }

        public virtual bool TreatAsRichTextEditor
        {
            get { return false; }
        }

        public bool ShowLabel
        {
            get { return true; }
        }

        public void Save()
        {
            // Clear data
            if (helper.Request(ClientID + "clear") == "1")
            {
                // delete file
                deleteFile(_text);

                // set filename in db to nothing
                _text = "";
                _data.Value = _text;


                foreach (string prop in "umbracoExtension,umbracoBytes,umbracoWidth,umbracoHeight".Split(','))
                {
                    try
                    {
                        var bytesControl = FindControlRecursive<noEdit>(Page, "prop_" + prop);
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

            if (PostedFile != null && PostedFile.FileName != String.Empty)
            {
                _data.Value = PostedFile;

                // we update additional properties post image upload
                if (_data.Value != DBNull.Value && !string.IsNullOrEmpty(_data.Value.ToString()))
                {
                    string fullFilePath = IOHelper.MapPath(_data.Value.ToString());

                    Content content = Content.GetContentFromVersion(_data.Version);

                    // update extension in UI
                    try
                    {
                        var extensionControl = FindControlRecursive<noEdit>(Page, "prop_umbracoExtension");
                        if (extensionControl != null)
                        {
                            extensionControl.RefreshLabel(content.getProperty("umbracoExtension").Value.ToString());
                        }
                    }
                    catch
                    {
                    }


                    // update file size in UI
                    try
                    {
                        var bytesControl = FindControlRecursive<noEdit>(Page, "prop_umbracoBytes");
                        if (bytesControl != null)
                        {
                            bytesControl.RefreshLabel(content.getProperty("umbracoBytes").Value.ToString());
                        }
                    }
                    catch
                    {
                    }

                    try
                    {
                        var widthControl = FindControlRecursive<noEdit>(Page, "prop_umbracoWidth");
                        if (widthControl != null)
                        {
                            widthControl.RefreshLabel(content.getProperty("umbracoWidth").Value.ToString());
                        }
                        var heightControl = FindControlRecursive<noEdit>(Page, "prop_umbracoHeight");
                        if (heightControl != null)
                        {
                            heightControl.RefreshLabel(content.getProperty("umbracoHeight").Value.ToString());
                        }
                    }
                    catch
                    {
                    }
                }
                Text = _data.Value.ToString();
            }
        }

        #endregion

        public string SafeUrl(string url)
        {
            if (!String.IsNullOrEmpty(url))
                return Regex.Replace(url, @"[^a-zA-Z0-9\-\.\/\:]{1}", "_");
            else
                return String.Empty;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (_data != null && _data.Value != null)
                Text = _data.Value.ToString();
        }

        private void deleteFile(string file)
        {
            if (file.Length > 0)
            {
                // delete old file
                if (File.Exists(IOHelper.MapPath(file)))
                    File.Delete(IOHelper.MapPath(file));

                string extension = (file.Substring(file.LastIndexOf(".") + 1, file.Length - file.LastIndexOf(".") - 1));
                extension = extension.ToLower();

                //check for thumbnails
                if (",jpeg,jpg,gif,bmp,png,tiff,tif,".IndexOf("," + extension + ",") > -1)
                {
                    //delete thumbnails
                    string thumbnailfile = file.Replace("." + extension, "_thumb");

                    try
                    {
                        if (File.Exists(IOHelper.MapPath(thumbnailfile + _thumbnailext)))
                            File.Delete(IOHelper.MapPath(thumbnailfile + _thumbnailext));
                    }
                    catch
                    {
                    }

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
                                    if (File.Exists(IOHelper.MapPath(thumbnailextra)))
                                        File.Delete(IOHelper.MapPath(thumbnailextra));
                                }
                                catch
                                {
                                }
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
                return (T) parent;
            }

            foreach (Control control in parent.Controls)
            {
                var foundControl = FindControlRecursive<T>(control, id);
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
            if (!string.IsNullOrEmpty(Text))
            {
                string ext = _text.Substring(_text.LastIndexOf(".") + 1, _text.Length - _text.LastIndexOf(".") - 1);
                string fileNameThumb = _text.Replace("." + ext, "_thumb.jpg");
                bool hasThumb = false;
                try
                {
                    hasThumb = File.Exists(IOHelper.MapPath(IOHelper.FindFile(fileNameThumb)));
                    // 4.8.0 added support for png thumbnails (but for legacy it might have been jpg - hence the check before)
                    if (!hasThumb && (ext == "gif" || ext == "png"))
                    {
                        fileNameThumb = _text.Replace("." + ext, "_thumb.png");
                        hasThumb = File.Exists(IOHelper.MapPath(IOHelper.FindFile(fileNameThumb)));
                    }
                }
                catch
                {
                }
                if (hasThumb)
                {
                    var thumb = new Image();
                    thumb.ImageUrl = fileNameThumb;
                    thumb.BorderStyle = BorderStyle.None;

                    output.WriteLine("<a href=\"" + IOHelper.FindFile(_text) + "\" target=\"_blank\">");
                    thumb.RenderControl(output);
                    output.WriteLine("</a><br/>");
                }
                else
                    output.WriteLine("<a href=\"" + IOHelper.FindFile(Text) + "\" target=\"_blank\">" +
                                     IOHelper.FindFile(Text) + "</a><br/>");

                output.WriteLine("<input type=\"checkbox\" id=\"" + ClientID + "clear\" name=\"" + ClientID +
                                 "clear\" value=\"1\"/> <label for=\"" + ClientID + "clear\">" + ui.Text("uploadClear") +
                                 "</label><br/>");
            }
            base.Render(output);
        }
    }
}