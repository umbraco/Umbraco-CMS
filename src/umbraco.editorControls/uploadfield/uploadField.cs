using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.interfaces;
using Umbraco.Core;
using Content = umbraco.cms.businesslogic.Content;
using Umbraco.Core;

namespace umbraco.editorControls
{
    [ValidationProperty("IsValid")]
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class uploadField : HtmlInputFile, IDataEditor
    {
        private const string Thumbnailext = ".jpg";
        private readonly cms.businesslogic.datatype.FileHandlerData _data;
        private readonly string _thumbnails;
        private string _text;
        private readonly MediaFileSystem _fs;
        private CustomValidator _customValidator;

        public uploadField(IData Data, string ThumbnailSizes)
        {
            _fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            _data = (cms.businesslogic.datatype.FileHandlerData)Data; //this is always FileHandlerData
            _thumbnails = ThumbnailSizes;
        }


        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _customValidator = new CustomValidator
                {
                    EnableClientScript = false,
                    Display = ValidatorDisplay.Dynamic,
                    ErrorMessage = ui.Text("errors", "dissallowedMediaType")
                };
            _customValidator.ErrorMessage += "<br/>";

            //NOTE: it would be better to have this as a normal composite control but we don't want to 
            // break compatibility so we cannot make this inherit from a different class than it already
            // is, so now we have to hack this together and add this directly to the page validators collection
            // since we cannot add it as a control to this one.
            Page.Validators.Add(_customValidator);
        }

        /// <summary>
        /// Internal logic for validation controls to detect whether or not it's valid (has to be public though) 
        /// </summary>
        /// <value>Am I valid?</value>   
        /// <remarks>
        /// This is used for the required and regex validation of a document type's property
        /// </remarks>     
        public string IsValid
        {
            get
            {
                var tempText = Text;
                var isEmpty = PostedFile == null || string.IsNullOrEmpty(PostedFile.FileName);
                // checkbox, if it's used the file will be deleted and we should throw a validation error
                if (Page.Request[ClientID + "clear"] != null && Page.Request[ClientID + "clear"] != "")
                    return "";
                if (isEmpty == false)
                    return PostedFile.FileName;
                return string.IsNullOrEmpty(tempText) == false
                    ? tempText
                    : "";
            }
        }

        /// <summary>
        /// Checks if the file is valid based on our dissallowed file types
        /// </summary>
        /// <param name="postedFile"></param>
        /// <returns></returns>
        internal bool IsValidFile(HttpPostedFile postedFile)
        {
            //return true if there is no file
            if (postedFile == null) return true;
            if (postedFile.FileName.IsNullOrWhiteSpace()) return true;

            //now check the file type
            var extension = Path.GetExtension(postedFile.FileName).TrimStart(".");

            return UmbracoConfig.For.UmbracoSettings().Content.DisallowedUploadFiles.Any(x => x.InvariantEquals(extension)) == false;
        }

        public string Text
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
                DeleteFile(_text);

                // set filename in db to nothing
                _text = "";
                _data.Value = _text;

