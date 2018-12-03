using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Core.Media;
using System.IO;
using Umbraco.Core.IO;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// A controller used for the embed dialog
    /// </summary>
    [PluginController("UmbracoApi")]
    public class RteEmbedController : UmbracoAuthorizedJsonController
    {
        public Result GetEmbed(string url, int width, int height)
        {
            var result = new Result();

            //todo cache embed doc
            var xmlConfig = new XmlDocument();
            xmlConfig.Load(IOHelper.GetRootDirectorySafe() + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "EmbeddedMedia.config");

            foreach (XmlNode node in xmlConfig.SelectNodes("//provider"))
            {
                var regexPattern = new Regex(node.SelectSingleNode("./urlShemeRegex").InnerText, RegexOptions.IgnoreCase);

                if (regexPattern.IsMatch(url))
                {
                    var prov = (IEmbedProvider)Activator.CreateInstance(Type.GetType(node.Attributes["type"].Value));

                    if (node.Attributes["supportsDimensions"] != null)
                        result.SupportsDimensions = node.Attributes["supportsDimensions"].Value == "True";
                    else
                        result.SupportsDimensions = prov.SupportsDimensions;

                    var settings = node.ChildNodes.Cast<XmlNode>().ToDictionary(settingNode => settingNode.Name);

                    foreach (var prop in prov.GetType().GetProperties().Where(prop => prop.IsDefined(typeof(ProviderSetting), true)))
                    {

                        if (settings.Any(s => s.Key.ToLower() == prop.Name.ToLower()))
                        {
                            var setting = settings.FirstOrDefault(s => s.Key.ToLower() == prop.Name.ToLower()).Value;
                            var settingType = typeof(Media.EmbedProviders.Settings.String);

                            if (setting.Attributes["type"] != null)
                                settingType = Type.GetType(setting.Attributes["type"].Value);

                            var settingProv = (IEmbedSettingProvider)Activator.CreateInstance(settingType);
                            prop.SetValue(prov, settingProv.GetSetting(settings.FirstOrDefault(s => s.Key.ToLower() == prop.Name.ToLower()).Value), null);
                        }
                    }
                    try
                    {
                        result.Markup = prov.GetMarkup(url, width, height);
                        result.Status = Status.Success;
                    }
                    catch(Exception ex)
                    {
                        Logger.Error<RteEmbedController>(ex, "Error embedding url {Url} - width: {Width} height: {Height}", url, width, height);
                        result.Status = Status.Error;
                    }

                    return result;
                }
            }

            result.Status = Status.NotSupported;
            return result;
        }
    }
}
