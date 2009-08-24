using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.media;

namespace umbraco.presentation.webservices
{
    public class MediaPickerServiceHelpers
    {

        public static string GetThumbNail(int mediaId)
        {


            string fileName;
            string thumbnail = string.Empty;
            try
            {

                Media m = new Media(mediaId);

                fileName = m.getProperty("umbracoFile").Value.ToString();
                string ext = fileName.Substring(fileName.LastIndexOf('.') + 1, fileName.Length - (fileName.LastIndexOf('.') + 1));


                if (",jpeg,jpg,gif,bmp,png,tiff,tif,".IndexOf("," + ext.ToLower() + ",") > -1)
                {
                    thumbnail = fileName.Substring(0, fileName.LastIndexOf('.')) + "_thumb.jpg";
                }
                else
                {
                    return "";
                    //switch (ext.ToLower())
                    //{
                    //    case "pdf":
                    //        thumbnail = "";
                    //        break;
                    //    case "doc":
                    //        thumbnail = "";
                    //        break;
                    //    default:
                    //        thumbnail = "";
                    //        break;
                    //}
                }

            }
            catch { }

            return thumbnail;
        }

        public static string GetFile(int mediaId)
        {
            string fileName = string.Empty;

            try
            {


                Media m = new Media(mediaId);

                fileName = m.getProperty("umbracoFile").Value.ToString();


            }
            catch { }

            return fileName;

        }
    }
}