                var props = new[] { Constants.Conventions.Media.Bytes, Constants.Conventions.Media.Extension, Constants.Conventions.Media.Height, Constants.Conventions.Media.Width };
                foreach (var prop in props)
                {
                    try
                    {
                        var bytesControl = Page.FindControlRecursive<noEdit>("prop_" + prop);
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

            if (PostedFile == null || PostedFile.FileName == string.Empty) return;

            //don't save if the file is invalid
            if (IsValidFile(PostedFile) == false)
            {
                //set the validator to not valid
                _customValidator.IsValid = false;
                return;
            }

            _data.Value = PostedFile;

            // we update additional properties post image upload
            if (_data.Value != DBNull.Value && string.IsNullOrEmpty(_data.Value.ToString()) == false)
            {
                //check the FileHandlerData to see if it already loaded in the content item and set it's properties.
                //if not, then the properties haven't changed so skip.
                if (_data.LoadedContentItem != null)
                {
                    var content = _data.LoadedContentItem;

                    // update extension in UI
                    UpdateLabelValue(Constants.Conventions.Media.Extension, "prop_umbracoExtension", Page, content);
                    // update file size in UI
                    UpdateLabelValue(Constants.Conventions.Media.Bytes, "prop_umbracoBytes", Page, content);
                    UpdateLabelValue(Constants.Conventions.Media.Width, "prop_umbracoWidth", Page, content);
                    UpdateLabelValue(Constants.Conventions.Media.Height, "prop_umbracoHeight", Page, content);
                }

            }
            Text = _data.Value.ToString();
        }


        #endregion

        private static void UpdateLabelValue(string propAlias, string controlId, Page controlPage, Content content)
        {
            var extensionControl = controlPage.FindControlRecursive<noEdit>(controlId);
            if (extensionControl != null)
            {
                if (content.getProperty(propAlias) != null && content.getProperty(propAlias).Value != null)
                {
                    extensionControl.RefreshLabel(content.getProperty(propAlias).Value.ToString());
                }
            }
        }

        [Obsolete("This method is now obsolete due to a change in the way that files are handled.  If you need to check if a URL for an uploaded file is safe you should implement your own as this method will be removed in a future version", false)]
        public string SafeUrl(string url)
        {
            return string.IsNullOrEmpty(url) == false
                ? Regex.Replace(url, @"[^a-zA-Z0-9\-\.\/\:]{1}", "_")
                : String.Empty;
        }

        protected override void OnInit(EventArgs e)
        {
            EnsureChildControls();
            base.OnInit(e);
            if (_data != null && _data.Value != null)
                Text = _data.Value.ToString();
        }

        private void DeleteFile(string fileUrl)
        {
            if (fileUrl.Length > 0)
            {
                var relativeFilePath = _fs.GetRelativePath(fileUrl);

                // delete old file
                if (_fs.FileExists(relativeFilePath))
                    _fs.DeleteFile(relativeFilePath);

                var extension = (relativeFilePath.Substring(relativeFilePath.LastIndexOf(".") + 1, relativeFilePath.Length - relativeFilePath.LastIndexOf(".") - 1));
                extension = extension.ToLower();

                //check for thumbnails
                if (",jpeg,jpg,gif,bmp,png,tiff,tif,".IndexOf("," + extension + ",") > -1)
                {
                    //delete thumbnails
                    string relativeThumbFilePath = relativeFilePath.Replace("." + extension, "_thumb");

                    try
                    {
                        if (_fs.FileExists(relativeThumbFilePath + Thumbnailext))
                            _fs.DeleteFile(relativeThumbFilePath + Thumbnailext);
                    }
                    catch
                    {
                    }

                    if (_thumbnails != "")
                    {
                        var thumbnailSizes = _thumbnails.Split(";".ToCharArray());
                        foreach (var thumb in thumbnailSizes)
                        {
                            if (thumb != "")
                            {
                                string relativeExtraThumbFilePath = relativeThumbFilePath + "_" + thumb + Thumbnailext;

                                try
                                {
                                    if (_fs.FileExists(relativeExtraThumbFilePath))
                                        _fs.DeleteFile(relativeExtraThumbFilePath);
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
        /// Render this control to the output parameter specified.
        /// </summary>
        /// <param name="output"> The HTML writer to write out to </param>
        protected override void Render(HtmlTextWriter output)
        {
            //render the validator if it is not valid
            //NOTE: it would be better to have this as a normal composite control but we don't want to 
            // break compatibility so we cannot make this inherit from a different class than it already
            // is, so now we have to hack this together.     
            if (_customValidator.IsValid == false)
            {
                _customValidator.RenderControl(output);
            }

            if (string.IsNullOrEmpty(Text) == false)
            {
                var relativeFilePath = _fs.GetRelativePath(_text);
                var ext = relativeFilePath.Substring(relativeFilePath.LastIndexOf(".") + 1, relativeFilePath.Length - relativeFilePath.LastIndexOf(".") - 1);
                var relativeThumbFilePath = relativeFilePath.Replace("." + ext, "_thumb." + ext);
                var hasThumb = false;
                try
                {
                    hasThumb = _fs.FileExists(relativeThumbFilePath);
                    
                    // 7.4.0 generates thumbs with the correct file extension, but check for old possible extensions as well
                    if (hasThumb == false)
                    {
                        relativeThumbFilePath = relativeFilePath.Replace("." + ext, "_thumb.png");
                        hasThumb = _fs.FileExists(relativeThumbFilePath);
                    }

                    if (hasThumb == false)
                    {
                        relativeThumbFilePath = relativeFilePath.Replace("." + ext, "_thumb.jpg");
                        hasThumb = _fs.FileExists(relativeThumbFilePath);
                    }
                }
                catch
                {
                }
                if (hasThumb)
                {
                    var thumb = new Image
                    {
                        ImageUrl = _fs.GetUrl(relativeThumbFilePath),
                        BorderStyle = BorderStyle.None
                    };

                    output.WriteLine("<a href=\"" + _fs.GetUrl(relativeFilePath) + "\" target=\"_blank\">");
                    thumb.RenderControl(output);
                    output.WriteLine("</a><br/>");
                }
                else
                    output.WriteLine("<a href=\"" + _fs.GetUrl(relativeFilePath) + "\" target=\"_blank\">" +
                                     _fs.GetUrl(relativeFilePath) + "</a><br/>");

                output.WriteLine("<input type=\"checkbox\" id=\"" + ClientID + "clear\" name=\"" + ClientID +
                                 "clear\" value=\"1\"/> <label for=\"" + ClientID + "clear\">" + ui.Text("uploadClear") +
                                 "</label><br/>");
            }
            base.Render(output);
        }
    }
}