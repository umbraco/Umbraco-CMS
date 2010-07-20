using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Web;
using umbraco.BusinessLogic;
using umbraco.IO;

namespace umbraco.editorControls.tinymce
{
    internal class tinyMCEImageHelper
    {
        public static string cleanImages(string html)
        {
            string[] allowedAttributes = UmbracoSettings.ImageAllowedAttributes.ToLower().Split(',');
            string pattern = @"<img [^>]*>";
            MatchCollection tags =
                Regex.Matches(html + " ", pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            foreach (Match tag in tags)
            {
                if (tag.Value.ToLower().IndexOf("umbraco_macro") == -1)
                {
                    string cleanTag = "<img";
                    int orgWidth = 0, orgHeight = 0;
                    // gather all attributes
                    // TODO: This should be replaced with a general helper method - but for now we'll wanna leave umbraco.dll alone for this patch
                    Hashtable ht = new Hashtable();
                    MatchCollection m =
                        Regex.Matches(tag.Value.Replace(">", " >"),
                                      "(?<attributeName>\\S*)=\"(?<attributeValue>[^\"]*)\"|(?<attributeName>\\S*)=(?<attributeValue>[^\"|\\s]*)\\s",
                                      RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                    foreach (Match attributeSet in m)
                    {
                        ht.Add(attributeSet.Groups["attributeName"].Value.ToString().ToLower(),
                               attributeSet.Groups["attributeValue"].Value.ToString());
                        if (attributeSet.Groups["attributeName"].Value.ToString().ToLower() == "width")
                            orgWidth = int.Parse(attributeSet.Groups["attributeValue"].Value.ToString());
                        else if (attributeSet.Groups["attributeName"].Value.ToString().ToLower() == "height")
                            orgHeight = int.Parse(attributeSet.Groups["attributeValue"].Value.ToString());
                    }

                    // If rel attribute exists and if it's different from current sizing we should resize the image!
                    if (helper.FindAttribute(ht, "rel") != "")
                    {
                        int newWidth = 0, newHeight = 0;

                        // if size has changed resize image serverside
                        string[] newDims = helper.FindAttribute(ht, "rel").Split(",".ToCharArray());
                        if (newDims.Length > 0 &&
                            (newDims[0] != helper.FindAttribute(ht, "width") ||
                             newDims[1] != helper.FindAttribute(ht, "height")))
                        {
                            try
                            {
                                cleanTag += doResize(ht, out newWidth, out newHeight);
                            }
                            catch (Exception err)
                            {
                                cleanTag += " src=\"" + helper.FindAttribute(ht, "src") + "\"";
                                if (helper.FindAttribute(ht, "width") != "")
                                {
                                    cleanTag += " width=\"" + helper.FindAttribute(ht, "width") + "\"";
                                    cleanTag += " height=\"" + helper.FindAttribute(ht, "height") + "\"";
                                }
                                Log.Add(LogTypes.Error, User.GetUser(0), -1,
                                        "Error resizing image in editor: " + err.ToString());
                            }
                        } else
                        {
                            cleanTag = StripSrc(cleanTag, ht);

                            if (helper.FindAttribute(ht, "width") != "")
                            {
                                cleanTag += " width=\"" + helper.FindAttribute(ht, "width") + "\"";
                                cleanTag += " height=\"" + helper.FindAttribute(ht, "height") + "\"";
                            }
                            
                        }
                    }
                    else
                    {
                        // Add src, width and height properties
                        cleanTag = StripSrc(cleanTag, ht);


                        if (helper.FindAttribute(ht, "width") != "")
                        {
                            cleanTag += " width=\"" + helper.FindAttribute(ht, "width") + "\"";
                            cleanTag += " height=\"" + helper.FindAttribute(ht, "height") + "\"";
                        }

                    }


                    // Build image tag
                    foreach (string attr in allowedAttributes)
                    {
                        if (helper.FindAttribute(ht, attr) != "")
                        {
                            string attrValue = helper.FindAttribute(ht, attr);
                            cleanTag += " " + attr + "=\"" + attrValue + "\"";
                        }
                    }


                    if (bool.Parse(GlobalSettings.EditXhtmlMode))
                        cleanTag += "/";
                    cleanTag += ">";
                    html = html.Replace(tag.Value, cleanTag);
                }
            }

            return html;
        }

        private static string StripSrc(string cleanTag, Hashtable ht)
        {
            string src = helper.FindAttribute(ht, "src");
            // update orgSrc to remove umbraco reference
            if (src.IndexOf("/media/") > -1)
                src = src.Substring(src.IndexOf("/media/"), src.Length - src.IndexOf("/media/"));

            cleanTag += " src=\"" + src + "\"";
            return cleanTag;
        }

        private static string doResize(Hashtable attributes, out int finalWidth, out int finalHeight)
        {
            string resizeDim = helper.FindAttribute(attributes, "width") + "," +
                               helper.FindAttribute(attributes, "height");
            string[] resizeDimSplit = resizeDim.Split(',');
            string orgSrc = HttpContext.Current.Server.HtmlDecode(helper.FindAttribute(attributes, "src").Replace("%20", " "));
            string[] orgDim = helper.FindAttribute(attributes, "rel").Split(",".ToCharArray());
            float orgWidth = float.Parse(orgDim[0]);
            float orgHeight = float.Parse(orgDim[1]);
            string newSrc = "";
            int newWidth = int.Parse(resizeDimSplit[0]);
            int newHeight = int.Parse(resizeDimSplit[1]);

            if (orgHeight > 0 && orgWidth > 0 && resizeDim != "" && orgSrc != "")
            {
                // Check dimensions
                if (Math.Abs(orgWidth/newWidth) > Math.Abs(orgHeight/newHeight))
                {
                    newHeight = (int) Math.Round((float) newWidth*(orgHeight/orgWidth));
                }
                else
                {
                    newWidth = (int) Math.Round((float) newHeight*(orgWidth/orgHeight));
                }

                // update orgSrc to remove umbraco reference
                string resolvedMedia = IOHelper.ResolveUrl(SystemDirectories.Media);
                if (IOHelper.ResolveUrl(orgSrc).IndexOf(resolvedMedia) > -1)
                {
                    orgSrc = SystemDirectories.Media + orgSrc.Substring(orgSrc.IndexOf(resolvedMedia) + resolvedMedia.Length); //, orgSrc.Length - orgSrc.IndexOf(String.Format("/media/", SystemDirectories.Media)));
                }
                string ext = orgSrc.Substring(orgSrc.LastIndexOf(".") + 1, orgSrc.Length - orgSrc.LastIndexOf(".") - 1);
                newSrc = orgSrc.Replace("." + ext, "_" + newWidth.ToString() + "x" + newHeight.ToString() + ".jpg");

                string fullSrc = IOHelper.MapPath(orgSrc);
                string fullSrcNew = IOHelper.MapPath(newSrc);

                // Load original image
                Image image = Image.FromFile(fullSrc);


                // Create new image with best quality settings
                Bitmap bp = new Bitmap(newWidth, newHeight);
                Graphics g = Graphics.FromImage(bp);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // Copy the old image to the new and resized
                Rectangle rect = new Rectangle(0, 0, newWidth, newHeight);
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
                bp.Save(fullSrcNew, codec, ep);
            }

            // return the new width and height
            finalWidth = newWidth;
            finalHeight = newHeight;

            return
                " src=\"" + newSrc + "\"  width=\"" + newWidth.ToString() + "\"  height=\"" + newHeight.ToString() +
                "\"";
        }
    }
}