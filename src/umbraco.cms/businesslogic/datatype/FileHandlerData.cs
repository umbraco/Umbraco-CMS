using System;
using System.IO;
using System.Web;
using umbraco.cms.businesslogic.Files;
using umbraco.IO;

namespace umbraco.cms.businesslogic.datatype
{
    public class FileHandlerData : DefaultData
    {
        private readonly string _thumbnailSizes;

        public FileHandlerData(BaseDataType DataType, string thumbnailSizes)
            : base(DataType)
        {
            _thumbnailSizes = thumbnailSizes;
        }

        public override object Value
        {
            get { return base.Value; }
            set
            {
                UmbracoFile um = null;
                if (value is HttpPostedFile)
                {
                    // handle upload
                    var file = value as HttpPostedFile;
                    if (file.FileName != String.Empty)
                    {
                        var fileName = UmbracoSettings.UploadAllowDirectories ?
                            Path.Combine(PropertyId.ToString(), file.FileName) :
                            PropertyId + "-" + file.FileName;

                        fileName = Path.Combine(SystemDirectories.Media, fileName);
                        um = UmbracoFile.Save(file, fileName);

                        if (um.SupportsResizing)
                        {
                            // make default thumbnail
                            um.Resize(100, "thumb");

                            // additional thumbnails configured as prevalues on the DataType
                            if (_thumbnailSizes != "")
                            {
                                char sep = (!_thumbnailSizes.Contains("") && _thumbnailSizes.Contains(",")) ? ',' : ';';

                                foreach (string thumb in _thumbnailSizes.Split(sep))
                                {
                                    int thumbSize;
                                    if (thumb != "" && int.TryParse(thumb, out thumbSize))
                                    {
                                        um.Resize(thumbSize, string.Format("thumb_{0}", thumbSize));
                                    }
                                }
                            }
                        }

                        base.Value = um.LocalName;
                    }
                    else
                    {
                        // if no file is uploaded, we reset the value
                        base.Value = String.Empty;
                    }
                }
                else
                {
                    base.Value = value;
                }
            }
        }
    }
}