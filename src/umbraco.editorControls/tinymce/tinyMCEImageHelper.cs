using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic.Files;

namespace umbraco.editorControls.tinymce
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    internal class tinyMCEImageHelper
    {
        public static string cleanImages(string html)
        {
            var allowedAttributes = UmbracoConfig.For.UmbracoSettings().Content.ImageTagAllowedAttributes.Select(x => x.ToLower()).ToList();
            
            //Always add src as it's essential to output any image at all
            if (allowedAttributes.Contains("src") == false)
                allowedAttributes.Add("src");
            
            const string pattern = @"<img [^>]*>";
            var tags = Regex.Matches(html + " ", pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            foreach (Match tag in tags)
            {
                if (tag.Value.ToLower().IndexOf("umbraco_macro", StringComparison.Ordinal) == -1)
                {
                    var cleanTag = "<img";
                    // gather all attributes
                    // TODO: This should be replaced with a general helper method - but for now we'll wanna leave umbraco.dll alone for this patch
                    var ht = new Hashtable();
                    var matches = Regex.Matches(tag.Value.Replace(">", " >"), "(?<attributeName>\\S*?)=\"(?<attributeValue>[^\"]*)\"|(?<attributeName>\\S*?)=(?<attributeValue>[^\"|\\s]*)\\s", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                    foreach (Match attributeSet in matches)
                    {
                        ht.Add(attributeSet.Groups["attributeName"].Value.ToLower(),
                               attributeSet.Groups["attributeValue"].Value);
                    }

                    // If rel attribute exists and if it's different from current sizing we should resize the image!
                    if (helper.FindAttribute(ht, "rel") != "")
                    {
                        // if size has changed resize image serverside
                        var newDims = helper.FindAttribute(ht, "rel").Split(",".ToCharArray());
                        if (newDims.Length > 0 && (newDims[0] != helper.FindAttribute(ht, "width") || newDims[1] != helper.FindAttribute(ht, "height")))
                        {
                            try
                            {
                                int newWidth;
                                int newHeight;
                                string newSrc;

                                cleanTag += DoResize(ht, out newWidth, out newHeight, out newSrc);

                                ht["src"] = newSrc;
                            }
                            catch (Exception err)
                            {
                                cleanTag += " src=\"" + helper.FindAttribute(ht, "src") + "\"";
                                if (helper.FindAttribute(ht, "width") != "")
                                {
                                    cleanTag += " width=\"" + helper.FindAttribute(ht, "width") + "\"";
                                    cleanTag += " height=\"" + helper.FindAttribute(ht, "height") + "\"";
                                }
								LogHelper.Error<tinyMCEImageHelper>("Error resizing image in editor", err);
                            }
                        }
                        else
                        {
                            if (helper.FindAttribute(ht, "width") != "")
                            {
                                cleanTag += " width=\"" + helper.FindAttribute(ht, "width") + "\"";
                                cleanTag += " height=\"" + helper.FindAttribute(ht, "height") + "\"";
                            }

                        }
                    }
                    else
                    {
                        if (helper.FindAttribute(ht, "width") != "")
                        {
                            cleanTag += " width=\"" + helper.FindAttribute(ht, "width") + "\"";
                            cleanTag += " height=\"" + helper.FindAttribute(ht, "height") + "\"";
                        }
                    }

                    // Build image tag
                    foreach (var attr in allowedAttributes)
                    {
                        if (helper.FindAttribute(ht, attr) != "")
                        {
                            var attrValue = helper.FindAttribute(ht, attr);
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

        private static string DoResize(IDictionary attributes, out int finalWidth, out int finalHeight, out string newSrc)
        {
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            var orgSrc = HttpContext.Current.Server.HtmlDecode(helper.FindAttribute(attributes, "src").Replace("%20", " "));
            var orgDim = helper.FindAttribute(attributes, "rel").Split(",".ToCharArray());
            var orgWidth = float.Parse(orgDim[0]);
            var orgHeight = float.Parse(orgDim[1]);
            var newWidth = int.Parse(helper.FindAttribute(attributes, "width"));
            var newHeight = int.Parse(helper.FindAttribute(attributes, "height"));

            newSrc = "";

            if (orgHeight > 0 && orgWidth > 0 && orgSrc != "")
            {
                // Check dimensions
                if (Math.Abs(orgWidth / newWidth) > Math.Abs(orgHeight / newHeight))
                {
                    newHeight = (int)Math.Round(newWidth * (orgHeight / orgWidth));
                }
                else
                {
                    newWidth = (int)Math.Round(newHeight * (orgWidth / orgHeight));
                }

                var orgPath = fs.GetRelativePath(orgSrc);
                if (fs.FileExists(orgPath))
                {
                    var uf = new UmbracoFile(orgPath);

                    try
                    {
                        newSrc = uf.Resize(newWidth, newHeight);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<tinyMCEImageHelper>(string.Format("The file {0} could not be resized, reverting the image src attribute to the original source: {1}", orgPath, orgSrc), ex);
                        newSrc = orgSrc;
                    }
                }
                else
                {
                    LogHelper.Warn<tinyMCEImageHelper>(string.Format("The file {0} does not exist, reverting the image src attribute to the original source: {1}", orgPath, orgSrc));
                    newSrc = orgSrc;
                }
            }

            finalWidth = newWidth;
            finalHeight = newHeight;

            return " width=\"" + newWidth + "\"  height=\"" + newHeight + "\"";
        }
    }
}